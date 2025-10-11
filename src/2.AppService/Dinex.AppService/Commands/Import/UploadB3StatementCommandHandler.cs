namespace Dinex.AppService;

public class UploadB3StatementCommandHandler : ICommandHandler, IRequestHandler<UploadB3StatementCommand, OperationResult<Guid>>
{
    private readonly IImportJobRepository _importJobRepository;
    private readonly IB3StatementRowRepository _b3StatementRowRepository;
    private readonly IB3StatementParser _parser; // se quiser injetar parser customizado

    public UploadB3StatementCommandHandler(
        IImportJobRepository importJobRepository,
        IB3StatementRowRepository b3StatementRowRepository,
        IB3StatementParser parser
        )
    {
        _importJobRepository = importJobRepository;
        _b3StatementRowRepository = b3StatementRowRepository;
        _parser = parser;
    }

    public async Task<OperationResult<Guid>> Handle(UploadB3StatementCommand request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Guid>();

        if (request.File == null || request.File.Length == 0)
            return result.AddError("Arquivo inválido.");

        ImportJob importJob;
        try
        {
            importJob = ImportJob.Create(
                fileName: request.File.FileName,
                uploadedAt: DateTime.UtcNow,
                uploadedBy: request.UserId,
                status: ImportJobStatus.Pendente
            );
        }
        catch (ArgumentException ex)
        {
            return result.AddError(ex.Message);
        }

        await _importJobRepository.AddAsync(importJob);

        List<B3StatementRow> statementRows = new();
        int totalRows = 0, errorRows = 0;

        try
        {
            using (var stream = request.File.OpenReadStream())
            {
                var linhas = _parser.Parse(stream, importJob.Id);
                statementRows.AddRange(linhas);

                totalRows = statementRows.Count;
                errorRows = statementRows.Count(x => x.Status == B3StatementRowStatus.Erro);
            }

            if (statementRows.Any())
                await _b3StatementRowRepository.AddRangeAsync(statementRows);

            // Aqui, use métodos da entidade!
            if (errorRows > 0)
            {
                importJob.MarkAsFalha(
                    $"O arquivo foi processado com {errorRows} linha(s) inválida(s).",
                    totalRows,
                    errorRows
                );
            }
            else
            {
                importJob.MarkAsConcluido(totalRows, errorRows);
            }

            await _importJobRepository.UpdateAsync(importJob);

            return result.SetData(importJob.Id);
        }
        catch (Exception ex)
        {
            importJob.MarkAsFalha(ex);
            await _importJobRepository.UpdateAsync(importJob);

            return result.AddError("Falha ao processar arquivo: " + ex.Message)
                         .SetAsInternalServerError();
        }
    }
}

