using OnlineQueueAPI.DTOs;
using OnlineQueueAPI.Models;

namespace OnlineQueueAPI.BL
{
    public interface IServiceBL : IBaseBL<Service>
    {
        Task<Service> AddServiceAsync(ServiceDTO.ServiceCreateDTO addDTO);
        
        Task<Service> UpdateServiceAsync(Guid id, ServiceDTO.ServiceUpdateDTO updateDTO);

    }
}
