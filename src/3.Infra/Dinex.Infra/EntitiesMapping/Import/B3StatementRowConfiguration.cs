namespace Dinex.Infra;

public class B3StatementRowConfiguration : IEntityTypeConfiguration<B3StatementRow>
{
    public void Configure(EntityTypeBuilder<B3StatementRow> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.ImportJobId).IsRequired();
        builder.Property(r => r.RowNumber).IsRequired();

        // Asset pode ser nulo em linhas de erro
        builder.Property(r => r.Asset)
            .HasMaxLength(100)
            .IsRequired(false);

        // OperationType: enum nullable (Buy/Sell)
        builder.Property(r => r.OperationType)
            .HasConversion<string>() // mantém como string para leitura clara
            .HasMaxLength(20)
            .IsRequired(false);

        // LedgerSide: enum nullable (Credit/Debit)
        builder.Property(r => r.LedgerSide)
            .HasConversion<string>() // armazena como texto (legível)
            .HasMaxLength(20)
            .IsRequired(false);

        // StatementCategory: enum obrigatório (TradeBuy, CashIncomeDividend etc.)
        builder.Property(r => r.StatementCategory)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        // Movement (evento da planilha "Movimentação")
        builder.Property(r => r.Movement)
            .HasMaxLength(120)
            .IsRequired(false);

        builder.Property(r => r.Date).IsRequired();

        builder.Property(r => r.DueDate)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(r => r.Quantity)
            .HasPrecision(18, 6)
            .IsRequired(false);

        builder.Property(r => r.UnitPrice)
            .HasPrecision(18, 6)
            .IsRequired(false);

        builder.Property(r => r.TotalValue)
            .HasPrecision(18, 6)
            .IsRequired(false);

        builder.Property(r => r.Broker)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(r => r.RawLineJson)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.Error)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(x => x.ProcessedTrade)
            .HasColumnName("ProcessedTrade")
            .IsRequired(false);

        builder.Property(x => x.ProcessedTradeAtUtc)
            .HasColumnName("ProcessedTradeAtUtc")
            .IsRequired(false);


        // Índices
        builder.HasIndex(r => r.ImportJobId);
        builder.HasIndex(r => r.StatementCategory);
    }
}
