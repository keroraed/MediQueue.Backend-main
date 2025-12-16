namespace MediQueue.APIs.Errors;

public class ApiResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; }

    public ApiResponse(int statusCode, string message = null)
    {
        StatusCode = statusCode;
        Message = message ?? GetDefaultMessageForStatusCode(statusCode);
    }

    private string GetDefaultMessageForStatusCode(int statusCode)
    {
        return statusCode switch
        {
            200 => "Success",
            201 => "Created successfully",
            204 => "No content",
            400 => "Bad request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Resource not found",
            500 => "Internal server error",
            _ => "Unknown error occurred"
        };
    }
}
