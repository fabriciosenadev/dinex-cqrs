namespace Dinex.AppService;

public class GetPositionQuery : IRequest<OperationResult<PositionDTO>>
{
    public Guid PositionId { get; set; }

    public GetPositionQuery(Guid positionId)
    {
        PositionId = positionId;
    }
}
