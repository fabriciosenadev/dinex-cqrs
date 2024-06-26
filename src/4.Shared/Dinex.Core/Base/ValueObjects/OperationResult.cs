namespace Dinex.Core;

public class OperationResult : OperationResultBase<OperationResult>
{
    public OperationResult()
    {

    }

    public OperationResult(IReadOnlyCollection<Notification> erros)
    {
        AddNotifications(erros);
    }

}


public class OperationResult<T> : OperationResultBase<OperationResult<T>>
{
    public T Data { get; set; }

    public OperationResult()
    {

    }

    public OperationResult(T data)
    {
        Data = data;
    }

    public OperationResult(T data, IReadOnlyCollection<Notification> erros)
    {
        Data = data;
        AddNotifications(erros);
    }

    public OperationResult<T> SetData(T data)
    {
        Data = data;
        return this as OperationResult<T>;
    }

}

public abstract class OperationResultBase<OP> : ValueObject
where OP : OperationResultBase<OP>
{
    private bool _notFound { get; set; }
    private bool _internalServerError { get; set; }
    public IEnumerable<string> Errors { get => Notifications.Select(x => x.Message); }

    public bool Succeded => IsValid && !IsNotFound && !InternalServerError;

    public bool IsNotFound => _notFound;
    public bool InternalServerError => _internalServerError;

    public OP AddError(string erroMsg)
    {
        AddNotification("", erroMsg);
        return this as OP;
    }

    public OP AddError(Notification erroMsg)
    {
        AddNotification(erroMsg);
        return this as OP;
    }


    public OP AddErrors(IEnumerable<string> erroMsg)
    {
        IList<Notification> notifications = erroMsg.Select(x => new Notification("", x)).ToList();
        AddNotifications(notifications);
        return this as OP;
    }

    public OP AddErrors(IList<Notification> erroMsgs)
    {
        AddNotifications(erroMsgs);
        return this as OP;
    }

    public OP SetAsNotFound()
    {
        _notFound = true;
        return this as OP;
    }

    public OP SetAsInternalServerError()
    {
        _internalServerError = true;
        return this as OP;
    }

    public bool HasErrors()
    {
        return Errors.Any();
    }
}


