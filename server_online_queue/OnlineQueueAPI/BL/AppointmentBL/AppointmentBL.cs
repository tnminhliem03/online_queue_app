using AutoMapper;
using OnlineQueueAPI.DL;
using OnlineQueueAPI.DTOs;
using OnlineQueueAPI.Models;
using OnlineQueueAPI.Services;
using System.Linq.Expressions;

namespace OnlineQueueAPI.BL
{
    public class AppointmentBL : BaseBL<Appointment>, IAppointmentBL
    {
        private readonly IBaseBL<Appointment> _baseBL;
        private readonly IAppointmentDL _appointmentDL;
        private readonly IBaseDL<Service> _serviceDL;
        private readonly IBaseDL<Organization> _organizationDL;
        private readonly IQueueDL _queueDL;
        private readonly WebSocketService _webSocketService;

        public AppointmentBL(
                IHttpContextAccessor httpContextAccessor,
                IBaseDL<Appointment> baseDL,
                IMapper mapper,
                WebSocketService webSocketService,
                IBaseBL<Appointment> baseBL,
                IAppointmentDL appointmentDL,
                IQueueDL queueDL,
                IBaseDL<Service> serviceDL,
                IBaseDL<Organization> organizationDL
            ) : base(httpContextAccessor, baseDL, mapper, webSocketService)
        {
            _appointmentDL = appointmentDL;
            _queueDL = queueDL;
            _serviceDL = serviceDL;
            _baseBL = baseBL;
            _organizationDL = organizationDL;
            _webSocketService = webSocketService;
        }

        public async Task<(
                IEnumerable<Appointment> Data,
                int TotalRecords,
                int CurrentPage,
                int TotalPages
            )> GetAppointmentsForServicePagedAsync(int pageNumber, int pageSize, Guid serviceId)
        {
            var serviceExists = await _serviceDL.GetById(serviceId);
            if (serviceExists == null) throw new ArgumentException("Service not found.");

            Expression<Func<Appointment, bool>> filter = a => a.Queue!.ServiceId == serviceId;

            var (appointments, totalRecords, currentPage, totalPages) = await _baseBL.GetPagedAsync(
                pageNumber,
                pageSize,
                filter
            );

            return (appointments, totalRecords, currentPage, totalPages);
        }

        private async Task<(DateTime expectedTime, bool isRescheduled)> CalculateExpectedTimeAsync(Guid queueId)
        {
            var queue = await _queueDL.GetQueueWithAppointmentsAsync(queueId);
            if (queue == null || queue.Service == null) throw new ArgumentException("Queue or Service not found");

            var service = queue.Service;

            var now = DateTime.UtcNow;
            var currentDate = now.Date;

            var serviceStartTime = currentDate.Add(service.StartTime);
            var serviceEndTime = currentDate.Add(service.EndTime);

            var appointments = queue.Appointments!.OrderBy(a => a.ExpectedDateTime).ToList();

            DateTime expectedTime;
            bool isRescheduled = false;

            if (!appointments.Any())
            {
                expectedTime = now < serviceStartTime ? serviceStartTime : now;

                if (expectedTime > serviceEndTime)
                {
                    isRescheduled = true;
                    expectedTime = currentDate.AddDays(1).Add(service.StartTime);
                }

                return (expectedTime, isRescheduled);
            }

            var lastAppointment = appointments.Last();
            expectedTime = lastAppointment.ExpectedDateTime.AddMinutes(service.AverageDuration);

            if (expectedTime < now)
            {
                expectedTime = now;
            }

            var estimatedEndTime = expectedTime.AddMinutes(service.AverageDuration);

            if (estimatedEndTime.TimeOfDay > service.EndTime)
            {
                isRescheduled = true;
                expectedTime = currentDate.AddDays(1).Add(service.StartTime);
            }

            return (expectedTime, isRescheduled);
        }

        public async Task<DateTime> getExpectedTimeAsync(Guid queueId)
        {
            return await CalculateExpectedTimeAsync(queueId)
                            .ContinueWith(t => t.Result.expectedTime);
        }

        public async Task<Appointment> AddAppointmentAsync(AppointmentDTO.AppointmentCreateDTO addDTO)
        {
            var userId = GetUserId();

            var hasUnfinishedAppointment = await _appointmentDL.GetAppointmentUnfinished(userId!.Value);

            if (hasUnfinishedAppointment) throw new InvalidOperationException("User already has an unfinished appointment.");

            var queue = await _queueDL.GetQueueByService(addDTO.ServiceId, addDTO.Priority);

            if (queue == null) throw new ArgumentException("Queue not found");

            var (expectedTime, isRescheduled) = await CalculateExpectedTimeAsync(queue.Id);

            var newAppointment = new Appointment
            {
                UserId = userId!.Value,
                QueueId = queue.Id,
                Priority = addDTO.Priority,
                ExpectedDateTime = expectedTime,
                Status = AppointmentStatus.Pending
            };

            var created = await AddAsync(newAppointment, true);

            return created!;
        }

        public async Task<bool> UpdateAppointmentStatusAsync(Guid appointmentId, AppointmentStatus status)
        {
            var appointment = await GetByIdAsync(appointmentId);

            appointment!.Status = status;

            await _appointmentDL.UpdateAppointment(appointment);

            await _webSocketService.SendUpdateToClients("Appointment");

            return true;
        }

        public async Task<StatisticsDTO.AppointmentStatisticsDto> GetAppointmentStatisticsAsync(DateTime date, Guid organizationId)
        {
            if (date == default || organizationId == Guid.Empty) throw new ArgumentException("Invalid date or organization ID");

            var organization = await _organizationDL.GetById(organizationId);

            if (organization == null) throw new ArgumentException("Organization not found");

            var appointmentsInDay = await _appointmentDL.GetAppointmentsInDayOfOrganization(date, organizationId);

            var statistics = new StatisticsDTO.AppointmentStatisticsDto
            {
                SelectedDate = date,
                OrganizationId = organizationId,
                TotalAppointments = appointmentsInDay.Count,
                TotalPriorityAppointments = appointmentsInDay.Count(a => a.Priority),
                TotalNormalAppointments = appointmentsInDay.Count(a => !a.Priority),
                TotalSkippedAppointments = appointmentsInDay.Count(a => a.Status == AppointmentStatus.Skipped),
                TotalServicesUsed = appointmentsInDay.Select(a => a.Queue?.ServiceId).Distinct().Count(),
                ServiceStatistics = new List<StatisticsDTO.ServiceStatisticsDto>()
            };

            if (organization.Services != null)
            {
                foreach (var service in organization.Services)
                {
                    var serviceAppointments = appointmentsInDay.Where(a => a.Queue?.ServiceId == service.Id).ToList();

                    statistics.ServiceStatistics.Add(new StatisticsDTO.ServiceStatisticsDto
                    {
                        ServiceId = service.Id,
                        ServiceName = service.Name,
                        TotalAppointments = serviceAppointments.Count,
                        TotalPriorityAppointments = serviceAppointments.Count(a => a.Priority),
                        TotalNormalAppointments = serviceAppointments.Count(a => !a.Priority),
                        TotalSkippedAppointments = serviceAppointments.Count(a => a.Status == AppointmentStatus.Skipped)
                    });
                }
            }

            return statistics;
        }
    }
}
