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

        // FileName: obrigatório, só aceita .xlsx
        if (string.IsNullOrWhiteSpace(fileName))
            errors.Add("Nome do arquivo obrigatório.");
        else if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            errors.Add("Arquivo deve ser .xlsx.");

        // UploadedAt: obrigatório e válido
        if (uploadedAt == default)
            errors.Add("Data de upload obrigatória e não informada.");

        // UploadedBy: obrigatório
        if (string.IsNullOrWhiteSpace(uploadedBy))
            errors.Add("Usuário responsável pelo upload não informado.");

        // Status: obrigatório, dentro dos valores do enum
        if (!Enum.IsDefined(typeof(ImportJobStatus), status))
            errors.Add("Status de importação inválido.");

        // Se for Falha, Error é obrigatório e deve ter texto relevante
        if (status == ImportJobStatus.Falha && string.IsNullOrWhiteSpace(error))
            errors.Add("Descrição do erro obrigatória quando status for 'Falha'.");
        // Nos outros casos, Error não deveria ser preenchido
        if (status != ImportJobStatus.Falha && !string.IsNullOrWhiteSpace(error))
            errors.Add("Erro só deve ser informado se o status for 'Falha'.");

        // TotalRows/ErrorRows: se vier, deve ser >= 0
        if (totalRows.HasValue && totalRows < 0)
            errors.Add("TotalRows não pode ser negativo.");
        if (errorRows.HasValue && errorRows < 0)
            errors.Add("ErrorRows não pode ser negativo.");

        // Consistência entre ErrorRows e Status: se tiver erroRows > 0, status deveria ser Falha ou Concluido com ressalvas (defina sua regra)
        if (errorRows.HasValue && errorRows > 0 && status == ImportJobStatus.Concluido && string.IsNullOrWhiteSpace(error))
            errors.Add("Se houver linhas com erro, o campo Error deve ser preenchido ou o status deve ser 'Falha'.");

        if (errors.Any())
            throw new ArgumentException("Erros ao criar ImportJob: " + string.Join(" | ", errors));

        // Aqui está o ajuste: nunca deixar Error = null no banco!
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
            Error = errorToSave, // Nunca null!
            TotalRows = totalRows,
            ErrorRows = errorRows,
            ProcessedAt = processedAt,
            StatementRows = statementRows ?? new List<B3StatementRow>()
        };

        return importJob;
    }

    public void SetStatus(ImportJobStatus status, string error = null)
    {
        Status = status;
        Error = status == ImportJobStatus.Falha
            ? (error ?? "Erro desconhecido ao importar.")
            : string.Empty;
    }

    public void MarkAsConcluido(int totalRows, int errorRows)
    {
        Status = ImportJobStatus.Concluido;
        Error = string.Empty;
        TotalRows = totalRows;
        ErrorRows = errorRows;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkAsFalha(string error, int totalRows, int errorRows)
    {
        Status = ImportJobStatus.Falha;
        Error = error ?? "Erro desconhecido ao importar.";
        TotalRows = totalRows;
        ErrorRows = errorRows;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkAsFalha(Exception ex)
    {
        Status = ImportJobStatus.Falha;
        Error = ex.Message ?? "Erro desconhecido ao importar.";
        ProcessedAt = DateTime.UtcNow;
    }
}
