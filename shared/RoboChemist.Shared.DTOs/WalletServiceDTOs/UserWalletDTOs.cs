using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboChemist.Shared.DTOs.WalletServiceDTOs
{
    public class UserWalletDTOs
    {
        public class UserWalletDTO
        {
            public Guid WalletId { get; set; }
            public Guid UserId { get; set; }
            public decimal Balance { get; set; }
            public DateTime? UpdateAt { get; set; }
        }
    }
}
