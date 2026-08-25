using System.Net;
using System.Text.Json;
using Neo4j.Driver;

namespace DevConnect.Api.Middleware
{
    /// <summary>
    /// Central error handler. Converts CognoDB/Neo4j connectivity failures
    /// into a clean, user-friendly JSON response instead of a raw 500 stack
    /// trace — this is what makes "database unreachable" a graceful
    /// experience for the frontend instead of a crash.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ServiceUnavailableException ex)
            {
                await WriteErrorAsync(context, HttpStatusCode.ServiceUnavailable,
                    "The graph database is temporarily unreachable. Please try again shortly.", ex);
            }
            catch (Neo4jException ex)
            {
                await WriteErrorAsync(context, HttpStatusCode.ServiceUnavailable,
                    "Could not complete the request due to a database error.", ex);
            }
            catch (InvalidOperationException ex) when (ex.InnerException is Neo4jException)
            {
                await WriteErrorAsync(context, HttpStatusCode.ServiceUnavailable,
                    "Could not complete the request due to a database error.", ex);
            }
            catch (Exception ex)
            {
                await WriteErrorAsync(context, HttpStatusCode.InternalServerError,
                    "An unexpected error occurred.", ex);
            }
        }

        private async Task WriteErrorAsync(HttpContext context, HttpStatusCode statusCode, string message, Exception ex)
        {
            _logger.LogError(ex, "Request failed: {Message}", message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var payload = JsonSerializer.Serialize(new
            {
                error = message,
                status = (int)statusCode
            });

            await context.Response.WriteAsync(payload);
        }
    }
}
