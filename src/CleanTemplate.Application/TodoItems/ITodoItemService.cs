using CleanTemplate.Application.TodoItems.Models;

namespace CleanTemplate.Application.TodoItems;

public interface ITodoItemService
{
    Task<IReadOnlyList<TodoItemDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(string title, CancellationToken cancellationToken = default);
}
