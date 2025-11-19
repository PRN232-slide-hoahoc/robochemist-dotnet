using System;
using System.Collections.Generic;

namespace RoboChemist.WalletService.Model.Entities;

public partial class WalletTransaction
{
    public Guid TransactionId { get; set; }

    public Guid WalletId { get; set; }

    public string TransactionType { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Method { get; set; } = null!;

    public string Status { get; set; } = null!;

    public Guid? ReferenceId { get; set; }

    public string? ReferenceType { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public string? Description { get; set; }

    public virtual UserWallet Wallet { get; set; } = null!;
}
