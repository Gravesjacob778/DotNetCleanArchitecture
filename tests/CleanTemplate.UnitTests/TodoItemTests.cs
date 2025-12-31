using CleanTemplate.Domain.Entities;

namespace CleanTemplate.UnitTests;

public class TodoItemTests
{
    [Fact]
    public void NewTodoItem_UsesDefaultValues()
    {
        var item = new TodoItem();

        Assert.False(item.IsDone);
        Assert.Equal(string.Empty, item.Title);
        Assert.NotEqual(default, item.CreatedAtUtc);
    }
}
