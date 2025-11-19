using Microsoft.AspNetCore.Http;
using RoboChemist.Shared.DTOs.Common;
using System.Text;
using System.Text.Json;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.WalletTransactionDTOs;

namespace RoboChemist.SlidesService.Service.HttpClients
{
    public class WalletServiceClient : IWalletServiceClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WalletServiceClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
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
        public async Task<ApiResponse<PaymentResponseDto>?> CreatePaymentAsync(CreatePaymentDto request)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("WalletService");

                AuthorizeHttpClient(httpClient);

                var url = "/api/v1/wallet/payment";

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(url, content);

                var responseJson = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<PaymentResponseDto>>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return apiResponse;
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException($"Failed to get current user", ex);
            }
        }
    }
}
