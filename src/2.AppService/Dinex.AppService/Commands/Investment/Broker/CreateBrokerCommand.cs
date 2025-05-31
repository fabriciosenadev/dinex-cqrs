namespace Dinex.AppService;

public record CreateBrokerCommand(
    string Name,
    string Cnpj,
    string? Website
) : IRequest<OperationResult<Guid>>;
