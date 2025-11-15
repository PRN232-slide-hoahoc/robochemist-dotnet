using RoboChemist.Shared.DTOs.Common;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.WalletTransactionDTOs;

namespace RoboChemist.SlidesService.Service.HttpClients
{
    public interface IWalletServiceClient
    {
        Task<ApiResponse<PaymentResponseDto>?> CreatePaymentAsync(CreatePaymentDto request);
    }
}
