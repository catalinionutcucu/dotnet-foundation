using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dotnet.Foundation.Implementations;

/// <summary>
/// Represents the exception handler implementing <see cref = "IExceptionHandler" />, mapping unhandled exceptions to HTTP responses.
/// </summary>
public sealed class ExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ExceptionHandler> _logger;

    private readonly IProblemDetailsService _problemDetailsService;

    public ExceptionHandler(ILogger<ExceptionHandler> logger, IProblemDetailsService problemDetailsService)
    {
        _logger = logger;
        _problemDetailsService = problemDetailsService;
    }

    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken = default)
    {
        if (exception is NotImplementedException)
        {
            _logger.LogWarning(exception, "A not implemented exception occurred while processing the request.");

            httpContext.Response.StatusCode = StatusCodes.Status501NotImplemented;
        }
        else
        {
            _logger.LogError(exception, "An unhandled exception occurred while processing the request.");

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
                                           {
                                               HttpContext = httpContext,
                                               ProblemDetails = new ProblemDetails
                                               {
                                                   Status = httpContext.Response.StatusCode
                                               },
                                               Exception = exception
                                           })
                                           .ConfigureAwait(false);
    }
}
