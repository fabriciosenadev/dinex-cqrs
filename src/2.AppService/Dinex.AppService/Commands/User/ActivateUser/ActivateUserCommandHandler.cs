
namespace Dinex.AppService;

public class ActivateUserCommandHandler : ICommandHandler, IRequestHandler<ActivateUserCommand, OperationResult>
{
    private readonly IUserRepository _userRepository;

    public ActivateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<OperationResult> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = new OperationResult();

            if (string.IsNullOrWhiteSpace(request.ActivationCode))
            {
                result.AddError("Código de ativação deve ser fornecido");
                return result;
            }

            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user is null)
            {
                result.AddError("Usuário não encontrado").SetAsNotFound();
                return result;
            }

            if (!user.HasValidActivationCode() && user.ActivationCode != request.ActivationCode)
            {
                result.AddError("Código de ativação inválido ou fora da validade, solicite outro código");
                return result;
            }

            user.Activate();
            await _userRepository.UpdateUserAsync(user);

            return result;
        }
        catch (Exception ex)
        {

            return new OperationResult().SetAsInternalServerError()
                        .AddError($"An unexpected error ocurred to method: ActivateUser {ex}");
        }
    }
}
