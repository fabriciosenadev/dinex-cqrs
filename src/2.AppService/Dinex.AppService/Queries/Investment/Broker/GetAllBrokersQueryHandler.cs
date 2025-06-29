namespace Dinex.AppService;

public class GetAllBrokersQueryHandler : IQueryHandler, IRequestHandler<GetAllBrokersQuery, OperationResult<IEnumerable<BrokerDTO>>>
{
    private readonly IBrokerRepository _brokerRepository;

    public GetAllBrokersQueryHandler(IBrokerRepository brokerRepository)
    {
        _brokerRepository = brokerRepository;
    }

    public async Task<OperationResult<IEnumerable<BrokerDTO>>> Handle(GetAllBrokersQuery request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<IEnumerable<BrokerDTO>>();

        var brokers = await _brokerRepository.GetAllAsync();

        var activeBrokers = brokers
            .Where(x => x.DeletedAt == null)
            .Select(broker => new BrokerDTO
            {
                Id = broker.Id,
                Name = broker.Name,
                Cnpj = broker.Cnpj,
                Website = broker.Website
            });

        result.SetData(activeBrokers);
        return result;
    }
}
