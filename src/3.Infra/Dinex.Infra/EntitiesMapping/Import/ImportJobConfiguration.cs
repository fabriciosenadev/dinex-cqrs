namespace Dinex.Infra;

public class ImportJobConfiguration : IEntityTypeConfiguration<ImportJob>
{
    public void Configure(EntityTypeBuilder<ImportJob> builder)
    {
        builder.HasKey(j => j.Id);

        builder.Property(j => j.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(j => j.UploadedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(j => j.UploadedAt)
            .IsRequired();

        builder.Property(j => j.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(j => j.Error)
            .HasMaxLength(500)
            .HasDefaultValue(string.Empty)
            .IsRequired(); // garante não nulo no banco

        builder.Property(j => j.TotalRows)
            .IsRequired(false);

        builder.Property(j => j.ErrorRows)
            .IsRequired(false);

        builder.Property(j => j.ProcessedAt)
            .IsRequired(false);

        builder.Property(j => j.PeriodStartUtc)
            .IsRequired(false);

        builder.Property(j => j.PeriodEndUtc)
            .IsRequired(false);

        builder.Property(j => j.CreatedAt)
            .IsRequired();

        builder.Property(j => j.UpdatedAt)
            .IsRequired(false);

        // Relacionamento ImportJob (1) -> B3StatementRows (N)
        builder.HasMany(j => j.StatementRows)
            .WithOne()
            .HasForeignKey(r => r.ImportJobId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices úteis
        builder.HasIndex(j => j.UploadedAt);
        builder.HasIndex(j => j.Status);
    }
}
