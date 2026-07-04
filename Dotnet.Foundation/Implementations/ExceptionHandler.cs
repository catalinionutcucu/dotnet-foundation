using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dotnet.Foundation.Implementations;

public sealed class ExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ExceptionHandler> _logger;

    public ExceptionHandler(ILogger<ExceptionHandler> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken = default)
    {
        if (exception is NotImplementedException)
        {
            _logger.LogWarning(exception, "A not implemented exception occurred while processing the request.");

            httpContext.Response.StatusCode = 501;

            await httpContext.Response
                             .WriteAsJsonAsync(
                                 new ProblemDetails
                                 {
                                     Status = 501,
                                     Title = "Not Implemented",
                                     Type = "https://tools.ietf.org/html/rfc7231#section-6.6.2"
                                 },
                                 cancellationToken
                             )
                             .ConfigureAwait(false);
        }
        else
        {
            _logger.LogError(exception, "An unhandled exception occurred while processing the request.");

            httpContext.Response.StatusCode = 500;

            await httpContext.Response
                             .WriteAsJsonAsync(
                                 new ProblemDetails
                                 {
                                     Status = 500,
                                     Title = "Internal Server Error",
                                     Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                                 },
                                 cancellationToken
                             )
                             .ConfigureAwait(false);
        }

        return true;
    }
}
