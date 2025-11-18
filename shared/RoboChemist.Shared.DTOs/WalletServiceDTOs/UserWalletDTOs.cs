namespace RoboChemist.Shared.DTOs.WalletServiceDTOs
{
    public class UserWalletDTOs
    {
        public class UserWalletDto
        {
            public Guid WalletId { get; set; }
            public Guid UserId { get; set; }
            public decimal Balance { get; set; }
            public DateTime? UpdateAt { get; set; }
        }
        public class UpdateBalanceRequest
        {
            public Guid WalletId { get; set; }
            public string TypeUpdate { get; set; } = string.Empty;
            public decimal Amount { get; set; }
        }

        public class WalletBalanceDto
        {
            public Guid UserId { get; set; }
            public Guid WalletId { get; set; }
            public decimal Balance { get; set; }
            public DateTime? UpdateAt { get; set; }
        }
    }
}
