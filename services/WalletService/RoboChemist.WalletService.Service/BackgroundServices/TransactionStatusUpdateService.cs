using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RoboChemist.WalletService.Repository.Interfaces;
using static RoboChemist.Shared.Common.Constants.RoboChemistConstants;

namespace RoboChemist.WalletService.Service.BackgroundServices
{
    public class TransactionStatusUpdateService : BackgroundService
    {
        private readonly ILogger<TransactionStatusUpdateService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // Kiểm tra mỗi 1 phút
        private readonly TimeSpan _pendingTimeout = TimeSpan.FromMinutes(10); // Timeout sau 10 phút

        public TransactionStatusUpdateService(
            ILogger<TransactionStatusUpdateService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateExpiredPendingTransactionsAsync();
                }
                catch (Exception ex)
                {
                }

                // Chờ trước khi chạy lần tiếp theo
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task UpdateExpiredPendingTransactionsAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                // Lấy tất cả các transaction có status là PENDING
                var allTransactions = await unitOfWork.WalletTransactionRepo.GetAllAsync();

                var expiredTransactions = allTransactions
                    .Where(t => t.Status == TRANSACTION_STATUS_PENDING &&
                                t.CreateAt.HasValue &&
                                DateTime.Now - t.CreateAt.Value > _pendingTimeout)
                    .ToList();

                if (expiredTransactions.Any())
                {
                    foreach (var transaction in expiredTransactions)
                    {
                        transaction.Status = TRANSACTION_STATUS_FAILED;
                        transaction.UpdateAt = DateTime.Now;
                        await unitOfWork.WalletTransactionRepo.UpdateAsync(transaction);
                    }
                }
            }
        }
    }
}
