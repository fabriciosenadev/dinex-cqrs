namespace Dinex.AppService;

public class CreateBrokerCommandHandler : ICommandHandler, IRequestHandler<CreateBrokerCommand, OperationResult<Guid>>
{
    private readonly IBrokerRepository _brokerRepository;

    public CreateBrokerCommandHandler(IBrokerRepository brokerRepository)
    {
        _brokerRepository = brokerRepository;
    }

    public async Task<OperationResult<Guid>> Handle(CreateBrokerCommand request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Guid>();

        var broker = Broker.Create(
            request.Name,
            request.Cnpj,
            request.Website
        );

        if (!broker.IsValid)
        {
            result.AddNotifications(broker.Notifications);
            return result;
        }

        await _brokerRepository.AddAsync(broker);
        result.SetData(broker.Id);

        return result;
    }
}
