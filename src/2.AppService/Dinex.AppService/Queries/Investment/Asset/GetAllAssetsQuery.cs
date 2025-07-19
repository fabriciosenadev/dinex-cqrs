namespace Dinex.AppService;

public class GetAllAssetsQuery : IRequest<OperationResult<PagedResult<AssetDTO>>>
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Cnpj { get; set; }
    public string? Sector { get; set; }
    public Exchange? Exchange { get; set; }
    public Currency? Currency { get; set; }
    public AssetType? Type { get; set; }

    // Paginação
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
