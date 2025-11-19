using Microsoft.AspNetCore.Http;
using RoboChemist.Shared.DTOs.Common;
using RoboChemist.Shared.DTOs.UserDTOs;
using RoboChemist.WalletService.Model.Entities;
using RoboChemist.WalletService.Repository.Implements;
using RoboChemist.WalletService.Service.HttpClients;
using RoboChemist.WalletService.Service.Interfaces;
using RoboChemist.WalletService.Service.Libraries;
using static RoboChemist.Shared.Common.Constants.RoboChemistConstants;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.UserWalletDTOs;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.VNPayDTOs;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.WalletTransactionDTOs;

namespace RoboChemist.WalletService.Service.Implements
{
    public class PaymentService : IPaymentService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly VNPayConfig _vnpayConfig;
        private readonly IWalletService _walletService;
        private readonly IAuthServiceClient _authService;
        public PaymentService(UnitOfWork unitOfWork, VNPayConfig vnpayConfig, IWalletService walletService, IAuthServiceClient authService)
        {
            _unitOfWork = unitOfWork;
            _vnpayConfig = vnpayConfig;
            _walletService = walletService;
            _authService = authService;
        }

        #region VNPay Deposit
        private string GetCallbackUrl()
        {
            // Cách lấy từ biến môi trường
            var callbackUrl = _vnpayConfig.CallbackUrl;

            // Nếu không có biến môi trường, bạn có thể lấy từ IConfiguration
            if (string.IsNullOrEmpty(callbackUrl))
            {
                callbackUrl = "http://178.128.59.172"; // Dự phòng nếu không có môi trường
            }

            return callbackUrl;
        }

        public async Task<ApiResponse<string>> CreateDepositUrlAsync(DepositRequestDTO depositRequestDTO, HttpContext httpContext)
        {
            //Get user info
            UserDto? user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                return ApiResponse<string>.ErrorResult("Người dùng không hợp lệ");
            }

            var wallet = await _unitOfWork.UserWalletRepo.GetWalletByUserIdAsync(user.Id);
            if (wallet == null)
            {
                return ApiResponse<string>.ErrorResult("Ví người dùng không tồn tại");
            }

            WalletTransaction newTransaction = new WalletTransaction
            {
                WalletId = wallet.WalletId,
                TransactionType = TRANSACTION_TYPE_DEPOSIT,
                Amount = depositRequestDTO.amount,
                Method = TRANSACTION_METHOD_VNPAY,
                Status = TRANSACTION_STATUS_PENDING,
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now
            };

            await _unitOfWork.WalletTransactionRepo.CreateAsync(newTransaction);

            VNPayLibrary pay = new VNPayLibrary();
            string tick = DateTime.Now.Ticks.ToString();
            string urlCallBack = GetCallbackUrl();

            pay.AddRequestData("vnp_Version", "2.1.0");
            pay.AddRequestData("vnp_Command", "pay");
            pay.AddRequestData("vnp_TmnCode", _vnpayConfig.TmnCode);

            pay.AddRequestData("vnp_Amount", (depositRequestDTO.amount * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", "VND");
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(httpContext));
            pay.AddRequestData("vnp_Locale", "vn");
            pay.AddRequestData("vnp_OrderInfo", $"Deposit to through VNPay with TransactionId {newTransaction.TransactionId}");
            pay.AddRequestData("vnp_OrderType", "other");
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", tick);

            string depositUrl = pay.CreateRequestUrl(_vnpayConfig.VnpayUrl, _vnpayConfig.HashSecret);

            return ApiResponse<string>.SuccessResult(depositUrl, "Tạo link nạp tiền thành công");
        }

        public async Task<ApiResponse<WalletTransactionDto>> DepositCallbackAsync(DepositCallbackRequestDto depositRequestDTO)
        {
            if (string.IsNullOrEmpty(depositRequestDTO.vnp_OrderInfo))
            {
                return ApiResponse<WalletTransactionDto>.ErrorResult("Thông tin đơn hàng không hợp lệ");
            }

            string orderInfo = depositRequestDTO.vnp_OrderInfo.Replace("+"," ");
            var orderInfoParts = orderInfo.Split("TransactionId ");
            if (orderInfoParts.Length < 2 || !Guid.TryParse(orderInfoParts[1].Trim(), out Guid transactionId))
            {
                return ApiResponse<WalletTransactionDto>.ErrorResult("Không thể parse Transaction ID từ thông tin đơn hàng");
            }

            var transaction = await _unitOfWork.WalletTransactionRepo.GetByIdAsync(transactionId);
            if (transaction == null)
            {
                return ApiResponse<WalletTransactionDto>.ErrorResult("Không tìm thấy giao dịch");
            }

            // Kiểm tra trạng thái transaction
            if (transaction.Status != TRANSACTION_STATUS_PENDING)
            {
                return ApiResponse<WalletTransactionDto>.ErrorResult($"Giao dịch đã được xử lý với trạng thái: {transaction.Status}");
            }

            // Kiểm tra số tiền khớp
            decimal vnpayAmount = depositRequestDTO.vnp_Amount / 100; // VNPay trả về số tiền nhân 100
            if (transaction.Amount != vnpayAmount)
            {
                return ApiResponse<WalletTransactionDto>.ErrorResult("Số tiền không khớp");
            }

            // Cập nhật trạng thái transaction thành công
            transaction.Status = TRANSACTION_STATUS_COMPLETED;
            transaction.UpdateAt = DateTime.Now;
            await _unitOfWork.WalletTransactionRepo.UpdateAsync(transaction);

            // Cập nhật số dư ví
            var wallet = await _unitOfWork.UserWalletRepo.GetByIdAsync(transaction.WalletId);
            if (wallet == null)
            {
                return ApiResponse<WalletTransactionDto>.ErrorResult("Không tìm thấy ví");
            }

            wallet.Balance += transaction.Amount;
            wallet.UpdateAt = DateTime.Now;
            await _unitOfWork.UserWalletRepo.UpdateAsync(wallet);

            // Tạo DTO để trả về
            var transactionDto = new WalletTransactionDto
            {
                TransactionId = transaction.TransactionId,
                WalletId = transaction.WalletId,
                TransactionType = transaction.TransactionType,
                Amount = transaction.Amount,
                Method = transaction.Method,
                Status = transaction.Status,
                CreateAt = transaction.CreateAt,
                UpdateAt = transaction.UpdateAt
            };

            return ApiResponse<WalletTransactionDto>.SuccessResult(transactionDto, "Nạp tiền thành công");
        }
        #endregion

        #region Payment & Refund
        public async Task<ApiResponse<CreateChangeBalanceRequestDto>> CreatePaymentRequestAsync(CreateChangeBalanceRequestDto paymentRequestDTO, Guid userId)
        {
            if (userId == null)
            {
                return ApiResponse<CreateChangeBalanceRequestDto>.ErrorResult("Người dùng không hợp lệ");
            }

            UserWallet wallet = await _unitOfWork.UserWalletRepo.GetWalletByUserIdAsync(userId);
            if (wallet == null)
            {
                return ApiResponse<CreateChangeBalanceRequestDto>.ErrorResult("Ví người dùng không tồn tại");
            }

            WalletTransaction newTransaction = new WalletTransaction
            {
                WalletId = wallet.WalletId,
                TransactionType = TRANSACTION_TYPE_PAYMENT,
                Amount = paymentRequestDTO.Amount,
                Method = TRANSACTION_METHOD_WALLET,
                Status = TRANSACTION_STATUS_PENDING,
                ReferenceId = paymentRequestDTO.ReferenceId,
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now
            };

            // Create transaction with pending status
            await _unitOfWork.WalletTransactionRepo.CreateAsync(newTransaction);

            // Try to subtract balance from wallet
            UpdateBalanceRequest updateRequest = new UpdateBalanceRequest
            {
                WalletId = wallet.WalletId,
                Amount = paymentRequestDTO.Amount,
                TypeUpdate = UPDATE_BALANCE_TYPE_SUBTRACT,
            };
            var result = await _walletService.UpdateWalletBalanceAsync(updateRequest);

            // Update transaction status based on result
            newTransaction.Status = result.Success ? TRANSACTION_STATUS_COMPLETED : TRANSACTION_STATUS_FAILED;
            newTransaction.UpdateAt = DateTime.Now;
            await _unitOfWork.WalletTransactionRepo.UpdateAsync(newTransaction);

            //var transactionDto = new WalletTransactionDto
            //{
            //    TransactionId = newTransaction.TransactionId,
            //    WalletId = newTransaction.WalletId,
            //    TransactionType = newTransaction.TransactionType,
            //    Amount = newTransaction.Amount,
            //    Method = newTransaction.Method,
            //    Status = newTransaction.Status,
            //    CreateAt = newTransaction.CreateAt,
            //    UpdateAt = newTransaction.UpdateAt
            //};

            if (!result.Success)
            {
                return ApiResponse<CreateChangeBalanceRequestDto>.ErrorResult("Thanh toán thất bại: " + result.Message);
            }

            return ApiResponse<CreateChangeBalanceRequestDto>.SuccessResult(paymentRequestDTO, "Tạo yêu cầu thanh toán thành công");
        }

        public async Task<ApiResponse<CreateChangeBalanceRequestDto>> CreateRefundRequestAsync(CreateChangeBalanceRequestDto paymentRequestDTO, Guid userId)
        {
            if (userId == null)
            {
                return ApiResponse<CreateChangeBalanceRequestDto>.ErrorResult("Người dùng không hợp lệ");
            }
            UserWallet wallet = await _unitOfWork.UserWalletRepo.GetWalletByUserIdAsync(userId);
            if (wallet == null)
            {
                return ApiResponse<CreateChangeBalanceRequestDto>.ErrorResult("Ví người dùng không tồn tại");
            }
            WalletTransaction newTransaction = new WalletTransaction
            {
                WalletId = wallet.WalletId,
                TransactionType = TRANSACTION_TYPE_REFUND,
                Amount = paymentRequestDTO.Amount,
                Method = TRANSACTION_METHOD_SYSTEM,
                Status = TRANSACTION_STATUS_PENDING,
                ReferenceId = paymentRequestDTO.ReferenceId,
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now
            };
            // Create transaction with pending status
            await _unitOfWork.WalletTransactionRepo.CreateAsync(newTransaction);
            // Add balance to wallet
            UpdateBalanceRequest updateRequest = new UpdateBalanceRequest
            {
                WalletId = wallet.WalletId,
                Amount = paymentRequestDTO.Amount,
                TypeUpdate = UPDATE_BALANCE_TYPE_ADD,
            };
            var result = await _walletService.UpdateWalletBalanceAsync(updateRequest);
            // Update transaction status based on result
            newTransaction.Status = result.Success ? TRANSACTION_STATUS_COMPLETED : TRANSACTION_STATUS_FAILED;
            newTransaction.UpdateAt = DateTime.Now;
            await _unitOfWork.WalletTransactionRepo.UpdateAsync(newTransaction);
            return ApiResponse<CreateChangeBalanceRequestDto>.SuccessResult(paymentRequestDTO, "Tạo yêu cầu hoàn tiền thành công");
        }
        #endregion

        #region Transaction
        public async Task<ApiResponse<List<WalletTransactionDto>>> GetAllTransactionAsync()
        {
            //Get user info
            UserDto? user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                return ApiResponse<List<WalletTransactionDto>>.ErrorResult("Người dùng không hợp lệ");
            }

            UserWallet wallet = await _unitOfWork.UserWalletRepo.GetWalletByUserIdAsync(user.Id);
            if (wallet == null)
            {
                return ApiResponse<List<WalletTransactionDto>>.ErrorResult("Ví người dùng không tồn tại");
            }

            List<WalletTransaction> transactions = await _unitOfWork.WalletTransactionRepo.GetAllAsync();
            if (transactions == null)
            {
                return ApiResponse<List<WalletTransactionDto>>.ErrorResult("Không tìm thấy giao dịch");
            }
            List<WalletTransactionDto> transactionDto = transactions
                .Where(t => t.WalletId == wallet.WalletId)
                .Select(t => new WalletTransactionDto
                {
                    TransactionId = t.TransactionId,
                    UserId = user.Id,
                    WalletId = t.WalletId,
                    TransactionType = t.TransactionType,
                    Amount = t.Amount,
                    Method = t.Method,
                    Status = t.Status,
                    ReferenceId = t.ReferenceId,
                    ReferenceType = t.ReferenceType,
                    Description = t.Description,
                    CreateAt = t.CreateAt,
                    UpdateAt = t.UpdateAt
                })
                .OrderByDescending(t => t.CreateAt)
                .ToList();
            return ApiResponse<List<WalletTransactionDto>>.SuccessResult(transactionDto, "Lấy thông tin giao dịch thành công");
        }
        #endregion
    }
}
