namespace Dinex.Infra;

public class OperationConfiguration : IEntityTypeConfiguration<Operation>
{
    public void Configure(EntityTypeBuilder<Operation> builder)
    {
        builder.ToTable("Operations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Quantity)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(x => x.UnitPrice)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(x => x.TotalValue)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(x => x.ExecutedAt)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.Property(x => x.DeletedAt);

        builder.Property(x => x.WalletId)
            .IsRequired();

        builder.Property(x => x.AssetId)
            .IsRequired();

        builder.Property(x => x.BrokerId);
    }
}
