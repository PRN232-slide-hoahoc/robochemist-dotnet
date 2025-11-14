using System.Text;
using System.Text.Json;
using RoboChemist.Shared.DTOs.Common;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.WalletTransactionDTOs;

namespace RoboChemist.ExamService.Service.HttpClients
{
    public class WalletServiceHttpClient : IWalletServiceHttpClient
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public WalletServiceHttpClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<PaymentResponseDto?> CreatePaymentAsync(CreatePaymentDto request, string? authToken = null)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiGateway");

                if (!string.IsNullOrEmpty(authToken))
                {
                    httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
                }

                var url = "/wallet/v1/wallets/payment";
                Console.WriteLine($"[SAGA] WalletService.CreatePayment: {httpClient.BaseAddress}{url}");
                Console.WriteLine($"[SAGA] Payment Request JSON: {JsonSerializer.Serialize(request)}");

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[SAGA FAILED] Payment failed: {response.StatusCode} - {errorContent}");
                    return null;
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<PaymentResponseDto>>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (apiResponse?.Success == true)
                {
                    Console.WriteLine($"[SAGA SUCCESS] Payment completed: TransactionId={apiResponse.Data?.TransactionId}");
                    return apiResponse.Data;
                }

                Console.WriteLine($"[SAGA FAILED] Payment business logic failed: {apiResponse?.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SAGA ERROR] CreatePaymentAsync exception: {ex.Message}");
                return null;
            }
        }

        public async Task<RefundResponseDto?> RefundPaymentAsync(RefundRequestDto request, string? authToken = null)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiGateway");

                if (!string.IsNullOrEmpty(authToken))
                {
                    httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
                }

                var url = "/wallet/v1/wallets/refund";
                Console.WriteLine($"[SAGA COMPENSATE] WalletService.RefundPayment: {httpClient.BaseAddress}{url}");

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[SAGA COMPENSATE FAILED] Refund failed: {response.StatusCode} - {errorContent}");
                    return null;
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<RefundResponseDto>>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (apiResponse?.Success == true)
                {
                    Console.WriteLine($"[SAGA COMPENSATE SUCCESS] Refund completed: RefundTransactionId={apiResponse.Data?.RefundTransactionId}");
                    return apiResponse.Data;
                }

                Console.WriteLine($"[SAGA COMPENSATE FAILED] Refund business logic failed: {apiResponse?.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SAGA COMPENSATE ERROR] RefundPaymentAsync exception: {ex.Message}");
                return null;
            }
        }
    }
}
