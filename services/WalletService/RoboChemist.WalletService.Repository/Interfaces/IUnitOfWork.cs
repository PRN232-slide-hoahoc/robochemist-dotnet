namespace RoboChemist.WalletService.Repository.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserWalletRepository UserWalletRepo { get; }
        IWalletTransactionRepository WalletTransactionRepo { get; }
    }
}
