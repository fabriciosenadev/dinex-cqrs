namespace Dinex.AppService;

public class LoginUserCommand : IRequest<OperationResult<AuthenticationUserDTO>>
{
    public string Email { get; set; }
    public string Password { get; set; }
}
