namespace Dinex.Core;

public class Asset : Entity
{
    public string Name { get; private set; } = null!;
    public string Code { get; private set; } = null!;
    public string? Cnpj { get; private set; }
    public Exchange Exchange { get; private set; }
    public Currency Currency { get; private set; }
    public AssetType Type { get; private set; }
    public string? Sector { get; private set; }

    protected Asset() { }

    public static Asset Create(
        string name,
        string code,
        string? cnpj,
        Exchange exchange,
        Currency currency,
        AssetType type,
        string? sector = null)
    {
        var asset = new Asset
        {
            Name = name,
            Code = code,
            Cnpj = cnpj,
            Exchange = exchange,
            Currency = currency,
            Type = type,
            Sector = sector,
            CreatedAt = DateTime.UtcNow
        };

        var contract = new Contract<Asset>()
            .Requires()
            .IsNotNullOrEmpty(name, nameof(Name), "Nome é obrigatório.")
            .IsGreaterOrEqualsThan(name.Length, 3, nameof(Name), "Nome deve ter no mínimo 3 caracteres.")
            .IsLowerOrEqualsThan(name.Length, 100, nameof(Name), "Nome deve ter no máximo 100 caracteres.")

            .IsNotNullOrEmpty(code, nameof(Code), "Código é obrigatório.")
            .IsGreaterOrEqualsThan(code.Length, 4, nameof(Code), "Código deve ter no mínimo 4 caracteres.")
            .IsLowerOrEqualsThan(code.Length, 10, nameof(Code), "Código deve ter no máximo 10 caracteres.");

        if (!string.IsNullOrWhiteSpace(cnpj))
        {
            contract
                .IsTrue(cnpj.Length == 14, nameof(Cnpj), "CNPJ deve conter 14 dígitos.")
                .Matches(cnpj, @"^\d{14}$", nameof(Cnpj), "CNPJ deve conter apenas números.");
        }

        if (!string.IsNullOrWhiteSpace(sector))
        {
            contract
                .IsGreaterOrEqualsThan(sector.Length, 3, nameof(Sector), "Setor deve ter no mínimo 3 caracteres.");
        }

        asset.AddNotifications(contract);
        return asset;
    }

    public void Update(
        string name,
        string code,
        string? cnpj,
        Exchange exchange,
        Currency currency,
        AssetType type,
        string? sector)
    {
        Name = name;
        Code = code;
        Cnpj = cnpj;
        Exchange = exchange;
        Currency = currency;
        Type = type;
        Sector = sector;
        UpdatedAt = DateTime.UtcNow;

        var contract = new Contract<Asset>()
            .Requires()
            .IsNotNullOrEmpty(name, nameof(Name), "Nome é obrigatório.")
            .IsGreaterOrEqualsThan(name.Length, 3, nameof(Name), "Nome deve ter no mínimo 3 caracteres.")
            .IsLowerOrEqualsThan(name.Length, 100, nameof(Name), "Nome deve ter no máximo 100 caracteres.")

            .IsNotNullOrEmpty(code, nameof(Code), "Código é obrigatório.")
            .IsGreaterOrEqualsThan(code.Length, 4, nameof(Code), "Código deve ter no mínimo 4 caracteres.")
            .IsLowerOrEqualsThan(code.Length, 10, nameof(Code), "Código deve ter no máximo 10 caracteres.");

        if (!string.IsNullOrWhiteSpace(cnpj))
        {
            contract
                .IsTrue(cnpj.Length == 14, nameof(Cnpj), "CNPJ deve conter 14 dígitos.")
                .Matches(cnpj, @"^\d{14}$", nameof(Cnpj), "CNPJ deve conter apenas números.");
        }

        if (!string.IsNullOrWhiteSpace(sector))
        {
            contract
                .IsGreaterOrEqualsThan(sector.Length, 3, nameof(Sector), "Setor deve ter no mínimo 3 caracteres.");
        }

        AddNotifications(contract);
    }
}
