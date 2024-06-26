using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Net;

namespace Dinex.AppService;

public class SendEmailService : ISendEmailService
{
    private readonly ILogger<SendEmailService> _logger;
    private readonly AppSettings _appSettings;

    public SendEmailService(
        ILogger<SendEmailService> logger,
        IOptions<AppSettings> appSettings)
    {
        _logger = logger;
        _appSettings = appSettings.Value;
    }

    public async Task Execute(SendEmailModel sendEmailModel)
    {
        try
        {
            _logger.LogInformation($"Creating email to {sendEmailModel.DestinationEmailAddress}");
            var message = CreateMessage(sendEmailModel);

            _logger.LogInformation($"Sending email to {sendEmailModel.DestinationEmailAddress}");
            await SendEmailMessageAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error: {ex.Message}");
            throw;
        }
    }

    public string GetEmailMessage(EmailMessageModel emailMessageModel)
    {
        var partialTemplatePath = $"{_appSettings.MailTemplateFolder}/{emailMessageModel.EmailTemplateFileName}";

        _logger.LogInformation($"Getting path from {partialTemplatePath}");
        var templateFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, partialTemplatePath);

        var htmlToBody = string.Empty;
        using (StreamReader Source = File.OpenText(templateFolderPath))
        {
            htmlToBody = Source.ReadToEnd();
        }

        var activationUrl = $"{_appSettings.AllowedHost}/{emailMessageModel.Origin}/{emailMessageModel.GeneratedCode}";

        var formattedEmail = htmlToBody
            .Replace(emailMessageModel.TemplateFieldToName, emailMessageModel.DestinationName)
            .Replace(emailMessageModel.TemplateFieldToUrl, activationUrl); ;

        _logger.LogInformation($"Ending process for get email message");
        return formattedEmail;
    }

    private MailMessage CreateMessage(SendEmailModel sendEmailModel)
    {
        var fromDisplayName = _appSettings.MailboxName;
        var fromEmailAddress = _appSettings.MailboxAddress;

        var message = new MailMessage();

        message.From = new MailAddress(fromEmailAddress, fromDisplayName);
        message.To.Add(new MailAddress(sendEmailModel.DestinationEmailAddress, sendEmailModel.DestinationName));
        message.Subject = sendEmailModel.DestinationSubject;
        message.IsBodyHtml = sendEmailModel.IsHtml;
        message.Body = sendEmailModel.EmailMessage;

        return message;
    }

    private async Task SendEmailMessageAsync(MailMessage message)
    {
        var smtpHost = _appSettings.SmtpHost;
        var smtpPort = _appSettings.SmtpPort;
        var userName = _appSettings.MailboxAddress;
        var password = _appSettings.MailboxPassword;
        var enableSsl = _appSettings.SmtpUseSsl;

        var client = new SmtpClient
        {
            Host = smtpHost,
            Port = smtpPort,
            EnableSsl = enableSsl,
            Credentials = new NetworkCredential(userName, password)
        };
        await client.SendMailAsync(message);
        client.Dispose();
    }
}
