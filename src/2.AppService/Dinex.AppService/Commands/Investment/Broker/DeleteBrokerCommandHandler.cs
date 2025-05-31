namespace Dinex.AppService;

public class DeleteBrokerCommandHandler : ICommandHandler, IRequestHandler<DeleteBrokerCommand, OperationResult<bool>>
{
    private readonly IBrokerRepository _brokerRepository;

    public DeleteBrokerCommandHandler(IBrokerRepository brokerRepository)
    {
        _brokerRepository = brokerRepository;
    }

    public async Task<OperationResult<bool>> Handle(DeleteBrokerCommand request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<bool>();

        var broker = await _brokerRepository.GetByIdAsync(request.Id);
        if (broker is null)
        {
            result.AddError("Corretora não encontrada.");
            return result;
        }

        broker.MarkAsDeleted();

        await _brokerRepository.UpdateAsync(broker);
        result.SetData(true);

        return result;
    }
}
