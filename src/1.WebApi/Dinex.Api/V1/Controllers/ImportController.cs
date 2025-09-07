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
        public async Task<IActionResult> GetJobs([FromQuery] string? status, [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var result = await _mediator.Send(new GetImportJobsQuery(status, page, pageSize));
            return HandleResult(result);
        }
    }
}
