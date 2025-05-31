namespace Dinex.Infra;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("Positions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.WalletId)
            .IsRequired();

        builder.Property(x => x.BrokerId)
            .IsRequired(false);

        builder.Property(x => x.AssetId)
            .IsRequired();

        builder.Property(x => x.CurrentQuantity)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(x => x.AveragePrice)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(x => x.InvestedValue)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt);
        builder.Property(x => x.DeletedAt);
    }
}
