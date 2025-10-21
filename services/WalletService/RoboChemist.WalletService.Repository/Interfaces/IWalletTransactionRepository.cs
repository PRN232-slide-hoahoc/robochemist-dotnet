using RoboChemist.Shared.Common.GenericRepositories;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.WalletTransactionDTOs;
using RoboChemist.WalletService.Model.Entities;

namespace RoboChemist.WalletService.Repository.Interfaces
{
    public interface IWalletTransactionRepository : IGenericRepository<WalletTransaction>
    {
        Task<List<WalletTransactionDto>> GetTransactionHistoryAsync(Guid userId);
    }
}
