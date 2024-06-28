namespace Dinex.Core;

public class QueueIn : Entity
{
    public Guid UserId { get; private set; }
    public TransactionActivity Type { get; private set; }
    public string FileName { get; private set; }

    public static QueueIn Create(Guid userId, TransactionActivity transactionActivity, string fileName)
    {
        var filePrefix = Guid.NewGuid();

        var newQueueIn = new QueueIn()
        {
            UserId = userId,
            Type = transactionActivity,
            FileName = $"{filePrefix}_{fileName}",
            CreatedAt = DateTime.UtcNow,
        };

        return newQueueIn;
    }
}
