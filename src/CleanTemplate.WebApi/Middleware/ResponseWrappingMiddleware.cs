using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.Json;
using CleanTemplate.Application.Common.Models;

namespace CleanTemplate.WebApi.Middleware;

public sealed class ResponseWrappingMiddleware
{
    private const string DefaultErrorMessage = "An unexpected error occurred.";
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ResponseWrappingMiddleware> _logger;

    public ResponseWrappingMiddleware(RequestDelegate next, ILogger<ResponseWrappingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Request started {Method} {Path} {TraceId}",
            context.Request.Method,
            context.Request.Path,
            context.TraceIdentifier);

        var originalBody = context.Response.Body;
        await using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        try
        {
            await _next(context);
            await HandleResponseAsync(context, originalBody, buffer);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, originalBody, ex);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation("Request completed {Method} {Path} {StatusCode} {ElapsedMs}ms {ContentType}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.Elapsed.TotalMilliseconds,
                context.Response.ContentType ?? "unknown");
            context.Response.Body = originalBody;
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Stream originalBody, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            _logger.LogWarning(exception, "Response already started. Re-throwing exception.");
            ExceptionDispatchInfo.Capture(exception).Throw();
        }

        _logger.LogError(exception, "Unhandled exception. Returning 500 response.");
        context.Response.Body = originalBody;
        context.Response.Clear();
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await WriteWrappedAsync(context, originalBody, HttpActionResponse.Fail(DefaultErrorMessage));
    }

    private static async Task HandleResponseAsync(HttpContext context, Stream originalBody, MemoryStream buffer)
    {
        context.Response.Body = originalBody;

        if (context.Response.StatusCode == StatusCodes.Status204NoContent)
        {
            _logger.LogDebug("Response status 204. Skipping wrapping.");
            await CopyBufferAsync(buffer, originalBody);
            return;
        }

        if (HasNonJsonContentType(context.Response.ContentType))
        {
            _logger.LogDebug("Non-JSON content type {ContentType}. Skipping wrapping.", context.Response.ContentType);
            await CopyBufferAsync(buffer, originalBody);
            return;
        }

        var responseBody = await ReadBufferAsync(buffer);

        if (context.Response.StatusCode is >= 200 and < 300)
        {
            await HandleSuccessAsync(context, originalBody, buffer, responseBody);
            return;
        }

        if (context.Response.StatusCode >= 400)
        {
            await HandleFailureAsync(context, originalBody, buffer, responseBody);
            return;
        }

        await CopyBufferAsync(buffer, originalBody);
    }

    private async Task HandleSuccessAsync(
        HttpContext context,
        Stream originalBody,
        MemoryStream buffer,
        string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            _logger.LogDebug("Empty success response. Wrapping with default message.");
            await WriteWrappedAsync(context, originalBody, HttpActionResponse.Ok(null, "Success"));
            return;
        }

        if (!TryParseJson(responseBody, out var jsonElement))
        {
            _logger.LogDebug("Response body is not valid JSON. Skipping wrapping.");
            await CopyBufferAsync(buffer, originalBody);
            return;
        }

        if (IsWrapped(jsonElement))
        {
            _logger.LogDebug("Response already wrapped. Passing through.");
            await CopyBufferAsync(buffer, originalBody);
            return;
        }

        _logger.LogDebug("Wrapping successful response.");
        object? data = jsonElement.ValueKind == JsonValueKind.Null ? null : jsonElement;
        await WriteWrappedAsync(context, originalBody, HttpActionResponse.Ok(data, "Success"));
    }

    private async Task HandleFailureAsync(
        HttpContext context,
        Stream originalBody,
        MemoryStream buffer,
        string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            _logger.LogDebug("Empty error response. Wrapping with default message.");
            await WriteWrappedAsync(context, originalBody, HttpActionResponse.Fail(DefaultErrorMessage));
            return;
        }

        _logger.LogDebug("Error response has content. Passing through.");
        await CopyBufferAsync(buffer, originalBody);
    }

    private static bool HasNonJsonContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return false;
        }

        return !contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase)
            && !contentType.Contains("+json", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<string> ReadBufferAsync(MemoryStream buffer)
    {
        buffer.Position = 0;
        using var reader = new StreamReader(buffer, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        buffer.Position = 0;
        return body;
    }

    private static async Task CopyBufferAsync(MemoryStream buffer, Stream destination)
    {
        buffer.Position = 0;
        await buffer.CopyToAsync(destination);
    }

    private static async Task WriteWrappedAsync(HttpContext context, Stream destination, HttpActionResponse payload)
    {
        context.Response.ContentLength = null;
        context.Response.ContentType = "application/json; charset=utf-8";
        await JsonSerializer.SerializeAsync(destination, payload, SerializerOptions);
    }

    private static bool TryParseJson(string body, out JsonElement element)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            element = document.RootElement.Clone();
            return true;
        }
        catch (JsonException)
        {
            element = default;
            return false;
        }
    }

    private static bool IsWrapped(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        return HasProperty(element, "success")
            && HasProperty(element, "message")
            && HasProperty(element, "data");
    }

    private static bool HasProperty(JsonElement element, string name)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
