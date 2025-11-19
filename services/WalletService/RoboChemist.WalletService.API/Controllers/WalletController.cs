using Microsoft.AspNetCore.Mvc;
using RoboChemist.Shared.DTOs.Common;
using RoboChemist.WalletService.Service.Interfaces;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.UserWalletDTOs;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.VNPayDTOs;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.WalletTransactionDTOs;

namespace RoboChemist.WalletService.API.Controllers
{
    [Route("api/v1/wallet")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly IPaymentService _paymentService;
        public WalletController(IWalletService walletService, IPaymentService paymentService)
        {
            _walletService = walletService;
            _paymentService = paymentService;
        }

        /// <summary>
        /// Lấy thông tin ví của người dùng
        /// </summary>
        /// <returns>Trả về các thông tin cơ bản của ví (id ví,số dư, lần cuối cập nhật)</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<UserWalletDto>>> GetUserWallet()
        {
            try
            {
                var result = await _walletService.GetWalletByUserIdAsync();
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<UserWalletDto>.ErrorResult("Lỗi hệ thống"));
            }
        }

        /// <summary>
        /// Tạo ví cho người dùng
        /// </summary>
        /// <returns>nếu người dùng chưa có ví thì sẽ tạo mới, còn đã có thì vô hiệu</returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserWalletDto>>> CreateUserWallet()
        {
            try
            {

                var result = await _walletService.GenerateWalletAsync();
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UserWalletDto>.ErrorResult($"Lỗi hệ thống: {ex.Message}"));
            }
        }

        /// <summary>
        /// Chỉ lấy số dư ví của người dùng
        /// </summary>
        /// <returns></returns>
        [HttpGet("balance")]
        public async Task<ActionResult<ApiResponse<WalletBalanceDto>>> GetBalance()
        {
            try
            {
                var result = await _walletService.GetBalanceAsync();
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<WalletBalanceDto>.ErrorResult("Lỗi hệ thống"));
            }
        }

        /// <summary>
        /// Tạo một giao dịch thanh toán
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("payment")]
        public async Task<ActionResult<ApiResponse<PaymentResponseDto>>> CreatePayment([FromBody] CreatePaymentDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<PaymentResponseDto>.ErrorResult("Dữ liệu không hợp lệ"));
                }

                var result = await _walletService.CreatePaymentAsync(request);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<PaymentResponseDto>.ErrorResult("Lỗi hệ thống"));
            }
        }

        /// <summary>
        /// Tạo một giao dịch hoàn tiền
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("refund")]
        public async Task<ActionResult<ApiResponse<RefundResponseDto>>> RefundPayment([FromBody] RefundRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<RefundResponseDto>.ErrorResult("Dữ liệu không hợp lệ"));
                }

                var result = await _walletService.RefundPaymentAsync(request);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<RefundResponseDto>.ErrorResult("Lỗi hệ thống"));
            }
        }

        /// <summary>
        /// Tạo yêu cầu nạp tiền vào ví qua VNPay
        /// </summary>
        /// <param name="depositRequestDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("deposit")]
        public async Task<ActionResult<ApiResponse<string>>> CreateDepositUrl([FromBody] DepositRequestDTO depositRequestDTO)
        {
            try
            {
                if (depositRequestDTO.amount < 10000)
                {
                    return BadRequest(ApiResponse<string>.ErrorResult("Số tiền nạp phải lớn hơn 10000"));
                }
                else if (depositRequestDTO.amount > 999999999)
                {
                    return BadRequest(ApiResponse<string>.ErrorResult("Số tiền nạp không hợp lệ"));
                }

                var result = await _paymentService.CreateDepositUrlAsync(depositRequestDTO, HttpContext);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResult("Lỗi hệ thống"));
            }
        }

        /// <summary>
        /// Xử lý callback nạp tiền từ VNPay
        /// </summary>
        /// <param name="callbackRequestDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("deposit-callback")]
        public async Task<ActionResult<ApiResponse<DepositCallbackRequestDto>>> DepositCallback([FromBody] DepositCallbackRequestDto callbackRequestDTO)
        {
            try
            {
                var result = await _paymentService.DepositCallbackAsync(callbackRequestDTO);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<DepositCallbackRequestDto>.ErrorResult("Lỗi hệ thống"));
            }
        }

        /// <summary>
        /// Lấy tất cả giao dịch của người dùng
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("get-all-transaction")]
        public async Task<ActionResult<ApiResponse<List<WalletTransactionDto>>>> GetAllTransaction()
        {
            try
            {
                var result = await _paymentService.GetAllTransactionAsync();
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<List<WalletTransactionDto>>.ErrorResult("Lỗi hệ thống"));
                throw;
            }
        }

        /// <summary>
        /// Lấy các giao dịch theo ReferenceId
        /// </summary>
        /// <param name="referenceId"></param>
        /// <returns></returns>
        [HttpGet("transactions/reference/{referenceId}")]
        public async Task<ActionResult<ApiResponse<TransactionsByReferenceDto>>> GetTransactionsByReference(Guid referenceId)
        {
            try
            {
                var result = await _walletService.GetTransactionsByReferenceAsync(referenceId);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<TransactionsByReferenceDto>.ErrorResult("Lỗi hệ thống"));
            }
        }
    }
}
