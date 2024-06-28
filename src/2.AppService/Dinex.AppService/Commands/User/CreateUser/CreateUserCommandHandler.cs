using Dinex.Core;

namespace Dinex.AppService;

public class CreateUserCommandHandler : ICommandHandler, IRequestHandler<CreateUserCommand, OperationResult<Guid>>
{
    private const int DefaultCodeLength = 32;

    private readonly IUserRepository _userRepository;

    private readonly IAccountService _accountService;
    private readonly ISendEmailService _sendEmailService;

    public CreateUserCommandHandler(IUserRepository userRepository, IAccountService accountService, ISendEmailService sendEmailService)
    {
        _userRepository = userRepository;
        _accountService = accountService;
        _sendEmailService = sendEmailService;
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

        var activationCode = _accountService.GenerateCode(DefaultCodeLength);

        newUser.AssingActivationCode(activationCode);

        await _userRepository.AddAsync(newUser);

        result.SetData(newUser.Id);

        #region email to request activation link
        var emailMessageModel = new EmailMessageModel
        {
            DestinationName = newUser.Email,
            GeneratedCode = newUser.ActivationCode,
            Origin = "activation",
            EmailTemplateFileName = "activationAccount.html",
            TemplateFieldToName = "{name}",
            TemplateFieldToUrl = "{activationUrl}"
        };
        var emailMessage = _sendEmailService.GetEmailMessage(emailMessageModel);

        var sendEmail = new SendEmailModel
        {
            DestinationEmailAddress = newUser.Email,
            DestinationName = newUser.FullName,
            DestinationSubject = "Ativação de conta",
            EmailMessage = emailMessage,
            IsHtml = true
        };
        _sendEmailService.Execute(sendEmail);
        #endregion

        return result;
    }
}
