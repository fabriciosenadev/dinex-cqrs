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
        [ProducesResponseType(typeof(OperationResult<PagedResult<ImportErrorDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetErrors([FromRoute] Guid id, [FromQuery] GetImportErrorsQuery query)
        {
            query.ImportJobId = id;
            var result = await _mediator.Send(query);
            return HandleResult(result);
        }
    }
}
