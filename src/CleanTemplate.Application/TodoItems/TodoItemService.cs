using CleanTemplate.Application.Common.Interfaces;
using CleanTemplate.Application.TodoItems.Models;
using CleanTemplate.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanTemplate.Application.TodoItems;

public sealed class TodoItemService : ITodoItemService
{
    private readonly IApplicationDbContext _context;

    public TodoItemService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<TodoItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.TodoItems
            .AsNoTracking()
            .OrderBy(item => item.CreatedAtUtc)
            .Select(item => new TodoItemDto(item.Id, item.Title, item.IsDone, item.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid> CreateAsync(string title, CancellationToken cancellationToken = default)
    {
        var entity = new TodoItem
        {
            Id = Guid.NewGuid(),
            Title = title,
            IsDone = false,
            CreatedAtUtc = DateTime.UtcNow,
        };

        _context.TodoItems.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
