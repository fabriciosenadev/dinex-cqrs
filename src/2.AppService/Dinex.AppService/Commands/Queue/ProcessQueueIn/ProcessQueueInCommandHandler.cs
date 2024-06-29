using Dinex.Core;

namespace Dinex.AppService;

public class ProcessQueueInCommandHandler : ICommandHandler, IRequestHandler<ProcessQueueInCommand, OperationResult>
{
    private readonly IQueueInRepository _queueInRepository;
    private readonly IInvestmentHistoryRepository _investmentHistoryRepository;
    private readonly IStockBrokerRepository _stockBrokerRepository;
    private readonly IAssetRepository _assetRepository;
    private readonly ITransactionHistoryRepository _transactionHistoryRepository;
    private readonly IInvestmentTransactionRepository _investmentTransactionRepository;
    private readonly IWalletRepository _walletRepository;

    public ProcessQueueInCommandHandler(IQueueInRepository queueInRepository,
        IInvestmentHistoryRepository investmentHistoryRepository,
        IStockBrokerRepository stockBrokerRepository,
        IAssetRepository assetRepository,
        ITransactionHistoryRepository transactionHistoryRepository,
        IInvestmentTransactionRepository investmentTransactionRepository,
        IWalletRepository walletRepository
        )
    {
        _queueInRepository = queueInRepository;
        _investmentHistoryRepository = investmentHistoryRepository;
        _stockBrokerRepository = stockBrokerRepository;
        _assetRepository = assetRepository;
        _transactionHistoryRepository = transactionHistoryRepository;
        _investmentTransactionRepository = investmentTransactionRepository;
        _walletRepository = walletRepository;
    }

    public async Task<OperationResult> Handle(ProcessQueueInCommand request, CancellationToken cancellationToken)
    {
        var result = new OperationResult();
        try
        {
            Execute(request);
            return result;
        }
        catch (Exception ex)
        {
            return new OperationResult().SetAsInternalServerError()
                        .AddError($"An unexpected error ocurred to method: ActivateUser {ex}");
        }
    }

    #region
    private async Task Execute(ProcessQueueInCommand request)
    {
        var queueIn = await _queueInRepository.FindAsync(x => x.UserId == request.UserId);

        var investmentQueueIn = queueIn.Where(x => x.Type == TransactionActivity.Investment).ToList();
        if (investmentQueueIn.Count > 0)
        {
            var queueInIds = investmentQueueIn.Select(x => x.Id);
            await ProcessingInvestment(queueInIds, request.UserId);

            investmentQueueIn.ForEach(queueIn =>
            {
                queueIn.UpdateByProcessing();
            });
            await _queueInRepository.UpdateRangeAsync(investmentQueueIn);
        }

        var financialPlanningQueueIn = queueIn.Where(x => x.Type == TransactionActivity.FinancialPlanning).ToList();
        if (financialPlanningQueueIn.Count > 0)
        {
            // se o arquivo for do tipo FinancialPlanning -> chama a regra de processamento de controle financeiro enviado o Id da fila
            var queueInIds = financialPlanningQueueIn.Select(x => x.Id);
            //await ProcessFinancialPlanning(queueInIds);

            //financialPlanningQueueIn.ForEach(x =>
            //{
            //    x.UpdatedAt = DateTime.UtcNow;
            //});
            //await _queueInRepository.UpdateRangeAsync(financialPlanningQueueIn);
        }
    }

    private async Task ProcessingInvestment(IEnumerable<Guid> queueInIds, Guid userId)
    {
        var investmentData = await _investmentHistoryRepository.FindAsync(
            x => queueInIds.Contains(x.QueueInId) && x.DeletedAt == null);

        var boughtAssets = investmentData.Where(x =>
                x.TransactionType == InvestmentTransactionType.Transfer
                ||
                x.TransactionType == InvestmentTransactionType.SettlementTransfer);
        if (boughtAssets.Any())
        {
            var stockBrokerNames = boughtAssets.Select(x => x.Institution).Distinct();
            var stockBrokers = await StockBrokerAddRangeAsync(stockBrokerNames);

            var assetNames = boughtAssets.Select(x => x.Product).Distinct();
            var assets = await AssetsAddRangeAsync(assetNames);

            await InvestmentTransactionsAddRangeAsync(boughtAssets, stockBrokers, assets, userId);

            var queueInAssetsToDelete = boughtAssets.Select(x =>
            {
                x.UpdateByProcessing();
                return x;
            }).ToList();
        }

        // TODO: need to implement sell processing 

        // TODO: need to implement dividends processing
    }

    private async Task ProcessFinancialPlanning(IEnumerable<Guid> queueInIds)
    {
        throw new NotImplementedException();
    }

    private async Task<IEnumerable<StockBroker>> StockBrokerAddRangeAsync(IEnumerable<string> stockBrokerNames)
    {
        var stockBrokersFound = await _stockBrokerRepository.FindAsync(x => stockBrokerNames.Contains(x.Name));

        var stockBrokerNamesToCreate = stockBrokerNames.Except(stockBrokersFound.Select(x => x.Name));
        if (!stockBrokerNamesToCreate.Any())
            return stockBrokersFound;

        var stockBrokers = StockBroker.CreateRange(stockBrokerNamesToCreate);

        await _stockBrokerRepository.AddRangeAsync(stockBrokers);

        return stockBrokers;
    }

    private async Task<IEnumerable<Asset>> AssetsAddRangeAsync(IEnumerable<string> assetNames)
    {
        var assetsFound = await _assetRepository.FindAsync(x => assetNames.Contains(x.Ticker.Trim()));

        var assetNamesToCreate = assetNames.Except(assetsFound.Select(x => x.Ticker.Trim()));
        if (!assetNamesToCreate.Any())
            return assetsFound;

        var assets = Asset.CreateRange(assetNamesToCreate);

        await _assetRepository.AddRangeAsync(assets);

        return assets;
    }

    private async Task InvestmentTransactionsAddRangeAsync(IEnumerable<InvestmentHistory> boughtAssets,
        IEnumerable<StockBroker> stockBrokers,
        IEnumerable<Asset> assets,
        Guid userId)
    {
        var transactionHistories = new List<TransactionHistory>();
        var investmentTransactions = new List<InvestmentTransaction>();
        foreach (var boughtAsset in boughtAssets)
        {
            var assetId = assets.Where(x => boughtAsset.Product.Contains(x.Ticker.Trim()))
                                .Select(x => x.Id).First();
            var stockBrokerId = stockBrokers.Where(x => x.Name == boughtAsset.Institution)
                                                        .Select(x => x.Id).First();

            var transactionHistory = TransactionHistory.Create(
                userId,
                GetOperationDate(boughtAsset.Date),
                TransactionActivity.Investment);
            transactionHistories.Add(transactionHistory);

            var investmentTransaction = InvestmentTransaction.Create(
                transactionHistoryId: transactionHistory.Id,
                applicable: boughtAsset.Applicable,
                transactionType: boughtAsset.TransactionType,
                assetId,
                unitPrice: boughtAsset.UnitPrice,
                transactionAmount: boughtAsset.OperationValue,
                assetQuantity: boughtAsset.Quantity,
                stockBrokerId);
            investmentTransactions.Add(investmentTransaction);
        }

        await _transactionHistoryRepository.AddRangeAsync(transactionHistories);

        await _investmentTransactionRepository.AddRangeAsync(investmentTransactions);

        await HandleInvestmentWallet(investmentTransactions, userId);
    }

    private static DateTime GetOperationDate(DateTime dateFromFile)
    {
        DateTime operationDate = dateFromFile.AddDays(-2);

        switch (operationDate.DayOfWeek)
        {
            case DayOfWeek.Sunday:
                operationDate = dateFromFile.AddDays(-4);
                break;
            case DayOfWeek.Saturday:
                operationDate = dateFromFile.AddDays(-3);
                break;
        }

        return operationDate;
    }

    private async Task HandleInvestmentWallet(List<InvestmentTransaction> investmentTransactions, Guid userId)
    {

        var createWallets = new List<Wallet>();
        var updateWallets = new List<Wallet>();
        foreach (var transaction in investmentTransactions)
        {
            if (createWallets.Any(x => x.AssetId == transaction.AssetId))
            {
                var wallet = createWallets.Find(x => x.AssetId == transaction.AssetId);
                var newWallet = wallet;

                newWallet.UpdateAsset(transaction.AssetQuantity, transaction.AssetTransactionAmount);

                createWallets.Remove(wallet);
                createWallets.Add(newWallet);

                continue;
            }

            var walletAsset = await _walletRepository.GetByIdAsync(transaction.AssetId);
            if (walletAsset is null)
            {
                var wallet = Wallet.CreateAsset(
                    userId,
                    assetId: transaction.AssetId,
                    assetQuantity: transaction.AssetQuantity,
                    investedAmount: transaction.AssetTransactionAmount);
                createWallets.Add(wallet);

                continue;
            }

            walletAsset.UpdateAsset(transaction.AssetQuantity, transaction.AssetTransactionAmount);
            updateWallets.Add(walletAsset);
        }

        if (createWallets.Count > 0)
            await _walletRepository.AddRangeAsync(createWallets);

        if (updateWallets.Count > 0)
            await _walletRepository.UpdateRangeAsync(updateWallets);
    }
    #endregion
}
