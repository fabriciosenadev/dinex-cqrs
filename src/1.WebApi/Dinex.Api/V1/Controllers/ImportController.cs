namespace Dinex.Api.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    public class ImportController : MainController
    {
        private readonly IMediator _mediator;

        public ImportController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("b3/upload")]
        [Authorize]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadB3Statement([FromForm] UploadB3StatementCommand command)
        {
            var userId = GetUserId(HttpContext);
            if (userId == Guid.Empty)
                return Unauthorized("Invalid or missing user ID.");
            command.UserId = userId;
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>Lista as importações realizadas</summary>
        [HttpGet("jobs")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<ImportJobListItemDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetJobs([FromQuery] GetImportJobsQuery query)
        {
            var result = await _mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>List error rows for an ImportJob.</summary>
        [HttpGet("{id:guid}/errors")]
        [Authorize]
        [ProducesResponseType(typeof(OperationResult<PagedResult<ImportErrorDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetErrors([FromRoute] Guid id, [FromQuery] GetImportErrorsQuery query)
        {
            query.ImportJobId = id;
            var result = await _mediator.Send(query);
            return HandleResult(result);
        }

        [HttpGet("{id:guid}/rows/{rowId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ImportRowForEditDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRowForEdit([FromRoute] Guid id, [FromRoute] Guid rowId)
        {
            var query = new GetImportRowForEditQuery { ImportJobId = id, Id = rowId };
            var result = await _mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>Exclui fisicamente um ImportJob e suas linhas associadas.</summary>
        [HttpDelete("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteJob([FromRoute] Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new DeleteImportJobCommand(id), ct);
            return HandleResult(result); // retorna 200 OK com true em caso de sucesso
        }

        [HttpPost("{id:guid}/process")]
        [ProducesResponseType(typeof(OperationResult<ProcessReport>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> Process(Guid id, [FromBody] ProcessImportJobRequest req, CancellationToken ct)
        {
            var cmd = new ProcessImportJobCommand(id, req.WalletId, req.BrokerMode, req.BrokerId);
            var result = await _mediator.Send(cmd, ct);
            return HandleResult(result);
        }
    }
}
