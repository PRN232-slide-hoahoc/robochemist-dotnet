namespace RoboChemist.WalletService.Repository.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserWalletRepository userWallet { get; }
        IWalletTransactionRepository walletTransaction { get; }
    }
}
