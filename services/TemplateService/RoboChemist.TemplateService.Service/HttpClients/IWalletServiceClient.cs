using RoboChemist.Shared.DTOs.Common;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.WalletTransactionDTOs;

namespace RoboChemist.TemplateService.Service.HttpClients;

/// <summary>
/// HTTP client for WalletService
/// </summary>
public interface IWalletServiceClient
{
    /// <summary>
    /// Create payment transaction
    /// </summary>
    Task<ApiResponse<PaymentResponseDto>?> CreatePaymentAsync(CreatePaymentDto request);
    
    /// <summary>
    /// Refund a payment transaction
    /// </summary>
    Task<ApiResponse<RefundResponseDto>?> RefundAsync(RefundRequestDto request);
}
