namespace Dinex.AppService;

public class GetOperationQuery : IRequest<OperationResult<OperationDTO>>
{
    public Guid OperationId { get; set; }

    public GetOperationQuery(Guid operationId)
    {
        OperationId = operationId;
    }
}
