namespace Dinex.AppService;

public class GetBrokerQueryHandler : IQueryHandler, IRequestHandler<GetBrokerQuery, OperationResult<BrokerDTO>>
{
    private readonly IBrokerRepository _brokerRepository;

    public GetBrokerQueryHandler(IBrokerRepository brokerRepository)
    {
        _brokerRepository = brokerRepository;
    }

    public async Task<OperationResult<BrokerDTO>> Handle(GetBrokerQuery request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<BrokerDTO>();

        var broker = await _brokerRepository.GetByIdAsync(request.BrokerId);
        if (broker == null)
            return result.AddError("Broker not found").SetAsNotFound();

        broker.EnsureNotDeleted("Broker");

        result.SetData(new BrokerDTO
        {
            Id = broker.Id,
            Name = broker.Name,
            Cnpj = broker.Cnpj
        });

        return result;
    }
}
