using RoboChemist.Shared.DTOs.UserDTOs;

namespace RoboChemist.SlidesService.Service.HttpClients
{
    public interface IAuthServiceClient
    {
        Task<UserDto?> GetCurrentUserAsync();
    }
}
