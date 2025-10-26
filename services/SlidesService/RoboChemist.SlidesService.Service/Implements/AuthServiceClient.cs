using RoboChemist.Shared.DTOs.Common;
using RoboChemist.Shared.DTOs.UserDTOs;
using RoboChemist.SlidesService.Service.Interfaces;
using System.Net.Http.Json;

namespace RoboChemist.SlidesService.Service.Implements
{
    public class AuthServiceClient : IAuthServiceClient
    {
        private readonly HttpClient _httpClient;
        private const string _baseAuthV1Endpoint = "/auth/v1";

        public AuthServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<UserDto?> GetCurrentUserAsync()
        {
            try
            {
                //Hardcode đợi cái auth chạy được 🙏🙏🙏
                return new UserDto { Id = Guid.Parse("11111111-1111-1111-1111-111111111111") };

                //ApiResponse<UserDto>? response = await _httpClient.GetFromJsonAsync<ApiResponse<UserDto>>($"{_baseAuthV1Endpoint}/me");

                ////Return null if response is null or indicates failure
                //if (response == null || !response.Success)
                //{
                //    return null;
                //}

                ////Return the user data
                //return response.Data;
            }
            catch (HttpRequestException ex)
            {
                // Log the error here if you have ILogger injected
                throw new HttpRequestException($"Failed to get current user", ex);
            }
        }
    }
}
