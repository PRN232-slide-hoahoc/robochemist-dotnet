using Microsoft.AspNetCore.Http;
using RoboChemist.Shared.DTOs.Common;
using RoboChemist.Shared.DTOs.UserDTOs;
using System.Text.Json;

namespace RoboChemist.ExamService.Service.HttpClients
{
    /// <summary>
    /// Typed HTTP client for communicating with Auth Service
    /// Handles authentication token forwarding and user information retrieval
    /// </summary>
    public class AuthServiceClient : IAuthServiceClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthServiceClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        /// <summary>
        /// Forward Authorization Bearer token from current request to HttpClient
        /// </summary>
        private void AuthorizeHttpClient(HttpClient httpClient)
        {
            var authToken = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"]
                    .ToString()
                    .Replace("Bearer ", "");

            if (!string.IsNullOrEmpty(authToken))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
            }
        }

        /// <summary>
        /// Get current authenticated user information from Auth Service
        /// </summary>
        /// <returns>User details or null if not authenticated or request fails</returns>
        public async Task<UserDto?> GetCurrentUserAsync()
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("AuthService");
                AuthorizeHttpClient(httpClient);

                var url = "/api/v1/users/me";
                var response = await httpClient.GetAsync(url);

                if (response == null || !response.IsSuccessStatusCode)
                {
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<UserDto>>(json, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                // Return null if response is null or indicates failure
                if (apiResponse == null || !apiResponse.Success)
                {
                    return null;
                }

                // Return the user data
                return apiResponse.Data;
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException($"Failed to get current user from Auth Service", ex);
            }
        }
    }
}
