using Dotnet.Foundation.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Dotnet.Foundation.Extensions;

/// <summary>
/// Provides extension members for mapping <see cref = "RequestError" /> instances to HTTP responses.
/// </summary>
public static class RequestErrorExtensions
{
    extension(RequestError requestError)
    {
        /// <summary>
        /// Maps a <see cref = "RequestError" /> instance to an HTTP response of type <see cref = "IResult" /> based on its <see cref = "RequestErrorType" />.
        /// </summary>
        /// <returns>An HTTP response of type <see cref = "IResult" />.</returns>
        public IResult ToHttpResponse()
        {
            ArgumentNullException.ThrowIfNull(requestError);

            var status = requestError.Type switch
            {
                RequestErrorType.RequestInvalid => StatusCodes.Status400BadRequest,
                RequestErrorType.RequestNotAllowed => StatusCodes.Status403Forbidden,
                RequestErrorType.ResourceNotFound => StatusCodes.Status404NotFound,
                RequestErrorType.ResourceConflict => StatusCodes.Status409Conflict,
                _ => throw new UnreachableException()
            };

            return Results.Problem(
                new ProblemDetails
                {
                    Status = status,
                    Extensions = new Dictionary<string, object?>
                    {
                        {
                            "error", new Dictionary<string, object?>
                            {
                                { "code", requestError.Code },
                                { "issues", requestError.Issues }
                            }
                        }
                    }
                });
        }
    }
}
