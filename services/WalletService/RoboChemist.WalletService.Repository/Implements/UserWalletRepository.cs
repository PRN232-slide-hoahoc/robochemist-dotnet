using Microsoft.EntityFrameworkCore;
using RoboChemist.Shared.Common.GenericRepositories;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.UserWalletDTOs;
using RoboChemist.WalletService.Model.Entities;
using RoboChemist.WalletService.Repository.Interfaces;

namespace RoboChemist.WalletService.Repository.Implements
{
    public class UserWalletRepository : GenericRepository<UserWallet>, IUserWalletRepository
    {
        public UserWalletRepository(DbContext context) : base(context)
        {
        }

        public async Task<UserWallet> GetWalletByUserIdAsync(Guid userId)
        {
            UserWallet? wallet = await _dbSet
                .Where(w => w.UserId == userId)
                .Select(w => new UserWallet
                {
                    WalletId = w.WalletId,
                    UserId = w.UserId,
                    Balance = w.Balance,
                    UpdateAt = w.UpdateAt
                })
                .FirstOrDefaultAsync();
            return wallet;
        }
    }
}
