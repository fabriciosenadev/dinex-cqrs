namespace Dinex.AppService;

public record CreateAssetCommand(
    string Name,
    string Code,
    string? Cnpj,
    Exchange Exchange,
    Currency Currency,
    AssetType Type,
    string? Sector
) : IRequest<OperationResult<Guid>>;
