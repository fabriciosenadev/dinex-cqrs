namespace Dinex.AppService;

public class DeleteAssetCommandHandler : ICommandHandler, IRequestHandler<DeleteAssetCommand, OperationResult<bool>>
{
    private readonly IAssetRepository _assetRepository;

    public DeleteAssetCommandHandler(IAssetRepository assetRepository)
    {
        _assetRepository = assetRepository;
    }

    public async Task<OperationResult<bool>> Handle(DeleteAssetCommand request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<bool>();

        var asset = await _assetRepository.GetByIdAsync(request.Id);
        if (asset is null)
        {
            result.AddError("Ativo não encontrado.");
            return result;
        }

        asset.MarkAsDeleted();

        await _assetRepository.UpdateAsync(asset);
        result.SetData(true);

        return result;
    }
}
