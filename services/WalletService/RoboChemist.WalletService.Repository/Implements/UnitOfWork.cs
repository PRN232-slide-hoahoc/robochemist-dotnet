using RoboChemist.WalletService.Model.Data;
using RoboChemist.WalletService.Repository.Interfaces;

namespace RoboChemist.WalletService.Repository.Implements
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly WalletDbContext _context;

        public IUserWalletRepository UserWalletRepo { get; private set; }

        public IWalletTransactionRepository WalletTransactionRepo { get; private set; }

        public UnitOfWork(WalletDbContext context)
        {
            _context = context;
            UserWalletRepo = new UserWalletRepository(_context);
            WalletTransactionRepo = new WalletTransactionRepository(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
