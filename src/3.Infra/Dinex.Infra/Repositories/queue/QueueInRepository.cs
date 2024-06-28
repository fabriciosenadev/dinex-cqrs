namespace Dinex.Infra;

public interface IQueueInRepository : IRepository<QueueIn>
{   
}

internal class QueueInRepository : Repository<QueueIn>, IQueueInRepository
{
    public QueueInRepository(DinexApiContext context) : base(context)
    {
    }
}
