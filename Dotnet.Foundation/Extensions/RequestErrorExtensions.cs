using Dotnet.Foundation.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Dotnet.Foundation.Extensions;

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

            var (status, title, type) = requestError.Type switch
            {
                RequestErrorType.RequestInvalid => (400, "Bad Request", "https://tools.ietf.org/html/rfc7231#section-6.5.1"),
                RequestErrorType.RequestNotAllowed => (403, "Forbidden", "https://tools.ietf.org/html/rfc7231#section-6.5.3"),
                RequestErrorType.ResourceNotFound => (404, "Not Found", "https://tools.ietf.org/html/rfc7231#section-6.5.4"),
                RequestErrorType.ResourceConflict => (409, "Conflict", "https://tools.ietf.org/html/rfc7231#section-6.5.8"),
                _ => throw new UnreachableException()
            };

            return Results.Problem(
                new ProblemDetails
                {
                    Status = status,
                    Title = title,
                    Type = type,
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
