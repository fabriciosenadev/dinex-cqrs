namespace Dinex.AppService;

public interface ISendEmailService
{
    Task Execute(SendEmailModel sendEmailModel);
    string GetEmailMessage(EmailMessageModel emailBodyModel);
}
