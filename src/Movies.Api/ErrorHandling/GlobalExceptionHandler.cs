using System.Data.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace Movies.Api.ErrorHandling;

/// <summary>
/// The last line of defence: logs any unhandled exception and returns a standard ProblemDetails
/// response without internal details. Invalid input never gets this far: [ApiController] already
/// answers it with a 400 that lists what was wrong.
/// </summary>
internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetails,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private const int ClientClosedRequest = 499;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            // The caller went away (e.g. typed another letter in the search box); nothing went wrong.
            httpContext.Response.StatusCode = ClientClosedRequest;
            return true;
        }

        var databaseDown = IsDatabaseError(exception);
        logger.LogError(exception, "Unhandled exception for {Method} {Path}",
            httpContext.Request.Method, httpContext.Request.Path);

        var status = databaseDown ? StatusCodes.Status503ServiceUnavailable : StatusCodes.Status500InternalServerError;
        httpContext.Response.StatusCode = status;

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Status = status,
                Title = databaseDown ? "The database is unavailable" : "Something went wrong",
                Detail = "Please try again in a moment.",
            },
        });
    }

    private static bool IsDatabaseError(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is DbException)
            {
                return true;
            }
        }

        return false;
    }
}
