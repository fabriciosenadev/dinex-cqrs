namespace Dinex.AppService;

public class GetAllPositionsByWalletQuery : IRequest<OperationResult<IEnumerable<PositionDTO>>>
{
    public Guid WalletId { get; set; }

    public GetAllPositionsByWalletQuery(Guid walletId)
    {
        WalletId = walletId;
    }
}
