namespace Dinex.AppService;

public class EmailMessageModel
{
    public string GeneratedCode { get; set; }
    public string DestinationName { get; set; }
    public string EmailTemplateFileName { get; set; }
    public string Origin { get; set; }
    public string TemplateFieldToName { get; set; }
    public string TemplateFieldToUrl { get; set; }
}
