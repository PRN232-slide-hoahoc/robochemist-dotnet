using Microsoft.AspNetCore.Mvc;
using RoboChemist.Shared.DTOs.Common;
using RoboChemist.WalletService.Service.Interfaces;
using Sprache;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.VNPayDTOs;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.WalletTransactionDTOs;

namespace RoboChemist.WalletService.API.Controllers
{
    [Route("api/v1/[controller]")]
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

        [HttpPost]
        [Route("create-deposit-url")]
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

        [HttpPost]
        [Route("create-payment-request")]
        public async Task<ActionResult<ApiResponse<CreatePaymentRequestDto>>> CreatePaymentRequest([FromBody] CreatePaymentRequestDto createPaymentRequestDto)
        {
            try
            {
                var result = await _paymentService.CreatePaymentRequestAsync(createPaymentRequestDto);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<CreatePaymentRequestDto>.ErrorResult("Lỗi hệ thống"));
            }
        }

        [HttpPost]
        [Route("create-refund-request")]
        public async Task<ActionResult<ApiResponse<CreatePaymentRequestDto>>> CreateRefundRequest([FromBody] CreatePaymentRequestDto createPaymentRequestDto)
        {
            try
            {
                var result = await _paymentService.CreateRefundRequestAsync(createPaymentRequestDto);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<CreatePaymentRequestDto>.ErrorResult("Lỗi hệ thống"));
            }
        }

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
                return StatusCode(500, ApiResponse<CreatePaymentRequestDto>.ErrorResult("Lỗi hệ thống"));
                throw;
            }
        }
    }
}
