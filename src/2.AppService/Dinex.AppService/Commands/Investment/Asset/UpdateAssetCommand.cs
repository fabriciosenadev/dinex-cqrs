namespace AppService;

public record UpdateAssetCommand(
    Guid Id,
    string Name,
    string Code,
    string? Cnpj,
    Exchange Exchange,
    Currency Currency,
    AssetType Type,
    string? Sector
) : IRequest<OperationResult<Guid>>;
