using RoboChemist.Shared.DTOs.UserDTOs;

namespace RoboChemist.ExamService.Service.HttpClients
{
    /// <summary>
    /// Interface for Auth Service HTTP client
    /// Provides contract for authentication and user information retrieval
    /// </summary>
    public interface IAuthServiceClient
    {
        /// <summary>
        /// Get current authenticated user information
        /// </summary>
        /// <returns>Current user details or null if not authenticated</returns>
        Task<UserDto?> GetCurrentUserAsync();
    }
}
