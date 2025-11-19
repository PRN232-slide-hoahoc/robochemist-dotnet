using Microsoft.EntityFrameworkCore;
using RoboChemist.Shared.Common.GenericRepositories;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.WalletTransactionDTOs;
using RoboChemist.WalletService.Model.Entities;
using RoboChemist.WalletService.Repository.Interfaces;
using static RoboChemist.Shared.Common.Constants.RoboChemistConstants;

namespace RoboChemist.WalletService.Repository.Implements
{
    public class WalletTransactionRepository : GenericRepository<WalletTransaction>, IWalletTransactionRepository
    {
        public WalletTransactionRepository(DbContext context) : base(context)
        {
        }

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
                    ReferenceType = t.ReferenceType,
                    Description = t.Description,
                    CreateAt = t.CreateAt,
                    UpdateAt = t.UpdateAt
                })
                .OrderByDescending(t => t.CreateAt)
                .ToListAsync();

            return transactionHistory;
        }

        public async Task<List<WalletTransaction>> GetByReferenceIdAsync(Guid referenceId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(t => t.ReferenceId == referenceId)
                .ToListAsync();
        }

        public async Task<WalletTransaction?> GetPaymentByReferenceIdAsync(Guid referenceId)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.ReferenceId == referenceId && t.TransactionType == TRANSACTION_TYPE_PAYMENT);
        }
    }
}
