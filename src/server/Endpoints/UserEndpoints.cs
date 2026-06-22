using TaskSystem.Api.Dtos;
using TaskSystem.Api.Extensions;
using TaskSystem.Api.Services;

namespace TaskSystem.Api.Endpoints;

public static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users");

        group.MapGet("/", GetUsers)
            .WithName("GetUsers")
            .WithSummary("Get all users.")
            .Produces<IReadOnlyList<UserSummaryResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{email}/status", GetUserStatus)
            .WithName("GetUserStatus")
            .WithSummary("Check whether a user exists and return their status.")
            .Produces<UserStatusResponse>(StatusCodes.Status200OK);

        group.MapPost("/", CreateUser)
            .WithName("CreateUser")
            .WithSummary("Create a new user.")
            .Produces<UserResponse>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status409Conflict);

        return group;
    }

    private static async Task<IResult> GetUsers(IUserService userService, CancellationToken cancellationToken)
    {
        var users = await userService.GetAllUsersAsync(cancellationToken);
        return Results.Ok(users);
    }

    private static async Task<IResult> GetUserStatus(string email, IUserService userService, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Results.BadRequest(new ErrorResponse("Email is required."));
        }

        var status = await userService.GetStatusByEmailAsync(email, cancellationToken);
        return Results.Ok(status);
    }

    private static async Task<IResult> CreateUser(CreateUserRequest request, IUserService userService, CancellationToken cancellationToken)
    {
        request.ValidateRequest();
        var user = await userService.CreateUserAsync(request, cancellationToken);
        return Results.Created($"/api/users/{user.Email}/status", user);
    }
}
