using TaskSystem.Api.Entities;
using TaskSystem.Api.Extensions;
using Microsoft.EntityFrameworkCore;

namespace TaskSystem.Api.Data;

public static class DbInitializer
{
    public static async System.Threading.Tasks.Task InitializeAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();

        if (!await context.Categories.AnyAsync())
        {
            context.Categories.AddRange(
                new Category { Id = SeedIds.Epic1CategoryId, Name = "Epic 1" },
                new Category { Id = SeedIds.Epic2CategoryId, Name = "Epic 2" },
                new Category { Id = SeedIds.Epic3CategoryId, Name = "Epic 3" },
                new Category { Id = SeedIds.HackathonCategoryId, Name = "Hackathon" },
                new Category { Id = SeedIds.CultureCategoryId, Name = "Culture" });
            await context.SaveChangesAsync();
        }

        if (await context.Users.AnyAsync())
        {
            return;
        }

        var createDate = DateFormats.ToStorageString(DateTime.UtcNow);

        context.Users.Add(new User
        {
            Id = SeedIds.DemoUserId,
            Email = "demo@tasksystem.com",
            FirstName = "Demo",
            LastName = "User"
        });

        context.Tasks.AddRange(
            new TaskSystem.Api.Entities.Task
            {
                Id = SeedIds.DemoTask1Id,
                CategoryId = SeedIds.Epic1CategoryId,
                Title = "Review API docs",
                Description = "Confirm Swagger and README are accurate.",
                Creator = SeedIds.DemoUserId,
                CreateDate = createDate
            },
            new TaskSystem.Api.Entities.Task
            {
                Id = SeedIds.DemoTask2Id,
                CategoryId = SeedIds.HackathonCategoryId,
                Title = "Build React frontend",
                Description = "Connect to the task endpoints.",
                Creator = SeedIds.DemoUserId,
                CreateDate = createDate
            });

        await context.SaveChangesAsync();
    }
}
