using RoboChemist.Shared.DTOs.Common;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.UserWalletDTOs;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.WalletTransactionDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboChemist.WalletService.Service.Interfaces
{
    public interface IWalletService
    {
        Task<ApiResponse<UserWalletDto>> GenerateWalletAsync();
        Task<ApiResponse<UserWalletDto>> GetWalletByUserIdAsync();
        Task<ApiResponse<UserWalletDto>> UpdateWalletBalanceAsync(UpdateBalanceRequest request);
        Task<ApiResponse<PaymentResponseDto>> CreatePaymentAsync(CreatePaymentDto request);
        Task<ApiResponse<RefundResponseDto>> RefundPaymentAsync(RefundRequestDto request);
        Task<ApiResponse<WalletBalanceDto>> GetBalanceAsync();
        Task<ApiResponse<TransactionsByReferenceDto>> GetTransactionsByReferenceAsync(Guid referenceId);
    }
}
