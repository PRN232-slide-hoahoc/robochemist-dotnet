using RoboChemist.Shared.DTOs.Common;
using RoboChemist.Shared.DTOs.UserDTOs;

namespace RoboChemist.SlidesService.Service.Interfaces
{
    public interface IAuthServiceClient
    {
        /// <summary>
        /// Get current user from Auth Service via API Gateway
        /// </summary>
        /// <returns>User information</returns>
        Task<UserDto?> GetCurrentUserAsync();
    }
}
