namespace Dinex.AppService;

public interface IB3StatementParser
{
    List<B3StatementRow> Parse(Stream fileStream, Guid importJobId);
}

public class B3StatementParser : IB3StatementParser
{
    public List<B3StatementRow> Parse(Stream fileStream, Guid importJobId)
    {
        var rows = new List<B3StatementRow>();

        using var workbook = new XLWorkbook(fileStream);
        var worksheet = workbook.Worksheet(1);

        var firstDataRow = 2;
        var lastRow = worksheet.LastRowUsed().RowNumber();

        for (int rowIdx = firstDataRow; rowIdx <= lastRow; rowIdx++)
        {
            try
            {
                var row = worksheet.Row(rowIdx);

                string entradaSaida = row.Cell(1).GetValue<string>();          // "Crédito" / "Débito"
                DateTime date = ParseDate(row.Cell(2).GetValue<string>());
                string movimentacao = row.Cell(3).GetValue<string>();          // evento do ativo
                string asset = row.Cell(4).GetValue<string>();
                string broker = row.Cell(5).GetValue<string>();
                decimal? quantity = ParseNullableDecimal(row.Cell(6).GetValue<string>());
                decimal? unitPrice = ParseNullableDecimal(row.Cell(7).GetValue<string>());
                decimal? totalValue = ParseNullableDecimal(row.Cell(8).GetValue<string>());

                // OperationType vem de Entrada/Saída
                var operationType = FromEntradaSaida(entradaSaida);

                string? dueDate = null; // não existe na planilha

                string rawLineJson = JsonConvert.SerializeObject(
                    row.Cells(1, 8).Select(c => c.GetValue<string>()).ToList()
                );

                var statementRow = B3StatementRow.Create(
                    importJobId: importJobId,
                    rowNumber: rowIdx,
                    asset: asset,
                    operationType: operationType,
                    date: date,
                    dueDate: dueDate,
                    quantity: quantity,
                    unitPrice: unitPrice,
                    totalValue: totalValue,
                    broker: broker,
                    rawLineJson: rawLineJson,
                    movement: movimentacao      // <- guarda o evento original
                );

                rows.Add(statementRow);
            }
            catch (Exception ex)
            {
                string rawLineJson;
                try
                {
                    var row = worksheet.Row(rowIdx);
                    rawLineJson = JsonConvert.SerializeObject(
                        row.Cells(1, 8).Select(c => c.GetValue<string>()).ToList()
                    );
                }
                catch { rawLineJson = ""; }

                rows.Add(B3StatementRow.CreateError(
                    importJobId, rowIdx, rawLineJson,
                    $"Erro inesperado ao processar linha: {ex.Message}"));
            }
        }

        return rows;
    }

    private static OperationType FromEntradaSaida(string? value)
    {
        var v = Normalize(value);
        return v switch
        {
            "CREDITO" => OperationType.Buy,   // Entrada
            "DEBITO" => OperationType.Sell,  // Saída
            _ => throw new Exception($"Entrada/Saída inválido: \"{value}\" (esperado Crédito/Débito)")
        };
    }

    private static string Normalize(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;
        s = s.Trim().ToUpperInvariant();
        var normalized = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(capacity: normalized.Length);
        foreach (var ch in normalized)
        {
            var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC); // remove acento -> "CRÉDITO" vira "CREDITO"
    }

    private static decimal? ParseNullableDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        // trata formatos BR: "1.234,56" -> 1234.56
        var v = value.Replace(".", "").Replace(",", ".");
        return decimal.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
            ? result
            : null;
    }

    private static DateTime ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new Exception("Data vazia.");

        if (DateTime.TryParse(value, out var parsed))
            return DateTime.SpecifyKind(parsed, DateTimeKind.Utc);

        var formatos = new[] { "dd/MM/yyyy", "yyyy-MM-dd" };
        if (DateTime.TryParseExact(value, formatos, CultureInfo.InvariantCulture,
                                   DateTimeStyles.None, out parsed))
            return DateTime.SpecifyKind(parsed, DateTimeKind.Utc);

        throw new Exception($"Data inválida: \"{value}\".");
    }
}
