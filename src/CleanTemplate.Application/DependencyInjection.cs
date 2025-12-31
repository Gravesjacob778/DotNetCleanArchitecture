using CleanTemplate.Application.TodoItems;
using Microsoft.Extensions.DependencyInjection;

namespace CleanTemplate.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITodoItemService, TodoItemService>();
        return services;
    }
}
