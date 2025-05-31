namespace Dinex.Core;

public class Wallet : Entity
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string DefaultCurrency { get; private set; } = null!;

    protected Wallet() { }

    public static Wallet Create(Guid userId, string name, string defaultCurrency, string? description = null)
    {
        var wallet = new Wallet
        {
            UserId = userId,
            Name = name,
            DefaultCurrency = defaultCurrency,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var contract = new Contract<Wallet>()
            .Requires()
            .IsNotEmpty(userId, nameof(UserId), "Usuário é obrigatório.")
            .IsNotNullOrEmpty(name, nameof(Name), "Nome é obrigatório.")
            .IsGreaterOrEqualsThan(name.Length, 3, nameof(Name), "Nome deve ter no mínimo 3 caracteres.")
            .IsLowerOrEqualsThan(name.Length, 100, nameof(Name), "Nome deve ter no máximo 100 caracteres.")
            .IsNotNullOrEmpty(defaultCurrency, nameof(DefaultCurrency), "Moeda padrão é obrigatória.")
            .IsLowerOrEqualsThan(defaultCurrency.Length, 10, nameof(DefaultCurrency), "Moeda deve ter no máximo 10 caracteres.");

        if (!string.IsNullOrWhiteSpace(description))
        {
            contract.IsGreaterOrEqualsThan(description.Length, 3, nameof(Description), "Descrição deve ter no mínimo 3 caracteres.");
        }

        wallet.AddNotifications(contract);
        return wallet;
    }

    public void Update(string name, string defaultCurrency, string? description)
    {
        Name = name;
        DefaultCurrency = defaultCurrency;
        Description = description;
        UpdatedAt = DateTime.UtcNow;

        var contract = new Contract<Wallet>()
            .Requires()
            .IsNotNullOrEmpty(name, nameof(Name), "Nome é obrigatório.")
            .IsGreaterOrEqualsThan(name.Length, 3, nameof(Name), "Nome deve ter no mínimo 3 caracteres.")
            .IsLowerOrEqualsThan(name.Length, 100, nameof(Name), "Nome deve ter no máximo 100 caracteres.")
            .IsNotNullOrEmpty(defaultCurrency, nameof(DefaultCurrency), "Moeda padrão é obrigatória.")
            .IsLowerOrEqualsThan(defaultCurrency.Length, 10, nameof(DefaultCurrency), "Moeda deve ter no máximo 10 caracteres.");

        if (!string.IsNullOrWhiteSpace(description))
        {
            contract.IsGreaterOrEqualsThan(description.Length, 3, nameof(Description), "Descrição deve ter no mínimo 3 caracteres.");
        }

        AddNotifications(contract);
    }

}
