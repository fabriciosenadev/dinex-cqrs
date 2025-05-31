namespace Dinex.AppService;

public class DeleteOperationCommandHandler : ICommandHandler, IRequestHandler<DeleteOperationCommand, OperationResult<bool>>
{
    private readonly IOperationRepository _operationRepository;

    public DeleteOperationCommandHandler(IOperationRepository operationRepository)
    {
        _operationRepository = operationRepository;
    }

    public async Task<OperationResult<bool>> Handle(DeleteOperationCommand request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<bool>();

        var operation = await _operationRepository.GetByIdAsync(request.Id);
        if (operation is null)
        {
            result.AddError("Operação não encontrada.");
            return result;
        }

        operation.MarkAsDeleted();

        await _operationRepository.UpdateAsync(operation);
        result.SetData(true);

        return result;
    }
}
