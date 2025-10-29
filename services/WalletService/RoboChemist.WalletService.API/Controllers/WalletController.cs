using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoboChemist.Shared.DTOs.Common;
using RoboChemist.WalletService.Service.Interfaces;
using System.Security.Claims;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.UserWalletDTOs;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.WalletTransactionDTOs;

namespace RoboChemist.WalletService.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;
        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<UserWalletDto>>> GetUserWallet()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(ApiResponse<UserWalletDto>.ErrorResult("Người dùng chưa đăng nhập"));
                }

                if (!Guid.TryParse(userIdClaim, out Guid userId))
                {
                    return BadRequest(ApiResponse<UserWalletDto>.ErrorResult("User ID không hợp lệ"));
                }

                var result = await _walletService.GetWalletByUserIdAsync(userId);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<UserWalletDto>.ErrorResult("Lỗi hệ thống"));
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserWalletDto>>> CreateUserWallet()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(ApiResponse<UserWalletDto>.ErrorResult("Người dùng chưa đăng nhập"));
                }

                if (!Guid.TryParse(userIdClaim, out Guid userId))
                {
                    return BadRequest(ApiResponse<UserWalletDto>.ErrorResult("User ID không hợp lệ"));
                }

                var result = await _walletService.GenerateWalletAsync(userId);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UserWalletDto>.ErrorResult($"Lỗi hệ thống: {ex.Message}"));
            }
        }

        [Authorize]
        [HttpGet("balance")]
        public async Task<ActionResult<ApiResponse<WalletBalanceDto>>> GetBalance()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(ApiResponse<WalletBalanceDto>.ErrorResult("Người dùng chưa đăng nhập"));
                }

                if (!Guid.TryParse(userIdClaim, out Guid userId))
                {
                    return BadRequest(ApiResponse<WalletBalanceDto>.ErrorResult("User ID không hợp lệ"));
                }

                var result = await _walletService.GetBalanceAsync(userId);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<WalletBalanceDto>.ErrorResult("Lỗi hệ thống"));
            }
        }

        [Authorize]
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

        [Authorize]
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

        [Authorize]
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
