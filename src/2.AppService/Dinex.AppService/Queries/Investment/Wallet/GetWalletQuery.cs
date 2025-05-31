namespace Dinex.AppService;

public class GetWalletQuery : IRequest<OperationResult<WalletDTO>>
{
    public Guid WalletId { get; set; }

    public GetWalletQuery(Guid walletId)
    {
        WalletId = walletId;
    }
}
