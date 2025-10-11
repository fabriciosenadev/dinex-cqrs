﻿namespace Dinex.AppService;

public sealed class DeleteImportJobCommandHandler
    : ICommandHandler, IRequestHandler<DeleteImportJobCommand, OperationResult<bool>>
{
    private readonly IImportJobRepository _jobs;
    private readonly IB3StatementRowRepository _rows;

    public DeleteImportJobCommandHandler(IImportJobRepository jobs, IB3StatementRowRepository rows)
    {
        _jobs = jobs;
        _rows = rows;
    }

    public async Task<OperationResult<bool>> Handle(DeleteImportJobCommand request, CancellationToken ct)
    {
        try
        {
            var job = await _jobs.GetByIdAsync(request.Id);
            if (job is null)
                return new OperationResult<bool>(false)
                    .AddError("ImportJob não encontrado.")
                    .SetAsNotFound();

            // regra: impedir remoção enquanto processando
            if (job.Status == ImportJobStatus.Processando)
                return new OperationResult<bool>(false)
                    .AddError("Não é possível excluir um job em processamento.");

            // 1) remove fisicamente as linhas associadas (defensivo caso FK não esteja em CASCADE)
            var rows = await _rows.GetByImportJobIdAsync(job.Id);
            foreach (var r in rows)
                await _rows.DeleteAsync(r); // seu Repository<T> já salva a cada operação

            // 2) remove fisicamente o job
            await _jobs.DeleteAsync(job); // seu Repository<T> já salva a cada operação

            return new OperationResult<bool>(true);
        }
        catch (Exception ex)
        {
            return new OperationResult<bool>(false)
                .AddError(ex.Message)
                .SetAsInternalServerError();
        }
    }
}
