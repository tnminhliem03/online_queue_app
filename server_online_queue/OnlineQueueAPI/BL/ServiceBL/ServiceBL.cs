using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineQueueAPI.DL;
using OnlineQueueAPI.DTOs;
using OnlineQueueAPI.Models;
using OnlineQueueAPI.Services;

namespace OnlineQueueAPI.BL
{
    public class ServiceBL : BaseBL<Service>, IServiceBL
    {
        private readonly IBaseDL<Service> _baseDL;
        private readonly IBaseDL<Organization> _organizationDL;
        private readonly IBaseBL<Queue> _queueBL;

        public ServiceBL(
                IHttpContextAccessor httpContextAccessor,
                IBaseDL<Service> baseDL,
                IMapper mapper,
                WebSocketService webSocketService,
                IBaseDL<Organization> organizationDL,
                IBaseBL<Queue> queueBL
            ) : base(httpContextAccessor, baseDL, mapper, webSocketService)
        {
            _baseDL = baseDL;
            _organizationDL = organizationDL;
            _queueBL = queueBL;
        }

        public async Task<Service> AddServiceAsync(ServiceDTO.ServiceCreateDTO addDTO)
        {
            bool orgExists = await _organizationDL
                                    .Query()
                                    .AnyAsync(o => o.Id == addDTO.OrganizationId);

            if (!orgExists) throw new InvalidDataException("Invalid Organization Id");

            var service = new Service
            {
                Name = addDTO.Name,
                Description = addDTO.Description,
                AverageDuration = addDTO.AverageDuration,
                OrganizationId = addDTO.OrganizationId,
                StartTime = addDTO.StartTime,
                EndTime = addDTO.EndTime,
            };

            var createdService = await AddAsync(service, true);

            var normalQueueDto = new QueueDTO.QueueCreateDTO
            {
                ServiceId = createdService!.Id,
                Type = QueueType.Normal,
            };

            var priorityQueueDto = new QueueDTO.QueueCreateDTO
            {
                ServiceId = createdService.Id,
                Type = QueueType.Priority,
            };

            await _queueBL.AddAsync(normalQueueDto);
            await _queueBL.AddAsync(priorityQueueDto);

            return createdService;
        }

        public async Task<Service> UpdateServiceAsync(Guid id, ServiceDTO.ServiceUpdateDTO updateDTO)
        {
            bool serviceExists = await _baseDL
                                        .Query()
                                        .AnyAsync(s => s.Id == id);

            if (!serviceExists) throw new ArgumentException("Service not found");

            var updatedService = await UpdateAsync(id, updateDTO);

            return updatedService!;
        }
    }
}
