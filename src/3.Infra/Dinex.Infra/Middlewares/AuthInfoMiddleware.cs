using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace Dinex.Infra;

public class AuthInfoMiddleware
{
    private readonly RequestDelegate _next;
    private readonly AppSettings _appSettings;

    public AuthInfoMiddleware(RequestDelegate next, IOptions<AppSettings> appSettings)
    {
        _next = next;
        _appSettings = appSettings.Value;
    }

    public async Task Invoke(HttpContext context)
    {
        var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;

        if (endpoint != null)
        {
            bool requiresAuthentication = endpoint.Metadata.GetMetadata<AuthorizeAttribute>() != null;

            if (requiresAuthentication)
            {
                var userId = GetUserId(context);
                if (userId != Guid.Empty)
                {
                    context.Items["UserId"] = userId;
                }
            }
        }

        await _next(context);
    }

    private Guid GetUserId(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (token == null)
            return Guid.Empty;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = Guid.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);

            return userId;
        }
        catch
        {
            return Guid.Empty;
        }
    }

}
