using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboChemist.Shared.DTOs.WalletServiceDTOs
{
    public class VNPayDTOs
    {
        public class DepositRequestDTO
        {
            public decimal amount { get; set; }
        }

        public class DepositCallbackRequestDto
        {
            public decimal vnp_Amount { get; set; }
            public string vnp_OrderInfo { get; set; }

        }
        public class DepositResponseDto
        {
            public string response { get; set; }
        }

        public class VNPayConfig
        {
            public string TmnCode { get; set; }
            public string HashSecret { get; set; }
            public string VnpayUrl { get; set; }
            public string CallbackUrl { get; set; }
        }
    }
}
