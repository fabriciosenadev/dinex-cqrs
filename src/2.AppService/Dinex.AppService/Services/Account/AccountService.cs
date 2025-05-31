namespace Dinex.AppService;

public class AccountService : IAccountService
{
    private readonly AppSettings _appSettings;
    private readonly IJwtGenerator _jwtGenerator;
    public AccountService(IOptions<AppSettings> appSettings, IJwtGenerator jwtGenerator)
    {
        _appSettings = appSettings.Value;
        _jwtGenerator = jwtGenerator;
    }
    public string GenerateToken(User user)
    {
        List<KeyValuePair<string, string>> claims = [
            new KeyValuePair<string, string>("id", user.Id.ToString())
        ];

        var token = _jwtGenerator.WithSecret(_appSettings.Secret)
            .WithSigningAlgorithm(SecurityAlgorithms.HmacSha256Signature)
            .WithClaims(claims)
            .GenerateToken();

        //var token = _jwtGenerator
        //    .WithSecret(_appSettings.Secret)
        //    .WithSigningAlgorithm(SecurityAlgorithms.HmacSha256Signature)
        //    .GenerateToken();

        //_jwtGenerator.IsTokenExpired(token);

        return token;
    }

    public string GenerateCode(int codeLength, CodeType generationOption = CodeType.Default)
    {
        var random = new Random();
        var chars = string.Empty;

        const string lower = "abcdefghijklmnopqrstuvwxyz";
        const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string numbers = "0123456789";

        switch (generationOption)
        {
            case CodeType.JustLower:
                chars = lower;
                break;
            case CodeType.JustUpper:
                chars = upper;
                break;
            case CodeType.JustNumbers:
                chars = numbers;
                break;
            case CodeType.LowerAndUpper:
                chars = lower + upper;
                break;
            case CodeType.LowerAndNumbers:
                chars = lower + numbers;
                break;
            case CodeType.UpperAndNumbers:
                chars = upper + numbers;
                break;
            default:
                chars = lower + upper + numbers;
                break;
        }

        return new string(Enumerable.Repeat(chars, codeLength)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
