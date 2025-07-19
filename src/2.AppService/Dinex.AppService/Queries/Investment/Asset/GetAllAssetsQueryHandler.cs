

namespace Dinex.AppService;

public class GetAllAssetsQueryHandler : IQueryHandler, IRequestHandler<GetAllAssetsQuery, OperationResult<PagedResult<AssetDTO>>>
{
    private readonly IAssetRepository _assetRepository;

    public GetAllAssetsQueryHandler(IAssetRepository assetRepository)
    {
        _assetRepository = assetRepository;
    }

    public async Task<OperationResult<PagedResult<AssetDTO>>> Handle(GetAllAssetsQuery request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<PagedResult<AssetDTO>>();

        // Filtro dinâmico (exemplo básico, pode ser expandido para outros campos)
        Expression<Func<Asset, bool>>? filter = asset => asset.DeletedAt == null;

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            // Combinando filtros (Name e ativos não deletados)
            filter = asset => asset.DeletedAt == null && asset.Name.Contains(request.Name);
        }

        // Ordenação pode ser passada se quiser customizar, senão usa CreatedAt do repo
        PagedResult<Asset> pagedAssets = await _assetRepository.GetPagedAsync(
            filter,
            request.Page,
            request.PageSize
        );

        // Projeção para DTO já paginado
        var dtoPagedResult = new PagedResult<AssetDTO>
        {
            Items = pagedAssets.Items.Select(asset => new AssetDTO
            {
                Id = asset.Id,
                Name = asset.Name,
                Code = asset.Code,
                Cnpj = asset.Cnpj,
                Exchange = asset.Exchange.ToString(),
                Currency = asset.Currency.ToString(),
                Type = asset.Type.ToString(),
                Sector = asset.Sector
            }),
            TotalCount = pagedAssets.TotalCount,
            Page = pagedAssets.Page,
            PageSize = pagedAssets.PageSize
        };

        result.SetData(dtoPagedResult);
        return result;
    }

}
