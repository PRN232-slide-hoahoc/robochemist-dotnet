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
        Task<ApiResponse<CreatePaymentRequestDto>> CreatePaymentRequestAsync(CreatePaymentRequestDto paymentRequestDTO);
        Task<ApiResponse<CreatePaymentRequestDto>> CreateRefundRequestAsync(CreatePaymentRequestDto paymentRequestDTO);
        Task<ApiResponse<List<WalletTransactionDto>>> GetAllTransactionAsync();
    }
}
