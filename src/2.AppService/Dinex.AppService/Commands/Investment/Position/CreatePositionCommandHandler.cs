namespace Dinex.AppService;

public class CreatePositionCommandHandler : ICommandHandler, IRequestHandler<CreatePositionCommand, OperationResult<Guid>>
{
    private readonly IPositionRepository _positionRepository;

    public CreatePositionCommandHandler(IPositionRepository positionRepository)
    {
        _positionRepository = positionRepository;
    }

    public async Task<OperationResult<Guid>> Handle(CreatePositionCommand request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Guid>();

        var position = Position.Create(
            request.WalletId,
            request.AssetId,
            request.Quantity,
            request.AveragePrice,
            request.InvestedValue,
            request.BrokerId
        );

        if (!position.IsValid)
        {
            result.AddNotifications(position.Notifications);
            return result;
        }

        await _positionRepository.AddAsync(position);
        result.SetData(position.Id);

        return result;
    }
}
