namespace Dinex.Core;

public class TransactionHistory : Entity
{
    public Guid UserId { get; private set; }
    public DateTime Date { get; private set; }
    public TransactionActivity Activity { get; private set; }

    private TransactionHistory(
        Guid userId,
        DateTime date,
        TransactionActivity activity,
        DateTime createdAt,
        DateTime? updatedAt,
        DateTime? deletedAt)
    {
        UserId = userId;
        Date = date;
        Activity = activity;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        DeletedAt = deletedAt;
    }

    public static TransactionHistory Create(Guid userId, DateTime date, TransactionActivity activity)
    {
        var newTransactionHistory = new TransactionHistory(
            userId,
            date,
            activity,
            DateTime.UtcNow,
            null,
            null);

        return newTransactionHistory;
    }
}
