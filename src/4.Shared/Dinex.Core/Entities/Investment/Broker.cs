namespace Dinex.Core;

public class Broker : Entity
{
    public string Name { get; private set; } = null!;
    public string? Cnpj { get; private set; }   // <── AGORA OPCIONAL
    public string? Website { get; private set; }

    protected Broker() { }

    public static Broker Create(string name, string? cnpj, string? website = null)
    {
        var broker = new Broker
        {
            Name = name,
            Cnpj = cnpj,
            Website = website,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var contract = new Contract<Broker>()
            .Requires()
            .IsNotNullOrEmpty(name, nameof(Name), "Nome é obrigatório.")
            .IsGreaterOrEqualsThan(name.Length, 3, nameof(Name), "Nome deve ter no mínimo 3 caracteres.")
            .IsLowerOrEqualsThan(name.Length, 100, nameof(Name), "Nome deve ter no máximo 100 caracteres.");

        // ❗ CNPJ só é validado se existir
        if (!string.IsNullOrWhiteSpace(cnpj))
        {
            contract
                .IsTrue(cnpj.Length == 14, nameof(Cnpj), "CNPJ deve conter 14 dígitos.")
                .Matches(cnpj, @"^\d{14}$", nameof(Cnpj), "CNPJ deve conter apenas números.");
        }

        if (!string.IsNullOrWhiteSpace(website))
        {
            contract.IsLowerOrEqualsThan(website.Length, 200, nameof(Website),
                "URL deve ter no máximo 200 caracteres.");
        }

        broker.AddNotifications(contract);
        return broker;
    }

    public void Update(string name, string? cnpj, string? website)
    {
        Name = name;
        Cnpj = cnpj;
        Website = website;
        UpdatedAt = DateTime.UtcNow;

        var contract = new Contract<Broker>()
            .Requires()
            .IsNotNullOrEmpty(name, nameof(Name), "Nome é obrigatório.")
            .IsGreaterOrEqualsThan(name.Length, 3, nameof(Name), "Nome deve ter no mínimo 3 caracteres.")
            .IsLowerOrEqualsThan(name.Length, 100, nameof(Name), "Nome deve ter no máximo 100 caracteres.");

        // ❗ Valida CNPJ apenas se fornecido
        if (!string.IsNullOrWhiteSpace(cnpj))
        {
            contract
                .IsTrue(cnpj.Length == 14, nameof(Cnpj), "CNPJ deve conter 14 dígitos.")
                .Matches(cnpj, @"^\d{14}$", nameof(Cnpj), "CNPJ deve conter apenas números.");
        }

        if (!string.IsNullOrWhiteSpace(website))
        {
            contract.IsLowerOrEqualsThan(website.Length, 200, nameof(Website),
                "URL deve ter no máximo 200 caracteres.");
        }

        AddNotifications(contract);
    }
}
