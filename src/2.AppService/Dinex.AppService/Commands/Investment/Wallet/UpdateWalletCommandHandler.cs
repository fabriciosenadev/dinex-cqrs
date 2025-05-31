namespace AppService;

public class UpdateWalletCommandHandler : ICommandHandler, IRequestHandler<UpdateWalletCommand, OperationResult<Guid>>
{
    private readonly IWalletRepository _walletRepository;

    public UpdateWalletCommandHandler(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task<OperationResult<Guid>> Handle(UpdateWalletCommand request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Guid>();

        var wallet = await _walletRepository.GetByIdAsync(request.Id);
        if (wallet is null)
        {
            result.AddError("Carteira não encontrada.");
            return result;
        }

        wallet.EnsureNotDeleted("Wallet");

        wallet.Update(
            request.Name,
            request.DefaultCurrency,
            request.Description
        );

        if (!wallet.IsValid)
        {
            result.AddNotifications(wallet.Notifications);
            return result;
        }

        await _walletRepository.UpdateAsync(wallet);
        result.SetData(wallet.Id);

        return result;
    }
}
