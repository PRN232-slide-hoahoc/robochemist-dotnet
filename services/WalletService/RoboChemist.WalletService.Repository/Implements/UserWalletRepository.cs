using Microsoft.EntityFrameworkCore;
using RoboChemist.Shared.Common.GenericRepositories;
using RoboChemist.WalletService.Model.Entities;
using RoboChemist.WalletService.Repository.Interfaces;

namespace RoboChemist.WalletService.Repository.Implements
{
    public class UserWalletRepository : GenericRepository<UserWallet>, IUserWalletRepository
    {
        public UserWalletRepository(DbContext context) : base(context)
        {
        }
    }
}
