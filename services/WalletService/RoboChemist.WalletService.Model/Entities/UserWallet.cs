using System;
using System.Collections.Generic;

namespace RoboChemist.WalletService.Model.Entities;

public partial class UserWallet
{
    public Guid WalletId { get; set; }

    public Guid UserId { get; set; }

    public decimal Balance { get; set; }

    public DateTime? UpdateAt { get; set; }

    public virtual ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
}
