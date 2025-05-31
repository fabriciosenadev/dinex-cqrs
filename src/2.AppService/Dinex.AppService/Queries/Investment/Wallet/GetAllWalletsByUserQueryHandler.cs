namespace Dinex.AppService;

public class GetAllWalletsByUserQueryHandler : IQueryHandler, IRequestHandler<GetAllWalletsByUserQuery, OperationResult<IEnumerable<WalletDTO>>>
{
    private readonly IWalletRepository _walletRepository;

    public GetAllWalletsByUserQueryHandler(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task<OperationResult<IEnumerable<WalletDTO>>> Handle(GetAllWalletsByUserQuery request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<IEnumerable<WalletDTO>>();

        var wallets = await _walletRepository.GetByUserAsync(request.UserId);

        var filtered = wallets
            .Where(x => x.DeletedAt == null)
            .Select(wallet => new WalletDTO
            {
                Id = wallet.Id,
                UserId = wallet.UserId,
                Name = wallet.Name,
                Description = wallet.Description,
                DefaultCurrency = wallet.DefaultCurrency
            });

        result.SetData(filtered);
        return result;
    }
}
