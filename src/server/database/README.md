# Database schema v1

The canonical table definitions for this project are:

- [`user-table.sql`](user-table.sql) — `"User"` table and email index
- [`categories-table.sql`](categories-table.sql) — `Categories` table (seeded, unique category names)
- [`tasks-table.sql`](tasks-table.sql) — `Tasks` table and foreign keys

EF Core migrations in `Data/Migrations/` are generated from the entity models in `Entities/` and `Data/AppDbContext.cs`, which are mapped to match this v1 schema. The repo keeps a single `InitialCreate` migration while the schema is still net new — add incremental migrations once the project is committed and shared.

`Tasks.categoryId` references `Categories.id`. `Tasks.creator` and `Tasks.assignee` reference `"User".id` via foreign keys.

On first run, `dotnet run` applies migrations and creates `tasksystem.db` locally. The `.db` file is gitignored.
