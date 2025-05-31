using AppService;

namespace Dinex.Api.V1.Controllers;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
public class BrokersController : MainController
{
    private readonly IMediator _mediator;

    public BrokersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(BrokerDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetBrokerQuery(id));
        return HandleResult(result);
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<BrokerDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllBrokersQuery());
        return HandleResult(result);
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateBrokerCommand command)
    {
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBrokerCommand command)
    {
        if (id != command.Id)
            return BadRequest("URL ID does not match body ID.");

        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteBrokerCommand(id));
        return HandleResult(result);
    }

}
