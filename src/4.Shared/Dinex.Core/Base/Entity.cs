﻿namespace Dinex.Core;

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

    public void MarkAsDeleted()
    {
        DeletedAt = DateTime.UtcNow;
    }

    public bool IsDeleted()
    {
        return DeletedAt.HasValue;
    }

    public void EnsureNotDeleted(string entityName)
    {
        if (IsDeleted())
            AddNotification(entityName, $"A entidade {entityName} foi excluída e não pode mais ser utilizada.");
    }
}
