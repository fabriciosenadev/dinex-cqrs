namespace AppService;

public class CreateOperationCommandHandler : ICommandHandler, IRequestHandler<CreateOperationCommand, OperationResult<Guid>>
{
    private readonly IOperationRepository _operationRepository;

    public CreateOperationCommandHandler(IOperationRepository operationRepository)
    {
        _operationRepository = operationRepository;
    }

    public async Task<OperationResult<Guid>> Handle(CreateOperationCommand request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Guid>();

        var operation = Operation.Create(
            request.WalletId,
            request.AssetId,
            request.Type,
            request.Quantity,
            request.UnitPrice,
            request.ExecutedAt,
            request.BrokerId
        );

        if (!operation.IsValid)
        {
            result.AddNotifications(operation.Notifications);
            return result;
        }

        await _operationRepository.AddAsync(operation);
        result.SetData(operation.Id);

        return result;
    }
}
