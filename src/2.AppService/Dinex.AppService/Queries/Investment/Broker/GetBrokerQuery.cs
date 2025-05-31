namespace Dinex.AppService;

public class GetBrokerQuery : IRequest<OperationResult<BrokerDTO>>
{
    public Guid BrokerId { get; set; }

    public GetBrokerQuery(Guid brokerId)
    {
        BrokerId = brokerId;
    }
}
