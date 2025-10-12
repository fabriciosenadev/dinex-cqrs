namespace Dinex.Core;

public class ImportJob : Entity
{
    public string FileName { get; private set; }
    public DateTime UploadedAt { get; private set; }
    public string UploadedBy { get; private set; }
    public ImportJobStatus Status { get; private set; }
    public string Error { get; private set; }
    public int? TotalRows { get; private set; }
    public int? ErrorRows { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public ICollection<B3StatementRow> StatementRows { get; private set; }
    public DateTime? PeriodStartUtc { get; private set; }
    public DateTime? PeriodEndUtc { get; private set; }

    public static ImportJob Create(
        string fileName,
        DateTime uploadedAt,
        string uploadedBy,
        ImportJobStatus status,
        string error = null,
        int? totalRows = null,
        int? errorRows = null,
        DateTime? processedAt = null,
        ICollection<B3StatementRow> statementRows = null)
    {
        var errors = new List<string>();

        // Nome do arquivo: obrigatório, deve ser .xlsx
        if (string.IsNullOrWhiteSpace(fileName))
            errors.Add("O campo 'Nome do arquivo' é obrigatório.");
        else if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            errors.Add("O arquivo deve estar no formato '.xlsx'.");

        // Data de upload: obrigatória e válida
        if (uploadedAt == default)
            errors.Add("O campo 'Data de upload' é obrigatório e deve ser uma data válida.");

        // Usuário responsável: obrigatório
        if (string.IsNullOrWhiteSpace(uploadedBy))
            errors.Add("O campo 'Usuário responsável pelo upload' é obrigatório.");

        // Status: obrigatório e deve ser um valor válido do enum
        if (!Enum.IsDefined(typeof(ImportJobStatus), status))
            errors.Add("O campo 'Status' contém um valor inválido.");

        // Se o status for Falha, a descrição do erro deve estar preenchida
        if (status == ImportJobStatus.Falha && string.IsNullOrWhiteSpace(error))
            errors.Add("O campo 'Erro' é obrigatório quando o status for 'Falha'.");

        // Nos demais casos, o campo de erro não deve estar preenchido
        if (status != ImportJobStatus.Falha && !string.IsNullOrWhiteSpace(error))
            errors.Add("O campo 'Erro' só deve ser preenchido se o status for 'Falha'.");

        // Total de linhas e linhas com erro: se informados, devem ser >= 0
        if (totalRows.HasValue && totalRows < 0)
            errors.Add("O campo 'Total de linhas' não pode ser negativo.");
        if (errorRows.HasValue && errorRows < 0)
            errors.Add("O campo 'Linhas com erro' não pode ser negativo.");

        // Consistência entre ErrorRows e Status
        if (errorRows.HasValue && errorRows > 0 && status == ImportJobStatus.Concluido && string.IsNullOrWhiteSpace(error))
            errors.Add("Se houver linhas com erro, o campo 'Erro' deve estar preenchido ou o status deve ser 'Falha'.");

        // Se houver qualquer erro de validação, lançar exceção com todas as mensagens
        if (errors.Any())
            throw new ArgumentException("Erros ao criar registro de importação: " + string.Join(" | ", errors));

        // Ajuste: nunca deixar o campo Error nulo no banco
        string errorToSave = status == ImportJobStatus.Falha
            ? error
            : string.Empty;

        var importJob = new ImportJob
        {
            Id = Guid.NewGuid(),
            FileName = fileName,
            UploadedAt = uploadedAt,
            UploadedBy = uploadedBy,
            Status = status,
            Error = errorToSave, // nunca null
            TotalRows = totalRows,
            ErrorRows = errorRows,
            ProcessedAt = processedAt,
            StatementRows = statementRows ?? new List<B3StatementRow>(),
            CreatedAt = DateTime.UtcNow
        };

        return importJob;
    }

    public void SetStatus(ImportJobStatus status, string error = null)
    {
        Status = status;
        Error = status == ImportJobStatus.Falha
            ? (error ?? "Erro desconhecido ao processar a importação.")
            : string.Empty;
    }

    public void MarkAsConcluido(int totalRows, int errorRows)
    {
        Status = ImportJobStatus.Concluido;
        Error = string.Empty;
        TotalRows = totalRows;
        ErrorRows = errorRows;
        ProcessedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFalha(string error, int totalRows, int errorRows)
    {
        Status = ImportJobStatus.Falha;
        Error = error ?? "Erro desconhecido ao processar a importação.";
        TotalRows = totalRows;
        ErrorRows = errorRows;
        ProcessedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFalha(Exception ex)
    {
        Status = ImportJobStatus.Falha;
        Error = ex.Message ?? "Erro desconhecido ao processar a importação.";
        ProcessedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPeriodFromRows(IEnumerable<B3StatementRow> rows)
    {
        if (rows is null) { PeriodStartUtc = null; PeriodEndUtc = null; return; }

        var dates = rows
            .Select(r => r.Date)
            .Where(d => d != default) // só datas válidas
            .ToArray();

        if (dates.Length == 0)
        {
            PeriodStartUtc = null;
            PeriodEndUtc = null;
            return;
        }

        var min = dates.Min();
        var max = dates.Max();

        // Garantia de ordem
        if (min > max) (min, max) = (max, min);

        PeriodStartUtc = min;
        PeriodEndUtc = max;
        UpdatedAt = DateTime.UtcNow;
    }
}
