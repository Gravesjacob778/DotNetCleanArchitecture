using CleanTemplate.Domain.Common;

namespace CleanTemplate.Domain.Entities;

public sealed class TodoItem : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public bool IsDone { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.Now;
}
