namespace Dinex.AppService;

public class UploadB3StatementCommand : IRequest<OperationResult<Guid>>
{
    public IFormFile File { get; set; }
    public Guid UserId { get; set; }
}

