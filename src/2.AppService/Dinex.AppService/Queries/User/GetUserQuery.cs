namespace Dinex.AppService;

public class GetUserQuery : IRequest<OperationResult<UserDTO>>
{
    public Guid UserId { get; set; }
}
