using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using RoboChemist.WalletService.Model.Entities;

namespace RoboChemist.WalletService.Repository.Data;

public partial class WalletDbContext : DbContext
{
    public WalletDbContext()
    {
    }

    public WalletDbContext(DbContextOptions<WalletDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<UserWallet> UserWallets { get; set; }

    public virtual DbSet<WalletTransaction> WalletTransactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserWallet>(entity =>
        {
            entity.HasKey(e => e.WalletId).HasName("UserWallet_pkey");

            entity.ToTable("UserWallet");

            entity.HasIndex(e => e.UserId, "idx_wallet_userid");

            entity.Property(e => e.WalletId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Balance).HasPrecision(18, 2);
            entity.Property(e => e.UpdateAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<WalletTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("WalletTransaction_pkey");

            entity.ToTable("WalletTransaction");

            entity.HasIndex(e => e.WalletId, "idx_transaction_walletid");

            entity.Property(e => e.TransactionId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Method).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TransactionType).HasMaxLength(50);
            entity.Property(e => e.UpdateAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Wallet).WithMany(p => p.WalletTransactions)
                .HasForeignKey(d => d.WalletId)
                .HasConstraintName("fk_wallet");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
