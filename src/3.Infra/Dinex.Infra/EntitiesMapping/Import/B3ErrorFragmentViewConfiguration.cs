namespace Dinex.Infra;

public sealed class B3ErrorFragmentViewConfiguration : IEntityTypeConfiguration<B3ErrorFragmentView>
{
    public void Configure(EntityTypeBuilder<B3ErrorFragmentView> eb)
    {
        eb.ToView("v_b3_error_fragments", "public");
        eb.HasNoKey();

        // evita qualquer tentativa de mapear coisas gerais que, porventura,
        // você adicione no BaseEntity no futuro
        // (no estado atual, BaseEntity está vazio, então não há o que ignorar)

        eb.Property(e => e.RowId).HasColumnName("RowId");
        eb.Property(e => e.ImportJobId).HasColumnName("ImportJobId");
        eb.Property(e => e.RowNumber).HasColumnName("RowNumber");
        eb.Property(e => e.Error).HasColumnName("Error");
        eb.Property(e => e.RawLineJson).HasColumnName("RawLineJson");
        eb.Property(e => e.ErrorIndex).HasColumnName("ErrorIndex");
        eb.Property(e => e.CreatedAt).HasColumnName("CreatedAt");
    }
}

