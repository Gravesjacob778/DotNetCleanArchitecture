namespace CleanTemplate.WebApi.Middleware;

public static class ResponseWrappingMiddlewareExtensions
{
    public static IApplicationBuilder UseResponseWrapping(this IApplicationBuilder app)
        => app.UseMiddleware<ResponseWrappingMiddleware>();
}
