namespace Dinex.AppService;

public class DeleteWalletCommandHandler : ICommandHandler, IRequestHandler<DeleteWalletCommand, OperationResult<bool>>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IPositionRepository _positionRepository;
    private readonly IOperationRepository _operationRepository;

    public DeleteWalletCommandHandler(
        IWalletRepository walletRepository,
        IPositionRepository positionRepository,
        IOperationRepository operationRepository)
    {
        _walletRepository = walletRepository;
        _positionRepository = positionRepository;
        _operationRepository = operationRepository;
    }

    public async Task<OperationResult<bool>> Handle(DeleteWalletCommand request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<bool>();

        var wallet = await _walletRepository.GetByIdAsync(request.Id);
        if (wallet is null)
        {
            result.AddError("Carteira não encontrada.");
            return result;
        }

        var positions = await _positionRepository.GetByWalletAsync(wallet.Id);
        foreach (var position in positions)
        {
            position.MarkAsDeleted();
            await _positionRepository.UpdateAsync(position);
        }

        var operations = await _operationRepository.GetByWalletAsync(wallet.Id);
        foreach (var operation in operations)
        {
            operation.MarkAsDeleted();
            await _operationRepository.UpdateAsync(operation);
        }

        wallet.MarkAsDeleted();
        await _walletRepository.UpdateAsync(wallet);

        result.SetData(true);
        return result;
    }
}
