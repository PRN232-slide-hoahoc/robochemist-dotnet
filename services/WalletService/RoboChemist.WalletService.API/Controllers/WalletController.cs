using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoboChemist.Shared.DTOs.Common;
using RoboChemist.WalletService.Service.Interfaces;
using System.Security.Claims;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.UserWalletDTOs;

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

        //Using for demo
        //[HttpPost]
        //[Route("update-balance")]
        //public async Task<ActionResult<ApiResponse<UserWalletDto>>> UpdateWalletBalance([FromBody] UpdateBalanceRequest request)
        //{
        //    try
        //    {
        //        var result = await _walletService.UpdateWalletBalanceAsync(request);
        //        return result.Success ? Ok(result) : BadRequest(result);
        //    }
        //    catch (Exception)
        //    {
        //        return StatusCode(500, ApiResponse<UserWalletDto>.ErrorResult("Lỗi hệ thống"));
        //    }
        //}
    }
}
