using TaskSystem.Api.Dtos;
using TaskSystem.Api.Extensions;
using TaskSystem.Api.Services;

namespace TaskSystem.Api.Endpoints;

public static class TaskEndpoints
{
    public static RouteGroupBuilder MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tasks")
            .WithTags("Tasks");

        group.MapGet("/", GetTasks)
            .WithName("GetTasks")
            .WithSummary("Get all tasks for the logged-in user.")
            .Produces<IReadOnlyList<TaskResponse>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized);

        group.MapPost("/", CreateTask)
            .WithName("CreateTask")
            .WithSummary("Create a task for the logged-in user.")
            .Produces<TaskResponse>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized);

        group.MapPut("/{id}", UpdateTask)
            .WithName("UpdateTask")
            .WithSummary("Update a task for the logged-in user.")
            .Produces<TaskResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id}", DeleteTask)
            .WithName("DeleteTask")
            .WithSummary("Delete a task for the logged-in user.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> GetTasks(ITaskService taskService, CancellationToken cancellationToken)
    {
        var tasks = await taskService.GetTasksForCurrentUserAsync(cancellationToken);
        return Results.Ok(tasks);
    }

    private static async Task<IResult> CreateTask(CreateTaskRequest request, ITaskService taskService, CancellationToken cancellationToken)
    {
        request.ValidateRequest();
        var task = await taskService.CreateTaskAsync(request, cancellationToken);
        return Results.Created($"/api/tasks/{task.Id}", task);
    }

    private static async Task<IResult> UpdateTask(string id, UpdateTaskRequest request, ITaskService taskService, CancellationToken cancellationToken)
    {
        request.ValidateRequest();
        var task = await taskService.UpdateTaskAsync(id, request, cancellationToken);
        return Results.Ok(task);
    }

    private static async Task<IResult> DeleteTask(string id, ITaskService taskService, CancellationToken cancellationToken)
    {
        await taskService.DeleteTaskAsync(id, cancellationToken);
        return Results.NoContent();
    }
}
