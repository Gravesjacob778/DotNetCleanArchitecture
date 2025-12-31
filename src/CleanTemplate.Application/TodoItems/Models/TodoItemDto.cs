namespace CleanTemplate.Application.TodoItems.Models;

public sealed record TodoItemDto(Guid Id, string Title, bool IsDone, DateTime CreatedAtUtc);
