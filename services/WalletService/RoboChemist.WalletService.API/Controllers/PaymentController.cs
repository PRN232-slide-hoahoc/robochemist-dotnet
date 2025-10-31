using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoboChemist.Shared.DTOs.Common;
using RoboChemist.WalletService.Service.Interfaces;
using System.Security.Claims;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.VNPayDTOs;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.WalletTransactionDTOs;

namespace RoboChemist.WalletService.API.Controllers
{
    [Route("api/v1/payments")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly IPaymentService _paymentService;
        public PaymentController(IWalletService walletService, IPaymentService paymentService)
        {
            _walletService = walletService;
            _paymentService = paymentService;
        }

        [Authorize]
        [HttpPost]
        [Route("create-deposit-url")]
        public async Task<ActionResult<ApiResponse<string>>> CreateDepositUrl([FromBody] DepositRequestDTO depositRequestDTO)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(ApiResponse<string>.ErrorResult("Người dùng chưa đăng nhập"));
                }

                if (!Guid.TryParse(userIdClaim, out Guid userId))
                {
                    return BadRequest(ApiResponse<string>.ErrorResult("User ID không hợp lệ"));
                }

                depositRequestDTO.userId = userId;

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

        [Authorize]
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

        [Authorize]
        [HttpPost]
        [Route("create-payment-request")]
        public async Task<ActionResult<ApiResponse<CreateChangeBalanceRequestDto>>> CreatePaymentRequest([FromBody] CreateChangeBalanceRequestDto createPaymentRequestDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(ApiResponse<CreateChangeBalanceRequestDto>.ErrorResult("Người dùng chưa đăng nhập"));
                }

                if (!Guid.TryParse(userIdClaim, out Guid userId))
                {
                    return BadRequest(ApiResponse<CreateChangeBalanceRequestDto>.ErrorResult("User ID không hợp lệ"));
                }

                var result = await _paymentService.CreatePaymentRequestAsync(createPaymentRequestDto, userId);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<CreateChangeBalanceRequestDto>.ErrorResult("Lỗi hệ thống"));
            }
        }

        [Authorize]
        [HttpPost]
        [Route("create-refund-request")]
        public async Task<ActionResult<ApiResponse<CreateChangeBalanceRequestDto>>> CreateRefundRequest([FromBody] CreateChangeBalanceRequestDto createPaymentRequestDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(ApiResponse<CreateChangeBalanceRequestDto>.ErrorResult("Người dùng chưa đăng nhập"));
                }

                if (!Guid.TryParse(userIdClaim, out Guid userId))
                {
                    return BadRequest(ApiResponse<CreateChangeBalanceRequestDto>.ErrorResult("User ID không hợp lệ"));
                }

                var result = await _paymentService.CreateRefundRequestAsync(createPaymentRequestDto, userId);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<CreateChangeBalanceRequestDto>.ErrorResult("Lỗi hệ thống"));
            }
        }

        [Authorize]
        [HttpGet]
        [Route("get-all-transaction")]
        public async Task<ActionResult<ApiResponse<List<WalletTransactionDto>>>> GetAllTransaction()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(ApiResponse<List<WalletTransactionDto>>.ErrorResult("Người dùng chưa đăng nhập"));
                }

                if (!Guid.TryParse(userIdClaim, out Guid userId))
                {
                    return BadRequest(ApiResponse<List<WalletTransactionDto>>.ErrorResult("User ID không hợp lệ"));
                }

                var result = await _paymentService.GetAllTransactionAsync(userId);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<List<WalletTransactionDto>>.ErrorResult("Lỗi hệ thống"));
                throw;
            }
        }
    }
}
