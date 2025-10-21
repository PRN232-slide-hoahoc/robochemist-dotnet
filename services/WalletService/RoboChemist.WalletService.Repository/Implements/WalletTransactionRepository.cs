using Microsoft.EntityFrameworkCore;
using RoboChemist.Shared.Common.GenericRepositories;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.WalletTransactionDTOs;
using RoboChemist.WalletService.Model.Entities;
using RoboChemist.WalletService.Repository.Interfaces;

namespace RoboChemist.WalletService.Repository.Implements
{
    public class WalletTransactionRepository : GenericRepository<WalletTransaction>, IWalletTransactionRepository
    {
        public WalletTransactionRepository(DbContext context) : base(context)
        {
        }
        /// <summary>
        /// Get transaction history for a specific user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<WalletTransactionDto>> GetTransactionHistoryAsync(Guid userId)
        {
            List<WalletTransactionDto> transactionHistory = await _dbSet
                .Include(t => t.Wallet)
                .Where(t => t.Wallet.UserId == userId)
                .Select(t => new WalletTransactionDto
                {
                    TransactionId = t.TransactionId,
                    UserId = t.Wallet.UserId,
                    WalletId = t.WalletId,
                    TransactionType = t.TransactionType,
                    Amount = t.Amount,
                    Method = t.Method,
                    Status = t.Status,
                    ReferenceId = t.ReferenceId,
                    CreateAt = t.CreateAt,
                    UpdateAt = t.UpdateAt
                })
                .OrderByDescending(t => t.CreateAt)
                .ToListAsync();

            return transactionHistory;
        }
    }
}
