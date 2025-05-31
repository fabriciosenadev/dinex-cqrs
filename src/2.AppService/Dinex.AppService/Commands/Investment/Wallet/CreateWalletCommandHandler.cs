namespace Dinex.AppService;

public class CreateWalletCommandHandler : ICommandHandler, IRequestHandler<CreateWalletCommand, OperationResult<Guid>>
{
    private readonly IWalletRepository _walletRepository;

    public CreateWalletCommandHandler(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task<OperationResult<Guid>> Handle(CreateWalletCommand request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Guid>();

        var wallet = Wallet.Create(
            request.UserId,
            request.Name,
            request.DefaultCurrency,
            request.Description
        );

        if (!wallet.IsValid)
        {
            result.AddNotifications(wallet.Notifications);
            return result;
        }

        await _walletRepository.AddAsync(wallet);
        result.SetData(wallet.Id);

        return result;
    }
}
