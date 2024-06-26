namespace Dinex.AppService;

public class SendEmailModel
{
    public string DestinationEmailAddress { get; set; }
    public string DestinationName { get; set; }
    public string DestinationSubject { get; set; }
    public bool IsHtml { get; set; }
    public string EmailMessage { get; set; }
}
