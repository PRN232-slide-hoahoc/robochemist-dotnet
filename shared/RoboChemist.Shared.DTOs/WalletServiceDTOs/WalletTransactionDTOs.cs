namespace RoboChemist.Shared.DTOs.WalletServiceDTOs
{
    public class WalletTransactionDTOs
    {
        public class WalletTransactionDto
        {
            public Guid TransactionId { get; set; }
            public Guid? UserId { get; set; }
            public Guid WalletId { get; set; }
            public string TransactionType { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public string Method { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public Guid? ReferenceId { get; set; }
            public DateTime? CreateAt { get; set; }
            public DateTime? UpdateAt { get; set; }
        }
        
        public class CreateWalletTransactionDto
        {
            public Guid WalletId { get; set; }
            public string TransactionType { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public string Method { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public Guid? ReferenceId { get; set; }
            public DateTime? CreateAt { get; set; } = DateTime.UtcNow;
        }
        public class CreatePaymentRequestDto
        {
            public decimal Amount { get; set; }
            public Guid? ReferenceId { get; set; }
        }
    }
}
