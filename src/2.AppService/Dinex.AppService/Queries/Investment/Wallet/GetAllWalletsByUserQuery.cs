namespace Dinex.AppService;

public class GetAllWalletsByUserQuery : IRequest<OperationResult<IEnumerable<WalletDTO>>>
{
    public Guid UserId { get; set; }

    public GetAllWalletsByUserQuery(Guid userId)
    {
        UserId = userId;
    }
}
