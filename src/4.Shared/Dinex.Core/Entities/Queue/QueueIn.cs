namespace Dinex.Core;

public class QueueIn : Entity
{
    public Guid UserId { get; private set; }
    public TransactionActivity Type { get; private set; }
    public string FileName { get; private set; }

    private QueueIn (
        Guid userId, 
        TransactionActivity type, 
        string fileName,
        DateTime createdAt,
        DateTime? updatedAt,
        DateTime? deletedAt)
    {
        UserId = userId;
        Type = type;
        FileName = fileName;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        DeletedAt = deletedAt;
    }
    public static QueueIn Create(Guid userId, TransactionActivity transactionActivity, string fileName)
    {
        var filePrefix = Guid.NewGuid();

        var newQueueIn = new QueueIn(
            userId,
            type: transactionActivity,
            fileName: $"{filePrefix}_{fileName}",
            createdAt: DateTime.UtcNow,
            updatedAt: null,
            deletedAt: null);

        return newQueueIn;
    }
}
