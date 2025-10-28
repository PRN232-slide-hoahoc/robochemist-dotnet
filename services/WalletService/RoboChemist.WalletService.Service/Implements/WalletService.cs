using RoboChemist.Shared.DTOs.Common;
using RoboChemist.WalletService.Model.Entities;
using RoboChemist.WalletService.Repository.Implements;
using RoboChemist.WalletService.Service.Interfaces;
using static RoboChemist.Shared.Common.Constants.RoboChemistConstants;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.UserWalletDTOs;

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

            if (userId == null)
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
            if (userId == null)
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
    }
}
