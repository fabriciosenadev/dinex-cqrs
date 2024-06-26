namespace Dinex.Api.Controllers;

[EnableCors("_myAllowSpecificOrigins")]
[ApiController]
public abstract class MainController : ControllerBase
{
    protected ActionResult HandleResult<T>(OperationResult<T> result)
    {
        if (result.HasErrors())
        {
            var errors = new { result.Errors };
            if (result.IsNotFound)
            {
                return NotFound(errors);
            }
            else if (!result.IsValid && !result.IsNotFound && !result.InternalServerError)
            {
                return BadRequest(errors);
            }
            else if (result.InternalServerError)
            {
                var internalError = result.Errors.ToList()[0];
                return Problem(internalError, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        var data = new { result.Data };
        return Ok(result.Data);
    }

    protected ActionResult HandleResult(OperationResult result)
    {
        if (result.HasErrors())
        {
            var errors = new { result.Errors };
            if (result.IsNotFound)
            {
                return NotFound(errors);
            }
            else if (!result.IsValid && !result.IsNotFound && !result.InternalServerError)
            {
                return BadRequest(errors);
            }
            else if (result.InternalServerError)
            {
                var internalError = result.Errors.ToList()[0];
                return Problem(internalError, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        return Ok();
    }

    protected static Guid GetUserId(HttpContext context)
    {
        if (context.Items["UserId"] is Guid userId)
        {
            return userId;
        }
        else
        {
            return Guid.Empty;
        }
    }

}
