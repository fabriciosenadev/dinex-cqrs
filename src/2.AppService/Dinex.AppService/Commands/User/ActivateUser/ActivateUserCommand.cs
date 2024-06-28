namespace Dinex.AppService;
public class ActivateUserCommand: IRequest<OperationResult>
{
    public string Email { get; set; }
    public string ActivationCode { get; set; }
}

