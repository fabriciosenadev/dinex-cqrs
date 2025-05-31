namespace Dinex.AppService;

public record ListExpensesQuery() : IRequest<OperationResult<IEnumerable<string>>>;

public class ListExpensesQueryHandler : IQueryHandler, IRequestHandler<ListExpensesQuery, OperationResult<IEnumerable<string>>>
{
    public Task<OperationResult<IEnumerable<string>>> Handle(ListExpensesQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new OperationResult<IEnumerable<string>>(new[] { "Expense A", "Expense B" }));
    }
}
