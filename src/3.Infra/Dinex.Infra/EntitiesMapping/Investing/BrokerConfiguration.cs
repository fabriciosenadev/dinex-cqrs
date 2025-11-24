namespace Dinex.Infra;

public class BrokerConfiguration : IEntityTypeConfiguration<Broker>
{
    public void Configure(EntityTypeBuilder<Broker> builder)
    {
        builder.ToTable("Brokers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Cnpj)
            .HasMaxLength(14)
            .IsUnicode(false)
            .IsRequired(false);

        builder.Property(x => x.Website)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt);
        builder.Property(x => x.DeletedAt);
    }
}
