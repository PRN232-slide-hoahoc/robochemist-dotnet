using Microsoft.AspNetCore.Http;
using RoboChemist.Shared.DTOs.WalletServiceDTOs;
using RoboChemist.WalletService.Repository.Implements;
using RoboChemist.WalletService.Service.Interfaces;
using RoboChemist.WalletService.Service.Libraries;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.VNPayDTOs;

namespace RoboChemist.WalletService.Service.Implements
{
    public class VNPayService : IVNPayService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly VNPayConfig _vnpayConfig;
        public VNPayService(UnitOfWork unitOfWork,VNPayConfig vnpayConfig)
        {
            _unitOfWork = unitOfWork;
            _vnpayConfig = vnpayConfig;
        }
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
        public Task<string> CreateDepositUrlAsync(VNPayDTOs.DepositRequestDTO depositRequestDTO, HttpContext httpContext)
        {
            VNPayLibrary pay = new VNPayLibrary();
            string tick = DateTime.Now.Ticks.ToString();
            string urlCallBack = GetCallbackUrl();

            pay.AddRequestData("vnp_Version", "2.1.0");
            pay.AddRequestData("vnp_Command", "pay");
            pay.AddRequestData("vnp_TmnCode", _vnpayConfig.TmnCode);

            pay.AddRequestData("vnp_Amount", depositRequestDTO.amount.ToString());
            pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", "VND");
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(httpContext));
            pay.AddRequestData("vnp_Locale", "vn");
            pay.AddRequestData("vnp_OrderInfo", $"Deposit through VNPay");
            pay.AddRequestData("vnp_OrderType", "other");
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", tick);

            string depositUrl = pay.CreateRequestUrl(_vnpayConfig.VnpayUrl, _vnpayConfig.HashSecret);

            return depositUrl;
        }
    }
}
