using MediQueue.APIs.Errors;
using System.Net;
using System.Text.Json;

namespace MediQueue.APIs.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        _logger.LogError(ex, ex.Message);
        context.Response.ContentType = "application/json";

        // Map exception types to appropriate HTTP status codes
        // Note: More specific exceptions must come before their base types
        var statusCode = ex switch
        {
            KeyNotFoundException => HttpStatusCode.NotFound,
            UnauthorizedAccessException => HttpStatusCode.Forbidden,
            InvalidOperationException => HttpStatusCode.BadRequest,
            ArgumentNullException => HttpStatusCode.BadRequest,
            ArgumentException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };

        context.Response.StatusCode = (int)statusCode;

        var response = _env.IsDevelopment()
            ? new ApiException((int)statusCode, ex.Message, ex.StackTrace?.ToString())
            : new ApiException((int)statusCode, GetUserFriendlyMessage(statusCode));

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(response, options);

        await context.Response.WriteAsync(json);
    }

    private static string GetUserFriendlyMessage(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.NotFound => "The requested resource was not found",
            HttpStatusCode.Forbidden => "You do not have permission to access this resource",
            HttpStatusCode.BadRequest => "The request was invalid",
            HttpStatusCode.InternalServerError => "An internal server error occurred",
            _ => "An error occurred processing your request"
        };
    }
}
