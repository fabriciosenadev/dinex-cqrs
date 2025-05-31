using AppService;

namespace Dinex.AppService;

public class UpdatePositionCommandHandler : ICommandHandler, IRequestHandler<UpdatePositionCommand, OperationResult<Guid>>
{
    private readonly IPositionRepository _positionRepository;

    public UpdatePositionCommandHandler(IPositionRepository positionRepository)
    {
        _positionRepository = positionRepository;
    }

    public async Task<OperationResult<Guid>> Handle(UpdatePositionCommand request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Guid>();

        var position = await _positionRepository.GetByWalletAndAssetAsync(request.WalletId, request.AssetId);
        if (position is null || position.Id != request.Id)
        {
            result.AddError("Posição não encontrada.");
            return result;
        }

        position.EnsureNotDeleted("Position");

        if (!position.IsValid)
        {
            result.AddNotifications(position.Notifications);
            return result;
        }

        position.Update(
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

        await _positionRepository.UpdateAsync(position);
        result.SetData(position.Id);

        return result;
    }
}
