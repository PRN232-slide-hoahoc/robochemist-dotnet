using RoboChemist.Shared.DTOs.Common;
using RoboChemist.WalletService.Model.Entities;
using RoboChemist.WalletService.Repository.Implements;
using RoboChemist.WalletService.Service.Interfaces;
using static RoboChemist.Shared.Common.Constants.RoboChemistConstants;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.UserWalletDTOs;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.WalletTransactionDTOs;

namespace RoboChemist.WalletService.Service.Implements
{
    public class WalletService : IWalletService
    {
        private readonly UnitOfWork _unitOfWork;
        public WalletService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<UserWalletDto>> GenerateWalletAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return ApiResponse<UserWalletDto>.ErrorResult("Người dùng không hợp lệ");
            }

            var wallet = await _unitOfWork.UserWalletRepo.GetWalletByUserIdAsync(userId);
            if (wallet == null)
            {
                UserWallet newWallet = new UserWallet
                {
                    UserId = userId,
                    Balance = 0,
                    UpdateAt = DateTime.Now,
                };
                await _unitOfWork.UserWalletRepo.CreateAsync(newWallet);

                var walletDto = new UserWalletDto
                {
                    WalletId = newWallet.WalletId,
                    UserId = newWallet.UserId,
                    Balance = newWallet.Balance,
                    UpdateAt = newWallet.UpdateAt
                };

                return ApiResponse<UserWalletDto>.SuccessResult(walletDto, "Tạo ví thành công");
            }
            else
            {
                return ApiResponse<UserWalletDto>.ErrorResult("Ví đã tồn tại");
            }


            
        }

        public async Task<ApiResponse<UserWalletDto>> GetWalletByUserIdAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return ApiResponse<UserWalletDto>.ErrorResult("Người dùng không hợp lệ");
            }

            var wallet = await _unitOfWork.UserWalletRepo.GetWalletByUserIdAsync(userId);
            if (wallet == null)
            {
                return ApiResponse<UserWalletDto>.ErrorResult("Ví không tồn tại");
            }
            var walletDto = new UserWalletDto
            {
                WalletId = wallet.WalletId,
                UserId = wallet.UserId,
                Balance = wallet.Balance,
                UpdateAt = wallet.UpdateAt
            };
            return ApiResponse<UserWalletDto>.SuccessResult(walletDto, "Lấy ví thành công");
        }

        public async Task<ApiResponse<UserWalletDto>> UpdateWalletBalanceAsync(UpdateBalanceRequest request)
        {
            var wallet = await _unitOfWork.UserWalletRepo.GetByIdAsync(request.WalletId);
            if (wallet == null)
            {
                return ApiResponse<UserWalletDto>.ErrorResult("Ví không tồn tại");
            }
            if (request.TypeUpdate == UPDATE_BALANCE_TYPE_ADD)
            {
                wallet.Balance += request.Amount;
            }
            else if (request.TypeUpdate == UPDATE_BALANCE_TYPE_SUBTRACT)
            {
                if (wallet.Balance < request.Amount)
                {
                    return ApiResponse<UserWalletDto>.ErrorResult("Số dư ví không đủ");
                }
                wallet.Balance -= request.Amount;
            }
            else
            {
                return ApiResponse<UserWalletDto>.ErrorResult("Loại cập nhật số dư không hợp lệ");
            }

            wallet.UpdateAt = DateTime.Now;
            await _unitOfWork.UserWalletRepo.UpdateAsync(wallet);
            var walletDto = new UserWalletDto
            {
                WalletId = wallet.WalletId,
                UserId = wallet.UserId,
                Balance = wallet.Balance,
                UpdateAt = wallet.UpdateAt
            };
            return ApiResponse<UserWalletDto>.SuccessResult(walletDto, "Cập nhật số dư ví thành công");
        }

        public async Task<ApiResponse<PaymentResponseDto>> CreatePaymentAsync(CreatePaymentDto request)
        {
            var wallet = await _unitOfWork.UserWalletRepo.GetWalletByUserIdAsync(request.UserId);
            if (wallet == null)
            {
                return ApiResponse<PaymentResponseDto>.ErrorResult("Ví không tồn tại");
            }

            if (wallet.Balance < request.Amount)
            {
                return ApiResponse<PaymentResponseDto>.ErrorResult("Số dư ví không đủ");
            }

            var existingTransaction = await _unitOfWork.WalletTransactionRepo.GetPaymentByReferenceIdAsync(request.ReferenceId);
            if (existingTransaction != null)
            {
                return ApiResponse<PaymentResponseDto>.ErrorResult("Giao dịch đã tồn tại với ReferenceId này");
            }

            wallet.Balance -= request.Amount;
            wallet.UpdateAt = DateTime.Now;
            await _unitOfWork.UserWalletRepo.UpdateAsync(wallet);

            var transaction = new WalletTransaction
            {
                WalletId = wallet.WalletId,
                TransactionType = TRANSACTION_TYPE_PAYMENT,
                Amount = request.Amount,
                Method = TRANSACTION_METHOD_WALLET,
                Status = TRANSACTION_STATUS_COMPLETED,
                ReferenceId = request.ReferenceId,
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now
            };
            await _unitOfWork.WalletTransactionRepo.CreateAsync(transaction);

            var response = new PaymentResponseDto
            {
                TransactionId = transaction.TransactionId,
                WalletId = wallet.WalletId,
                UserId = request.UserId,
                Amount = request.Amount,
                NewBalance = wallet.Balance,
                TransactionType = TRANSACTION_TYPE_PAYMENT,
                Method = TRANSACTION_METHOD_WALLET,
                Status = TRANSACTION_STATUS_COMPLETED,
                ReferenceId = request.ReferenceId,
                ReferenceType = request.ReferenceType,
                CreateAt = transaction.CreateAt.Value
            };

            return ApiResponse<PaymentResponseDto>.SuccessResult(response, "Thanh toán thành công");
        }

        public async Task<ApiResponse<RefundResponseDto>> RefundPaymentAsync(RefundRequestDto request)
        {
            var originalTransaction = await _unitOfWork.WalletTransactionRepo.GetPaymentByReferenceIdAsync(request.ReferenceId);
            if (originalTransaction == null)
            {
                return ApiResponse<RefundResponseDto>.ErrorResult("Không tìm thấy giao dịch gốc");
            }

            if (originalTransaction.Status != TRANSACTION_STATUS_COMPLETED)
            {
                return ApiResponse<RefundResponseDto>.ErrorResult("Chỉ có thể hoàn tiền cho giao dịch đã hoàn thành");
            }

            var existingRefund = await _unitOfWork.WalletTransactionRepo
                .GetByReferenceIdAsync(request.ReferenceId);
            
            if (existingRefund.Any(t => t.TransactionType == TRANSACTION_TYPE_REFUND))
            {
                return ApiResponse<RefundResponseDto>.ErrorResult("Giao dịch này đã được hoàn tiền");
            }

            var wallet = await _unitOfWork.UserWalletRepo.GetByIdAsync(originalTransaction.WalletId);
            if (wallet == null)
            {
                return ApiResponse<RefundResponseDto>.ErrorResult("Ví không tồn tại");
            }

            wallet.Balance += originalTransaction.Amount;
            wallet.UpdateAt = DateTime.Now;
            await _unitOfWork.UserWalletRepo.UpdateAsync(wallet);

            var refundTransaction = new WalletTransaction
            {
                WalletId = wallet.WalletId,
                TransactionType = TRANSACTION_TYPE_REFUND,
                Amount = originalTransaction.Amount,
                Method = TRANSACTION_METHOD_WALLET,
                Status = TRANSACTION_STATUS_COMPLETED,
                ReferenceId = request.ReferenceId,
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now
            };
            await _unitOfWork.WalletTransactionRepo.CreateAsync(refundTransaction);

            var response = new RefundResponseDto
            {
                RefundTransactionId = refundTransaction.TransactionId,
                OriginalTransactionId = originalTransaction.TransactionId,
                WalletId = wallet.WalletId,
                UserId = wallet.UserId,
                Amount = originalTransaction.Amount,
                NewBalance = wallet.Balance,
                TransactionType = TRANSACTION_TYPE_REFUND,
                Status = TRANSACTION_STATUS_COMPLETED,
                ReferenceId = request.ReferenceId,
                CreateAt = refundTransaction.CreateAt.Value
            };

            return ApiResponse<RefundResponseDto>.SuccessResult(response, "Hoàn tiền thành công");
        }

        public async Task<ApiResponse<WalletBalanceDto>> GetBalanceAsync(Guid userId)
        {
            var wallet = await _unitOfWork.UserWalletRepo.GetWalletByUserIdAsync(userId);
            if (wallet == null)
            {
                return ApiResponse<WalletBalanceDto>.ErrorResult("Ví không tồn tại");
            }

            var balanceDto = new WalletBalanceDto
            {
                UserId = wallet.UserId,
                WalletId = wallet.WalletId,
                Balance = wallet.Balance,
                UpdateAt = wallet.UpdateAt
            };

            return ApiResponse<WalletBalanceDto>.SuccessResult(balanceDto, "Lấy số dư thành công");
        }

        public async Task<ApiResponse<TransactionsByReferenceDto>> GetTransactionsByReferenceAsync(Guid referenceId)
        {
            var transactions = await _unitOfWork.WalletTransactionRepo.GetByReferenceIdAsync(referenceId);
            
            var transactionDtos = transactions.Select(t => new WalletTransactionDto
            {
                TransactionId = t.TransactionId,
                WalletId = t.WalletId,
                TransactionType = t.TransactionType,
                Amount = t.Amount,
                Method = t.Method,
                Status = t.Status,
                ReferenceId = t.ReferenceId,
                CreateAt = t.CreateAt,
                UpdateAt = t.UpdateAt
            }).ToList();

            var totalPayment = transactions
                .Where(t => t.TransactionType == TRANSACTION_TYPE_PAYMENT)
                .Sum(t => t.Amount);

            var totalRefund = transactions
                .Where(t => t.TransactionType == TRANSACTION_TYPE_REFUND)
                .Sum(t => t.Amount);

            var response = new TransactionsByReferenceDto
            {
                ReferenceId = referenceId,
                Transactions = transactionDtos,
                TotalPayment = totalPayment,
                TotalRefund = totalRefund,
                NetAmount = totalPayment - totalRefund
            };

            return ApiResponse<TransactionsByReferenceDto>.SuccessResult(response, "Lấy giao dịch thành công");
        }
    }
}
