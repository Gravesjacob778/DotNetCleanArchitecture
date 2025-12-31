using CleanTemplate.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanTemplate.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TodoItem> TodoItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
