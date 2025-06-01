using Jude.Server.Core.Helpers;
using Microsoft.AspNetCore.Diagnostics;

namespace Jude.Server.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        await httpContext.Response.WriteAsJsonAsync(
            Result.Ok(exception.Message),
            cancellationToken: cancellationToken
        );

        return true;
    }
}
