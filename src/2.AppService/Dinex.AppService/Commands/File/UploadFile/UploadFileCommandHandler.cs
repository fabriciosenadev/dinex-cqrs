
namespace Dinex.AppService;

public class UploadFileCommandHandler : ICommandHandler, IRequestHandler<UploadFileCommand, OperationResult>
{
    private readonly string[] excelMimeTypes =
    [
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", // .xlsx
        "application/vnd.ms-excel", // .xls
        "application/vnd.ms-office", // .xls (antiga versão)
        "application/vnd.ms-excel.sheet.macroEnabled.12", // .xlsm (com macros)
        // Adicione outros tipos, se necessário...
    ];

    private readonly IQueueService _queueService;

    public UploadFileCommandHandler(IQueueService queueService)
    {
        _queueService = queueService;
    }

    public async Task<OperationResult> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        var result = new OperationResult();

        if (request.FileHistory == null || request.FileHistory.Length == 0)
        {
            result.AddError("Arquivo é obrigatório");
        }
        else if (!excelMimeTypes.Contains(request.FileHistory.ContentType))
        {
            result.AddError("Formato de arquivo inválido");
        }

        var queueResult = await _queueService.CreateQueueIn(request.UserId, request.QueueType, request.FileHistory);
        if (queueResult.HasErrors())
        {
            result.AddErrors(queueResult.Errors);
        }

        return result;
    }
}
