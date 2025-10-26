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
            public string token { get; set; }
            public decimal amount { get; set; }
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
