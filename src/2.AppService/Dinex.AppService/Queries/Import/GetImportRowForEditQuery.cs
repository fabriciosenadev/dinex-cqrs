// Dinex.AppService/Import/GetImportRowForEditQuery.cs
namespace Dinex.AppService;

public sealed class GetImportRowForEditQuery : IRequest<OperationResult<ImportRowForEditDTO>>, IQueryHandler
{
    public Guid ImportJobId { get; set; }
    public Guid Id { get; set; } // RowId
}
