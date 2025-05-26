namespace Dinex.AppService;

public class CreateAssetCommandHandler : ICommandHandler, IRequestHandler<CreateAssetCommand, OperationResult<Guid>>
{
    private readonly IAssetRepository _assetRepository;

    public CreateAssetCommandHandler(IAssetRepository assetRepository)
    {
        _assetRepository = assetRepository;
    }

    public async Task<OperationResult<Guid>> Handle(CreateAssetCommand request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Guid>();

        var asset = Asset.Create(
            request.Name,
            request.Code,
            request.Cnpj,
            request.Exchange,
            request.Currency,
            request.Type,
            request.Sector
        );

        if (!asset.IsValid)
        {
            result.AddNotifications(asset.Notifications);
            return result;
        }

        await _assetRepository.AddAsync(asset);
        result.SetData(asset.Id);

        return result;
    }
}
