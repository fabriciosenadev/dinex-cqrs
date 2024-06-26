namespace Dinex.Core;

public class User : Entity
{
    public string FullName { get; private set; }
    public string Email { get; private set; }
    public string Password { get; private set; }
    public UserStatus UserStatus { get; private set; }
    public string? ActivationCode { get; private set; }

    private User(
        string fullName,
        string email,
        string password,
        UserStatus userStatus,
        string? activationCode,
        DateTime createdAt,
        DateTime? updatedAt,
        DateTime? deletedAt)
    {
        FullName = fullName;
        Email = email;
        Password = password;
        UserStatus = userStatus;
        ActivationCode = activationCode;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        DeletedAt = deletedAt;
    }

    public static User CreateUser(string fullName, string email, string password, string confirmPassword)
    {
        var newUser = new User(
                fullName: fullName,
                email: email,
                password: string.Empty,
                userStatus: UserStatus.Inactive,
                activationCode: null,
                createdAt: DateTime.UtcNow,
                updatedAt: null,
                deletedAt: null
            );

        newUser.AddNotifications(
            new Contract<Notification>()
            .Requires()
            .IsNotNullOrEmpty(newUser.FullName, "User.FullName", "Informe seu nome completo")
            .IsGreaterOrEqualsThan(newUser.FullName?.Length ?? 0, 3, "User.FullName", "Informe seu nome completo")
            .IsNotNullOrEmpty(newUser.Email, "User.Email", "Informe seu melhor e-mail")
            .IsEmail(newUser.Email, "User.Email", "Informe seu melhor e-mail")
            );

        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            newUser.AddNotification("User.Password", "Preencha a senha com no minimo 8 caracteres");
        }

        if (password != confirmPassword)
        {
            newUser.AddNotification("User.ConfirmPassword", "Senhas devem ser iguais");
        }

        newUser.Password = Encrypt(password);

        return newUser;
    }

    public static string Encrypt(string value)
    {
        var result = BCrypt.Net.BCrypt.HashPassword(value);
        return result;
    }

    public static bool MatchValues(string encryptedValue, string valueToCompare)
    {
        var result = BCrypt.Net.BCrypt.Verify(valueToCompare, encryptedValue);
        return result;
    }

    public bool HasValidActivationCode()
    {
        if (UpdatedAt is null)
            return false;

        var timeToValidate = UpdatedAt.Value.AddHours(2);
        if (timeToValidate <= DateTime.UtcNow)
            return false;

        return true;
    }

    public void AssingActivationCode(string activationCode)
    {
        ActivationCode = activationCode;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        ActivationCode = null;
        UpdatedAt = DateTime.UtcNow;
        UserStatus = UserStatus.Active;
    }
}
