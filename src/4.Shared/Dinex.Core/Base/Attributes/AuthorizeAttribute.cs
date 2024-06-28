namespace Dinex.Core;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public async void OnAuthorization(AuthorizationFilterContext context)
    {
        var result = new OperationResult();
        result.AddError("Voce precisa estar logado para usar este recurso");

        try
        {
            var user = await (Task<User>)context.HttpContext.Items["User"];
            if (user is null)
            {
                
                context.Result = new JsonResult(new { result.Errors }) { StatusCode = StatusCodes.Status401Unauthorized };
            }
        }
        catch (Exception e)
        {
            context.Result = new JsonResult(new { result.Errors }) { StatusCode = StatusCodes.Status401Unauthorized };
        }
    }
}
