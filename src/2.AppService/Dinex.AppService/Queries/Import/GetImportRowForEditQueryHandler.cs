// Dinex.AppService/Import/GetImportRowForEditQueryHandler.cs
namespace Dinex.AppService
{
    public sealed class GetImportRowForEditQueryHandler
        : IRequestHandler<GetImportRowForEditQuery, OperationResult<ImportRowForEditDTO>>, IQueryHandler
    {
        private readonly IB3StatementRowRepository _repo;

        public GetImportRowForEditQueryHandler(IB3StatementRowRepository repo) => _repo = repo;

        public async Task<OperationResult<ImportRowForEditDTO>> Handle(GetImportRowForEditQuery request, CancellationToken ct)
        {
            var result = new OperationResult<ImportRowForEditDTO>();

            var row = await _repo.GetByIdAsync(request.Id);
            if (row is null || row.ImportJobId != request.ImportJobId)
            {
                result.SetAsNotFound()
                      .AddError("Row not found for this job.");
                return result;
            }

            result.SetData(new ImportRowForEditDTO
            {
                Id = row.Id,
                ImportJobId = row.ImportJobId,
                RowNumber = row.RowNumber,

                Asset = row.Asset,
                OperationType = row.OperationType?.ToString(),
                Movement = row.Movement,

                Date = row.Date,
                DueDate = row.DueDate,
                Quantity = row.Quantity,
                UnitPrice = row.UnitPrice,
                TotalValue = row.TotalValue,

                Broker = row.Broker,
                RawLineJson = row.RawLineJson,
                Status = row.Status.ToString(),
                Error = row.Error,

                CreatedAt = row.CreatedAt,
                UpdatedAt = row.UpdatedAt
            });

            return result;
        }
    }
}
