namespace Dinex.Core;

public abstract class Entity : BaseEntity, INotifiableEntity
{
    // alocação sob demanda
    private List<Notification>? _notifications;

    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime? UpdatedAt { get; protected set; }
    public DateTime? DeletedAt { get; protected set; }

    public IReadOnlyCollection<Notification> Notifications
        => (IReadOnlyCollection<Notification>?)_notifications ?? Array.Empty<Notification>();

    public bool IsValid => _notifications == null || _notifications.Count == 0;

    // ---- NOVO: aceita Contract / Contract<T> / qualquer Notifiable ----
    protected void AddNotifications(Notifiable<Notification>? source)
    {
        if (source == null) return;
        var list = source.Notifications;
        if (list == null || list.Count == 0) return;

        _notifications ??= new List<Notification>(list.Count);
        _notifications.AddRange(list);
    }

    // overload para vários "sources"
    protected void AddNotifications(params Notifiable<Notification>?[] sources)
    {
        if (sources == null) return;
        foreach (var s in sources) AddNotifications(s);
    }

    // overload para IEnumerable<Notification> (se um dia precisar)
    protected void AddNotifications(IEnumerable<Notification>? notifications)
    {
        if (notifications == null) return;
        foreach (var n in notifications)
            AddNotification(n);
    }
    // -------------------------------------------------------------------

    protected void AddNotification(string key, string message)
    {
        _notifications ??= new List<Notification>(1);
        _notifications.Add(new Notification(key, message));
    }

    protected void AddNotification(Notification notification)
    {
        _notifications ??= new List<Notification>(1);
        _notifications.Add(notification);
    }

    public void ClearNotifications() => _notifications?.Clear();

    public void MarkAsDeleted() => DeletedAt = DateTime.UtcNow;

    public bool IsDeleted() => DeletedAt.HasValue;

    public void EnsureNotDeleted(string entityName)
    {
        if (IsDeleted())
            AddNotification(entityName, $"A entidade {entityName} foi excluída e não pode mais ser utilizada.");
    }

    public void Restore()
    {
        DeletedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

}