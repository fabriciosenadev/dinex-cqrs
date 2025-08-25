namespace Dinex.Infra;

public class ImportJobConfiguration : IEntityTypeConfiguration<ImportJob>
{
    public void Configure(EntityTypeBuilder<ImportJob> builder)
    {
        builder.HasKey(j => j.Id);

        builder.Property(j => j.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(j => j.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(j => j.UploadedBy)
            .HasMaxLength(100);

        builder.Property(j => j.Error)
            .HasMaxLength(500);

        builder.HasMany(j => j.StatementRows)
            .WithOne()
            .HasForeignKey(r => r.ImportJobId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
