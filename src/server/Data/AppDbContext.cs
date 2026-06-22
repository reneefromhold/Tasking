using TaskSystem.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace TaskSystem.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<TaskSystem.Api.Entities.Task> Tasks => Set<TaskSystem.Api.Entities.Task>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).HasColumnName("id");
            entity.Property(u => u.FirstName).HasColumnName("first_name").IsRequired();
            entity.Property(u => u.LastName).HasColumnName("last_name").IsRequired();
            entity.Property(u => u.Email).HasColumnName("email").IsRequired();
            entity.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("idx_user_email");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).HasColumnName("id");
            entity.Property(c => c.Name).HasColumnName("category").IsRequired();
            entity.HasIndex(c => c.Name)
                .IsUnique();
        });

        modelBuilder.Entity<TaskSystem.Api.Entities.Task>(entity =>
        {
            entity.ToTable("Tasks");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).HasColumnName("id");
            entity.Property(t => t.CategoryId).HasColumnName("categoryId");
            entity.Property(t => t.Title).HasColumnName("title").HasMaxLength(30).IsRequired();
            entity.Property(t => t.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(t => t.Creator).HasColumnName("creator");
            entity.Property(t => t.Assignee).HasColumnName("assignee");
            entity.Property(t => t.CreateDate).HasColumnName("createDate").HasMaxLength(25).IsRequired();
            entity.Property(t => t.DueDate).HasColumnName("dueDate").HasMaxLength(25);
            entity.HasIndex(t => t.Assignee)
                .HasDatabaseName("idx_tasks_assignee");
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(t => t.Creator)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(t => t.Assignee)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<Category>()
                .WithMany()
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
