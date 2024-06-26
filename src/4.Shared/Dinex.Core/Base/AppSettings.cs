namespace Dinex.Core;

public class AppSettings
{
    public string Secret { get; set; }

    // -- SendMail settings
    public string SmtpHost { get; set; }
    public int SmtpPort { get; set; }
    public bool SmtpUseSsl { get; set; }
    public string MailboxAddress { get; set; }
    public string MailboxPassword { get; set; }
    public string MailboxName { get; set; }
    public string MailTemplateFolder { get; set; }
    public string AllowedHost { get; set; }
}
