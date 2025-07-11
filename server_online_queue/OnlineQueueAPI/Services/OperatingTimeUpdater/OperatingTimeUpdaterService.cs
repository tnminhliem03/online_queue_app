using OnlineQueueAPI.BL;
using OnlineQueueAPI.Models;

namespace OnlineQueueAPI.Services
{
    public class OperatingTimeUpdaterService : IOperatingTimeUpdaterService
    {
        private readonly IServiceBL _serviceBL;
        private readonly IOrganizationBL _organizationBL;

        public OperatingTimeUpdaterService(IServiceBL serviceBL, IOrganizationBL organizationBL)
        {
            _serviceBL = serviceBL;
            _organizationBL = organizationBL;
        }

        public async Task UpdateStatusesAsync()
        {
            var now = DateTime.UtcNow;
            var currentTime = now.TimeOfDay;

            var services = await _serviceBL.GetAllAsync();
            foreach (var service in services)
            {
                service.Status = (currentTime >= service.StartTime && currentTime <= service.EndTime) ?
                   ServiceStatus.Opened :
                   ServiceStatus.Closed;

                await _serviceBL.UpdateAsync(service.Id, service);
            }

            var organizations = await _organizationBL.GetAllAsync();
            foreach (var organization in organizations)
            {
                organization.Status = (currentTime >= organization.StartTime && currentTime <= organization.EndTime) ?
                   OrganizationStatus.Opened :
                   OrganizationStatus.Closed;

                await _organizationBL.UpdateAsync(organization.Id, organization);
            }
        }
    }
}