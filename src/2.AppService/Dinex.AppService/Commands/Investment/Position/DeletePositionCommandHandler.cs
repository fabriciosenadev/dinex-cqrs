namespace Dinex.AppService;

public class DeletePositionCommandHandler : ICommandHandler, IRequestHandler<DeletePositionCommand, OperationResult<bool>>
{
    private readonly IPositionRepository _positionRepository;

    public DeletePositionCommandHandler(IPositionRepository positionRepository)
    {
        _positionRepository = positionRepository;
    }

    public async Task<OperationResult<bool>> Handle(DeletePositionCommand request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<bool>();

        var position = await _positionRepository.GetByIdAsync(request.Id);
        if (position is null)
        {
            result.AddError("Posição não encontrada.");
            return result;
        }

        position.MarkAsDeleted();

        await _positionRepository.UpdateAsync(position);
        result.SetData(true);

        return result;
    }
}
