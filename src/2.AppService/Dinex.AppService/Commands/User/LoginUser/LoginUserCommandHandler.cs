namespace Dinex.AppService;

public class LoginUserCommandHandler : ICommandHandler, IRequestHandler<LoginUserCommand, OperationResult<AuthenticationUserDTO>>
{
    private readonly IUserRepository _userRepository;
    private readonly IAccountService _accountService;

    public LoginUserCommandHandler(IUserRepository userRepository, IAccountService accountService)
    {
        _userRepository = userRepository;
        _accountService = accountService;
    }

    public async Task<OperationResult<AuthenticationUserDTO>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
		try
		{
            var result = new OperationResult<AuthenticationUserDTO>();

            var user = (await _userRepository.FindAsync(x => x.Email == request.Email)).FirstOrDefault();
            if (user is null)
            {
                result.AddError("Usuário não encontrado").SetAsNotFound();
                return result;
            }

            if (user.UserStatus == UserStatus.Inactive)
            {
                result.AddError("Conta inativa");
                return result;
            }

            if (!User.MatchValues(user.Password, request.Password))
            {
                result.AddError("Login ou senha incorretos");
                return result;
            }

            var token = _accountService.GenerateToken(user);
            result.SetData(new AuthenticationUserDTO(user, token));
            return result;

        }
        catch (Exception ex)
		{
            return new OperationResult<AuthenticationUserDTO>()
                .SetAsInternalServerError()
                .AddError($"An unexpected error ocurred to method: AuthenticateAsync {ex}");
        }
    }
}
