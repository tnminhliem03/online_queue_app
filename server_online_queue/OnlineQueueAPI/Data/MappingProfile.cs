using AutoMapper;
using OnlineQueueAPI.DTOs;
using OnlineQueueAPI.Models;

namespace OnlineQueueAPI.Data
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<AccountDTO.AccountRegisterDTO, User>()
                .ForMember(dest => dest.Password, opt => opt.Ignore());
            CreateMap<AccountDTO.AccountUpdateDTO, User>();
            CreateMap<AccountDTO.AccountChangePasswordDTO, User>();
            CreateMap<AccountDTO.RefreshTokenDTO, User>();
            CreateMap<FieldDTO, Field>();
            CreateMap<OrganizationDTO.OrgCreateDTO, Organization>();
            CreateMap<OrganizationDTO.OrgUpdateDTO, Organization>();
            CreateMap<OrganizationDTO.OrgUpdateStatusDTO, Organization>();
            CreateMap<ServiceDTO.ServiceCreateDTO, Service>();
            CreateMap<ServiceDTO.ServiceUpdateDTO, Service>();
            CreateMap<QueueDTO.QueueUpdateDTO, Queue>();
            CreateMap<QueueDTO.QueueCreateDTO, Queue>();
            CreateMap<AppointmentDTO.AppointmentCreateDTO, Appointment>();
            CreateMap<AppointmentDTO.AppointmentUpdateDTO, Appointment>();
            CreateMap<NotificationDTO, Notification>();
        }
    }
}
