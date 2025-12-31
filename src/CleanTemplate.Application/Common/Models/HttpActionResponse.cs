namespace CleanTemplate.Application.Common.Models;

public sealed record HttpActionResponse(bool Success, string Message, object? Data)
{
    public static HttpActionResponse Ok(object? data, string message = "Success")
        => new(true, message, data);

    public static HttpActionResponse Fail(string message)
        => new(false, message, null);
}
