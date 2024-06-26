namespace Dinex.AppService;

public class CreateUserCommandHandler : ICommandHandler, IRequestHandler<CreateUserCommand, OperationResult<Guid>>
{
    private readonly IUserRepository _userRepository;

    public CreateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<OperationResult<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Guid>();

        if (_userRepository.FindAsync(user => user.Email == request.Email).Result.Any())
        {
            result.AddError("Já existe um usuário com este E-mail");
            return result;
        }

        var newUser = User.CreateUser(
                fullName: request.FullName,
                email: request.Email,
                password: request.Password,
                confirmPassword: request.ConfirmPassword);
        if (!newUser.IsValid)
        {
            result.AddNotifications(newUser.Notifications);
            return result;
        }

        await _userRepository.AddAsync(newUser);

        result.SetData(newUser.Id);

        return result;
    }
}
