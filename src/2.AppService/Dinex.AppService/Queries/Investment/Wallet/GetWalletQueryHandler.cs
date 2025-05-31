namespace Dinex.AppService;

public class GetWalletQueryHandler : IQueryHandler, IRequestHandler<GetWalletQuery, OperationResult<WalletDTO>>
{
    private readonly IWalletRepository _walletRepository;

    public GetWalletQueryHandler(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task<OperationResult<WalletDTO>> Handle(GetWalletQuery request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<WalletDTO>();

        var wallet = await _walletRepository.GetByIdAsync(request.WalletId);
        if (wallet == null)
            return result.AddError("Wallet not found").SetAsNotFound();

        wallet.EnsureNotDeleted("Wallet");

        result.SetData(new WalletDTO
        {
            Id = wallet.Id,
            UserId = wallet.UserId,
            Name = wallet.Name,
            Description = wallet.Description,
            DefaultCurrency = wallet.DefaultCurrency
        });

        return result;
    }
}
