

namespace Dinex.AppService;

public class QueueService : IQueueService
{
    private readonly CultureInfo Culture = new("pt-BR");

    private readonly IQueueInRepository _queueInRepository;
    private readonly IInvestmentHistoryRepository _investmentHistoryRepository;

    private readonly IMediator _mediator;

    public QueueService(IQueueInRepository queueInRepository, IInvestmentHistoryRepository investmentHistoryRepository, IMediator mediator)
    {
        _queueInRepository = queueInRepository;
        _investmentHistoryRepository = investmentHistoryRepository;
        _mediator = mediator;
    }
    public async Task<OperationResult> CreateQueueIn(Guid userId, TransactionActivity queueType, IFormFile file)
    {
        var result = new OperationResult();

        try
        {
            var queueIn = QueueIn.Create(userId, queueType, file.FileName);

            var investmentHistoryData = GetInvestingHistoryData(file);
            if (investmentHistoryData.Count <= 0)
            {
                result.AddError("Não há dados de historico no arquivo enviado.");
                return result;
            }

            var investmentHistoryList = InvestmentHistory.CreateRange(investmentHistoryData, queueIn.Id, Culture);
            if (investmentHistoryList.Any(x => !x.IsValid))
            {
                var errors = investmentHistoryList.Where(x => !x.IsValid)
                        .SelectMany(x => x.Notifications).ToList();
                result.AddErrors(errors);
                return result;
            }

            await _queueInRepository.AddAsync(queueIn);
            await _investmentHistoryRepository.AddRangeAsync(investmentHistoryList);

            // enviar um evento para o comando de processamento
            var command = new ProcessQueueInCommand(userId);
            _ = _mediator.Send(command);

            return result;
        }
        catch (Exception ex)
        {
            return new OperationResult().SetAsInternalServerError()
                    .AddError($"An unexpected error ocurred to method: UploadInvestingStatement {ex}");
        }
    }

    #region private methods
    private static Dictionary<int, List<dynamic>> GetInvestingHistoryData(IFormFile fileHistory)
    {
        var dictionary = new Dictionary<int, List<dynamic>>();

        using (var stream = fileHistory?.OpenReadStream())
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var isFirstRow = true;
                var row = 0;
                while (reader.Read())
                {
                    if (isFirstRow)
                    {
                        isFirstRow = false;
                        continue;
                    }

                    var listOfColumns = new List<dynamic>();
                    for (int columnIndex = 0; columnIndex < reader.FieldCount; columnIndex++)
                    {
                        var columnType = reader.GetFieldType(columnIndex);
                        var columnValue = reader.GetValue(columnIndex);

                        if (columnType == typeof(string))
                        {
                            string stringValue = columnValue.ToString();
                            listOfColumns.Add(stringValue);
                        }
                        else if (columnType == typeof(int))
                        {
                            int intValue = Convert.ToInt32(columnValue);
                            listOfColumns.Add(intValue);
                        }
                        else if (columnType == typeof(double))
                        {
                            double doubleValue = Convert.ToDouble(columnValue);
                            listOfColumns.Add(doubleValue);
                        }
                        else if (columnType == typeof(float))
                        {
                            float floatValue = Convert.ToSingle(columnValue);
                            listOfColumns.Add(floatValue);
                        }
                        else if (columnType == typeof(DateTime))
                        {
                            var dateTime = Convert.ToDateTime(columnValue);
                            listOfColumns.Add(dateTime);
                        }
                    }
                    dictionary.Add(row, listOfColumns);
                    row++;
                }
                reader.Close();
            }
        }

        return dictionary;
    }
    #endregion
}
