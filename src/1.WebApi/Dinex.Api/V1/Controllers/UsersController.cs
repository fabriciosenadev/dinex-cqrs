﻿namespace Dinex.Api.V1.Controllers;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
public class UsersController : MainController
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Anonymous endpoints
    [HttpPost]
    public async Task<ActionResult> Create(CreateUserCommand command)
    {
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("activate")]
    public async Task<ActionResult> Activate(ActivateUserCommand command)
    {
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginUserCommand command)
    {
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }
    #endregion


    #region Authenticated endpoints
    [HttpGet]
    [Authorize]
    public async Task<ActionResult> Get([FromQuery] GetUserQuery request)
    {
        var result = await _mediator.Send(request);
        return HandleResult(result);
    }
    #endregion
}
