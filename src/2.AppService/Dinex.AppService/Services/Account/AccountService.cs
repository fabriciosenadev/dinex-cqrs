using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;

namespace Dinex.AppService;

public class AccountService : IAccountService
{
    private readonly AppSettings _appSettings;
    public AccountService(IOptions<AppSettings> appSettings)
    {
        _appSettings = appSettings.Value;
    }
    public string GenerateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
        var tokenDescription = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
            Expires = DateTime.UtcNow.AddDays(7), // expires in 7 days
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescription);
        return tokenHandler.WriteToken(token);
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
