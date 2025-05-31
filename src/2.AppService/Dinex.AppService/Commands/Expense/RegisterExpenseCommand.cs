namespace Dinex.AppService;

public record RegisterExpenseCommand(string Description, decimal Amount, DateTime Date) : IRequest<OperationResult<Guid>>;

public class RegisterExpenseCommandHandler : ICommandHandler, IRequestHandler<RegisterExpenseCommand, OperationResult<Guid>>
{
    public Task<OperationResult<Guid>> Handle(RegisterExpenseCommand request, CancellationToken cancellationToken)
    {
        // logic to register expense
        return Task.FromResult(new OperationResult<Guid>(Guid.NewGuid()));
    }
}
