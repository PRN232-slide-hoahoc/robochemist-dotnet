using static RoboChemist.Shared.DTOs.WalletServiceDTOs.WalletTransactionDTOs;

namespace RoboChemist.ExamService.Service.HttpClients
{
    public interface IWalletServiceHttpClient
    {
        Task<PaymentResponseDto?> CreatePaymentAsync(CreatePaymentDto request, string? authToken = null);
        Task<RefundResponseDto?> RefundPaymentAsync(RefundRequestDto request, string? authToken = null);
    }
}
