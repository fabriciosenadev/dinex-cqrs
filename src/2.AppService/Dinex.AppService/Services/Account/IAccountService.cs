namespace Dinex.AppService;

public interface IAccountService
{
    string GenerateToken(User user);
    string GenerateCode(int codeLength, CodeType generationOption = CodeType.Default);
}
