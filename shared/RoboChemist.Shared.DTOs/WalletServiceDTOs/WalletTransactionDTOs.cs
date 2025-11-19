using System.ComponentModel.DataAnnotations;

namespace RoboChemist.Shared.DTOs.WalletServiceDTOs
{
    public class WalletTransactionDTOs
    {
        public class WalletTransactionDto
        {
            public Guid TransactionId { get; set; }
            public Guid? UserId { get; set; }
            public string? UserName { get; set; }
            public Guid WalletId { get; set; }
            public string TransactionType { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public string Method { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public Guid? ReferenceId { get; set; }
            public string? ReferenceType { get; set; }
            public string? Description { get; set; }
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

        public class CreateChangeBalanceRequestDto
        {
            public decimal Amount { get; set; }
            public Guid? ReferenceId { get; set; }
        }

        public class CreatePaymentDto
        {
            [Required(ErrorMessage = "UserId là bắt buộc")]
            public Guid UserId { get; set; }

            [Required(ErrorMessage = "Amount là bắt buộc")]
            [Range(1, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 0")]
            public decimal Amount { get; set; }

            [Required(ErrorMessage = "ReferenceId là bắt buộc")]
            public Guid ReferenceId { get; set; }

            [Required(ErrorMessage = "ReferenceType là bắt buộc")]
            [StringLength(50, ErrorMessage = "ReferenceType không được vượt quá 50 ký tự")]
            public string ReferenceType { get; set; } = string.Empty;

            [StringLength(500, ErrorMessage = "Description không được vượt quá 500 ký tự")]
            public string? Description { get; set; }
        }

        public class PaymentResponseDto
        {
            public Guid TransactionId { get; set; }
            public Guid WalletId { get; set; }
            public Guid UserId { get; set; }
            public decimal Amount { get; set; }
            public decimal NewBalance { get; set; }
            public string TransactionType { get; set; } = string.Empty;
            public string Method { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public Guid ReferenceId { get; set; }
            public string ReferenceType { get; set; } = string.Empty;
            public DateTime CreateAt { get; set; }
        }

        public class RefundRequestDto
        {
            [Required(ErrorMessage = "ReferenceId là bắt buộc")]
            public Guid ReferenceId { get; set; }

            [StringLength(500, ErrorMessage = "Reason không được vượt quá 500 ký tự")]
            public string? Reason { get; set; }
        }

        public class RefundResponseDto
        {
            public Guid RefundTransactionId { get; set; }
            public Guid OriginalTransactionId { get; set; }
            public Guid WalletId { get; set; }
            public Guid UserId { get; set; }
            public decimal Amount { get; set; }
            public decimal NewBalance { get; set; }
            public string TransactionType { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public Guid ReferenceId { get; set; }
            public DateTime CreateAt { get; set; }
        }

        public class ChangeAdminBalanceDto
        {
            [Required(ErrorMessage = "Amount là bắt buộc")]
            public decimal Amount { get; set; }
            [Required(ErrorMessage = "TransactionType là bắt buộc")]
            [RegularExpression("^(Payment|Refund)$", ErrorMessage = "TransactionType phải là 'Payment' hoặc 'Refund'")]
            public string TransactionType { get; set; } = string.Empty;
            [Required(ErrorMessage = "ReferenceId là bắt buộc")]
            public Guid ReferenceId { get; set; }
            public string? ReferenceType { get; set; }
            public string? Reason { get; set; }
        }
        public class TransactionsByReferenceDto
        {
            public Guid ReferenceId { get; set; }
            public List<WalletTransactionDto> Transactions { get; set; } = new();
            public decimal TotalPayment { get; set; }
            public decimal TotalRefund { get; set; }
            public decimal NetAmount { get; set; }
        }
    }
}
