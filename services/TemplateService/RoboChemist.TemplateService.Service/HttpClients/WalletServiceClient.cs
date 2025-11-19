using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RoboChemist.Shared.DTOs.Common;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.WalletTransactionDTOs;

namespace RoboChemist.TemplateService.Service.HttpClients;

/// <summary>
/// HTTP client for WalletService via ApiGateway
/// </summary>
public class WalletServiceClient : IWalletServiceClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WalletServiceClient> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WalletServiceClient(
        IHttpClientFactory httpClientFactory,
        ILogger<WalletServiceClient> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ApiResponse<PaymentResponseDto>?> CreatePaymentAsync(CreatePaymentDto request)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("ApiGateway");
            
            // Forward JWT token from current request
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Remove("Authorization");
                httpClient.DefaultRequestHeaders.Add("Authorization", token);
            }

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("/wallet/v1/wallet/payment", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"WalletService payment failed: {response.StatusCode} - {responseContent}");
                return ApiResponse<PaymentResponseDto>.ErrorResult($"Payment failed: {responseContent}");
            }

            var result = JsonSerializer.Deserialize<ApiResponse<PaymentResponseDto>>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling WalletService payment");
            return ApiResponse<PaymentResponseDto>.ErrorResult($"Payment service error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<RefundResponseDto>?> RefundAsync(RefundRequestDto request)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("ApiGateway");
            
            // Forward JWT token from current request
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Remove("Authorization");
                httpClient.DefaultRequestHeaders.Add("Authorization", token);
            }

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("/wallet/v1/wallet/refund", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"WalletService refund failed: {response.StatusCode} - {responseContent}");
                return ApiResponse<RefundResponseDto>.ErrorResult($"Refund failed: {responseContent}");
            }

            var result = JsonSerializer.Deserialize<ApiResponse<RefundResponseDto>>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling WalletService refund");
            return ApiResponse<RefundResponseDto>.ErrorResult($"Refund service error: {ex.Message}");
        }
    }
}
