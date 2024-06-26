namespace Dinex.Core;

public abstract class Entity : Notifiable<Notification>
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public DateTime? DeletedAt { get; protected set; }

    protected Entity()
    {
        Id = Guid.NewGuid();
    }
}
