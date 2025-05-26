using AppService;

namespace Dinex.AppService;

public class UpdateAssetCommandHandler : ICommandHandler, IRequestHandler<UpdateAssetCommand, OperationResult<Guid>>
{
    private readonly IAssetRepository _assetRepository;

    public UpdateAssetCommandHandler(IAssetRepository assetRepository)
    {
        _assetRepository = assetRepository;
    }

    public async Task<OperationResult<Guid>> Handle(UpdateAssetCommand request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Guid>();

        var asset = await _assetRepository.GetByIdAsync(request.Id);
        if (asset is null)
        {
            result.AddError("Ativo não encontrado.");
            return result;
        }

        asset.EnsureNotDeleted("Asset");

        if (!asset.IsValid)
        {
            result.AddNotifications(asset.Notifications);
            return result;
        }

        asset.Update(
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

        await _assetRepository.UpdateAsync(asset);
        result.SetData(asset.Id);

        return result;
    }
}
