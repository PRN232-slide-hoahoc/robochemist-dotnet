using RoboChemist.WalletService.Model.Data;
using RoboChemist.WalletService.Repository.Interfaces;

namespace RoboChemist.WalletService.Repository.Implements
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly WalletDbContext _context;

        public IUserWalletRepository userWallet { get; private set; }

        public IWalletTransactionRepository walletTransaction { get; private set; }

        public UnitOfWork(WalletDbContext context)
        {
            _context = context;
            userWallet = new UserWalletRepository(_context);
            walletTransaction = new WalletTransactionRepository(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
