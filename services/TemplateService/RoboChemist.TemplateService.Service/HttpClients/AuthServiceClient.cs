using Microsoft.AspNetCore.Http;
using RoboChemist.Shared.DTOs.Common;
using RoboChemist.Shared.DTOs.UserDTOs;
using System.Text.Json;

namespace RoboChemist.TemplateService.Service.HttpClients
{
    public class AuthServiceClient : IAuthServiceClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthServiceClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

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

        public async Task<UserDto?> GetCurrentUserAsync()
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiGateway");
                AuthorizeHttpClient(httpClient);

                var url = "/auth/v1/users/me";
                var response = await httpClient.GetAsync(url);

                if (response == null || !response.IsSuccessStatusCode)
                {
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<UserDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                //Return null if response is null or indicates failure
                if (apiResponse == null || !apiResponse.Success)
                {
                    return null;
                }

                //Return the user data
                return apiResponse.Data;
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException($"Failed to get current user", ex);
            }
        }
    }
}
