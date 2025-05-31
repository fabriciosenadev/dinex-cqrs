namespace Dinex.AppService;

public class GetAllOperationsByWalletQuery : IRequest<OperationResult<IEnumerable<OperationDTO>>>
{
    public Guid WalletId { get; set; }

    public GetAllOperationsByWalletQuery(Guid walletId)
    {
        WalletId = walletId;
    }
}
