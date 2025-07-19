namespace Dinex.AppService;

public class GetAllOperationsByWalletQuery : IRequest<OperationResult<PagedResult<OperationDTO>>>
{
    public Guid WalletId { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public GetAllOperationsByWalletQuery(Guid walletId)
    {
        WalletId = walletId;
    }
}
