namespace AppService;

public record UpdateBrokerCommand(
    Guid Id,
    string Name,
    string Cnpj,
    string? Website
) : IRequest<OperationResult<Guid>>;
