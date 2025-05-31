namespace Dinex.Core;

public class AuthenticationUserDTO
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Token { get; set; }

    public AuthenticationUserDTO(User user, string token)
    {
        Id = user.Id;
        FullName = user.FullName;
        Email = user.Email;
        Token = token;
    }
}
