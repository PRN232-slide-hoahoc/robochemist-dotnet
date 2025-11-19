using RoboChemist.Shared.DTOs.UserDTOs;

namespace RoboChemist.WalletService.Service.HttpClients
{
    public interface IAuthServiceClient
    {
        Task<UserDto?> GetCurrentUserAsync();
    }
}
