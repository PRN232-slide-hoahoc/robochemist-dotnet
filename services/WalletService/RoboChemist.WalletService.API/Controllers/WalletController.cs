using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoboChemist.Shared.DTOs.Common;
using RoboChemist.WalletService.Model.Entities;
using RoboChemist.WalletService.Service.Interfaces;
using static RoboChemist.Shared.DTOs.GradeDTOs.GradeDTOs;
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

        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserWalletDto>>> CreateUserWallet()
        {
            try
            {
                var result = await _walletService.GenerateWalletAsync();
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<UserWalletDto>.ErrorResult("Lỗi hệ thống"));
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
