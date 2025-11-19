using Microsoft.AspNetCore.Http;
using RoboChemist.Shared.DTOs.Common;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.VNPayDTOs;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.WalletTransactionDTOs;

namespace RoboChemist.WalletService.Service.Interfaces
{
    public interface IPaymentService
    {
        Task<ApiResponse<string>> CreateDepositUrlAsync(DepositRequestDTO depositRequestDTO, HttpContext httpContext);
        Task<ApiResponse<WalletTransactionDto>> DepositCallbackAsync(DepositCallbackRequestDto depositCallbackRequestDto);
        Task<ApiResponse<CreateChangeBalanceRequestDto>> CreatePaymentRequestAsync(CreateChangeBalanceRequestDto paymentRequestDTO, Guid userId);
        Task<ApiResponse<CreateChangeBalanceRequestDto>> CreateRefundRequestAsync(CreateChangeBalanceRequestDto paymentRequestDTO, Guid userId);
        Task<ApiResponse<List<WalletTransactionDto>>> GetAllTransactionAsync();
    }
}
