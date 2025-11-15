using RoboChemist.Shared.DTOs.UserDTOs;

namespace RoboChemist.TemplateService.Service.HttpClients
{
    public interface IAuthServiceClient
    {
        Task<UserDto?> GetCurrentUserAsync();
    }
}
