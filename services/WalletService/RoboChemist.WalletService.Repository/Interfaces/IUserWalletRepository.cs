using RoboChemist.Shared.Common.GenericRepositories;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.UserWalletDTOs;
using RoboChemist.WalletService.Model.Entities;

namespace RoboChemist.WalletService.Repository.Interfaces
{
    public interface IUserWalletRepository : IGenericRepository<UserWallet>
    {
        Task<UserWallet> GetWalletByUserIdAsync(Guid userId);
    }
}
