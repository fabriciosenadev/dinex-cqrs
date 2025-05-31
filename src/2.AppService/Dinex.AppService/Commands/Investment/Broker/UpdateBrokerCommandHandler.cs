using AppService;

namespace Dinex.AppService;

public class UpdateBrokerCommandHandler : ICommandHandler, IRequestHandler<UpdateBrokerCommand, OperationResult<Guid>>
{
    private readonly IBrokerRepository _brokerRepository;

    public UpdateBrokerCommandHandler(IBrokerRepository brokerRepository)
    {
        _brokerRepository = brokerRepository;
    }

    public async Task<OperationResult<Guid>> Handle(UpdateBrokerCommand request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Guid>();

        var broker = await _brokerRepository.GetByCnpjAsync(request.Cnpj);
        if (broker is null || broker.Id != request.Id)
        {
            result.AddError("Corretora não encontrada.");
            return result;
        }

        broker.EnsureNotDeleted("Broker");

        if (!broker.IsValid)
        {
            result.AddNotifications(broker.Notifications);
            return result;
        }

        broker.Update(request.Name, request.Cnpj, request.Website);

        if (!broker.IsValid)
        {
            result.AddNotifications(broker.Notifications);
            return result;
        }

        await _brokerRepository.UpdateAsync(broker);
        result.SetData(broker.Id);

        return result;
    }
}
