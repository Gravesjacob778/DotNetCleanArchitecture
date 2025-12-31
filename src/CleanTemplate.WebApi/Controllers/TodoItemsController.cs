using CleanTemplate.Application.TodoItems;
using CleanTemplate.Application.TodoItems.Models;
using CleanTemplate.WebApi.Models.TodoItems;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanTemplate.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/todoitems")]
public sealed class TodoItemsController(ITodoItemService todoItemService) : ControllerBase
{
    private readonly ITodoItemService _todoItemService = todoItemService;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TodoItemDto>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await _todoItemService.GetAllAsync(cancellationToken);
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateTodoItemRequest request, CancellationToken cancellationToken)
    {
        var id = await _todoItemService.CreateAsync(request.Title, cancellationToken);
        return Ok(id);
    }
}
