# TaskSystem

Monorepo for a to-do task management application. The **backend** is an ASP.NET Core minimal API; the **frontend** is a React + Vite SPA. Both projects were originally separate repositories and now live under `src/`.

| App | Path | Stack | Dev URL |
|-----|------|-------|---------|
| API | `src/server` | ASP.NET Core 10, EF Core, SQLite | http://localhost:5044 |
| Web | `src/client` | React 19, TypeScript, Vite | http://localhost:5173 |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (LTS recommended)

```bash
dotnet --version
node --version
```

## Quick start

Copy the example config files, then run the API and frontend in separate terminals.

**Setup (once)**

```bash
# API — optional if you use the committed appsettings.json as-is
cp src/server/appsettings.example.json src/server/appsettings.json

# Frontend
cp src/client/.env.example src/client/.env
```

On Windows PowerShell, use `Copy-Item` instead of `cp`.

**Terminal 1 — API**

```bash
cd src/server
dotnet restore
dotnet run --launch-profile http
```

**Terminal 2 — Frontend**

```bash
cd src/client
npm install
npm run dev
```

Open http://localhost:5173. The frontend expects the API at `http://localhost:5044` by default (see [Configuration](#configuration)).

On first API run, EF Core applies migrations and seeds a demo user:

| Field | Value |
|-------|-------|
| Email | `demo@tasksystem.com` |
| User ID | `11111111-1111-1111-1111-111111111111` |

### Demo flow

1. Start the API and frontend (see above).
2. Sign in with the seeded demo user or create a new user at `/create-user`.
3. Open `/tasks` — list, add, edit, and delete tasks use the stored user id automatically.

### Swagger (Development)

- Swagger UI: http://localhost:5044/swagger
- OpenAPI JSON: http://localhost:5044/openapi/v1.json

Task endpoints require the `X-User-Id` header. Use the demo user ID above for seeded data.

## Project structure

```
Tasking/
├── README.md
└── src/
    ├── server/            # ASP.NET Core API
    │   ├── appsettings.example.json
    │   ├── Auth/          # Current user abstraction (header-based)
    │   ├── database/      # v1 SQL schema definitions (source of truth)
    │   ├── Data/          # DbContext, migrations, seed data
    │   ├── Dtos/          # Request/response models
    │   ├── Endpoints/     # Route definitions (no business/SQL logic)
    │   ├── Entities/      # EF Core domain models
    │   ├── Exceptions/    # Application-level exceptions
    │   ├── Extensions/    # DI registration, validation helpers
    │   ├── Middleware/    # Global exception handling
    │   ├── Repositories/  # Data access via EF Core
    │   ├── Services/      # Business logic
    │   ├── TaskSystem.Api.Tests/
    │   └── Program.cs
    └── client/            # React frontend
        ├── .env.example
        ├── src/
        │   ├── api/       # API client
        │   ├── auth/      # Auth context and session
        │   ├── components/
        │   └── pages/     # Login, create user, tasks
        └── package.json
```

## Authentication

This app uses a **minimal identity flow** for local demo and assessment review — not production-grade auth. That is a deliberate choice for a **4–6 hour** take-home scope, not an oversight. There are no passwords, JWTs, or server-side sessions.

Within that time budget, the priority was working CRUD, persistence, validation, tests, and end-to-end frontend integration. A production auth system would consume most of the window and leave core task features unfinished.

### End-to-end flow

1. User enters email on `/login` (or registers on `/create-user`).
2. Frontend calls `GET /api/users/{email}/status` or `POST /api/users`.
3. User `{ id, email }` is stored in `localStorage` (`tasksystem.auth`).
4. Task requests include `X-User-Id: <userId>`.
5. The API scopes list/update/delete to tasks the user **created** (`creator`).

That closes the loop for **demo and evaluation**: identify user → persist identity → act on tasks as that user. It does **not** close the loop for **security**: there is no step that proves the caller owns the email or user ID.

### Frontend behavior

**Sign-in (`/login`)**
- User enters email (the API's user identifier).
- App calls `GET /api/users/{email}/status`.
- On success, stores `{ id, email }` and redirects to `/tasks`.

**Registration (`/create-user`)**
- User submits email, first name, and last name.
- App calls `POST /api/users` and logs the user in with the returned id.

**Session**
- Logged-in user is persisted in `localStorage` (`tasksystem.auth`).
- `AuthProvider` restores the session on page load.

**Route protection**
- `/tasks` requires a stored user (`ProtectedRoute` → otherwise `/login`).
- `/login` and `/create-user` redirect to `/tasks` if already signed in.

**API identity (task ownership loop)**
- Task endpoints send `X-User-Id` with the stored user id:
  - `GET /api/tasks`
  - `POST /api/tasks`
  - `PUT /api/tasks/{id}`
  - `DELETE /api/tasks/{id}`

**Logout**
- Clears `localStorage` and returns to `/login`.

**Endpoints that do not send `X-User-Id`** (public reads used before or outside task ownership):

- `GET /api/users/{email}/status` — login lookup
- `GET /api/users` — assignee dropdown
- `GET /api/categories` — category picker

### API behavior

| Layer | Behavior |
|-------|----------|
| **Identity (tasks only)** | Client sends `X-User-Id: <guid>`. `HeaderCurrentUserAccessor` reads and trims this header. |
| **Existence check** | `TaskService` verifies the ID exists in the `User` table. Missing header → **401**. Unknown user ID → **401**. |
| **Authorization** | Task list/update/delete scoped to **creator** (`creator == X-User-Id`). Another user's task → **404** (not **403**) to avoid leaking existence. |
| **User endpoints** | `GET/POST /api/users` and `GET /api/users/{email}/status` are **open** — no header required. |

There is no cryptographic proof that the caller *is* that user. The header is a **self-asserted identity** suitable for local demo only.

### What is not implemented

- Passwords or credential verification
- Bearer tokens, cookies, refresh flows, or server-side sessions
- OAuth / SSO
- Email verification or magic links
- Session expiry, idle timeout, or role-based access control
- Rate limiting or abuse protection on open user registration
- `[Authorize]` or ASP.NET Core authentication middleware

### Extension point

`ICurrentUserAccessor` abstracts "who is the current user?" Services depend on the interface, not the header. A future `JwtCurrentUserAccessor` (or Identity integration) could validate credentials without rewriting `TaskService`.

Register in `ServiceCollectionExtensions`:

```csharp
services.AddScoped<ICurrentUserAccessor, HeaderCurrentUserAccessor>();
```

Replace that single registration when adding real auth.

### Auth trade-offs

| Choice | Why, in a short take-home | Cost |
|--------|---------------------------|------|
| Header-based identity instead of JWT/Identity | Ships a working task API + frontend integration in the time box | Any client can impersonate any `userId` |
| Open user registration | Simple onboarding for demo; no account system to build | Spam/duplicate accounts; not production-safe |
| Validate user exists before task ops | Prevents orphan FKs and gives clear **401** errors | Still does not prove the caller *is* that user |
| Creator-based task access | Clear ownership model; easy to test | `assignee` is metadata only; assignee cannot manage tasks created for them |
| **404** instead of **403** on wrong owner | Hides task existence from non-owners | Less obvious to API consumers than **403 Forbidden** |

### If authentication were required next

1. Add credentials (e.g. ASP.NET Core Identity with email + password, or magic-link email).
2. Issue signed tokens (JWT) on successful login.
3. Replace `HeaderCurrentUserAccessor` with middleware that validates the token and sets the user ID.
4. Protect `POST /api/users` or gate it behind admin/signup policy.
5. Add tests for unauthorized and expired token paths.

### Security note

This flow is suitable for **local demo and assessment review**, not production. Anyone who knows a valid user id could call task endpoints with that id.

## API endpoints

### Users

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/users` | List all users |
| `GET` | `/api/users/{email}/status` | Check if a user exists and return their status |
| `POST` | `/api/users` | Create a new user |

**User status response**

```json
{
  "exists": true,
  "status": "active",
  "id": "11111111-1111-1111-1111-111111111111"
}
```

When the user does not exist, `exists` is `false`, `status` is `not_found`, and `id` is omitted.

| Status | Meaning |
|--------|---------|
| `active` | User exists |
| `not_found` | No user with that email (`exists: false`) |

**Create user**

```json
{
  "email": "user@example.com",
  "firstName": "Jane",
  "lastName": "Doe"
}
```

### Categories

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/categories` | List seeded categories (read-only) |

**List categories response**

```json
[
  { "id": "33333333-3333-3333-3333-333333333331", "category": "Epic 1" },
  { "id": "33333333-3333-3333-3333-333333333332", "category": "Epic 2" }
]
```

Categories are seed-only — there is no endpoint to create them.

Seeded values: Epic 1, Epic 2, Epic 3, Hackathon, Culture.

### Tasks (requires `X-User-Id` header)

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/tasks` | List tasks for the logged-in user |
| `POST` | `/api/tasks` | Create a task |
| `PUT` | `/api/tasks/{id}` | Update a task |
| `DELETE` | `/api/tasks/{id}` | Delete a task |

**Create task**

```json
{
  "categoryId": "33333333-3333-3333-3333-333333333331",
  "title": "My task",
  "description": "Optional details",
  "assignee": "11111111-1111-1111-1111-111111111111",
  "dueDate": "2026-12-31T23:59:59Z"
}
```

**Update task**

```json
{
  "categoryId": "33333333-3333-3333-3333-333333333334",
  "title": "Updated title",
  "description": "Updated details",
  "assignee": "11111111-1111-1111-1111-111111111111",
  "dueDate": "2026-12-31T23:59:59Z"
}
```

### Example requests

```bash
# Check user status
curl http://localhost:5044/api/users/demo@tasksystem.com/status

# List users
curl http://localhost:5044/api/users

# Create user
curl -X POST http://localhost:5044/api/users \
  -H "Content-Type: application/json" \
  -d '{"email":"new@example.com","firstName":"New","lastName":"User"}'

# List categories
curl http://localhost:5044/api/categories

# List tasks (demo user)
curl http://localhost:5044/api/tasks -H "X-User-Id: 11111111-1111-1111-1111-111111111111"

# Create task
curl -X POST http://localhost:5044/api/tasks \
  -H "Content-Type: application/json" \
  -H "X-User-Id: 11111111-1111-1111-1111-111111111111" \
  -d '{"title":"Ship MVP","description":"Finish backend"}'
```

See `src/server/TaskSystem.Api.http` for more examples.

## Frontend routes

| Route | Description |
|-------|-------------|
| `/login` | Sign in with email |
| `/create-user` | Register a new user |
| `/tasks` | Task list and CRUD (protected) |

See [Configuration](#configuration) if the API runs on a different host or port.

## Configuration

Example files are committed; copy them to create local config (see [Quick start](#quick-start)).

### Frontend — `src/client/.env.example` → `.env`

| Variable | Default | Description |
|----------|---------|-------------|
| `VITE_API_BASE_URL` | `http://localhost:5044` | Base URL for the TaskSystem API. No trailing slash. Change this if the API runs on another host or port. |

If unset, the app falls back to `http://localhost:5044` in `src/client/src/api/client.ts`.

### API — `src/server/appsettings.example.json` → `appsettings.json`

| Setting | Default | Description |
|---------|---------|-------------|
| `ConnectionStrings:DefaultConnection` | `Data Source=tasksystem.db` | SQLite database file path, relative to `src/server`. Use an absolute path or a different filename for a separate local database. |
| `Logging:LogLevel:Default` | `Information` | Minimum log level for app code. |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Warning` | Reduces noise from ASP.NET Core framework logs. |
| `AllowedHosts` | `*` | Host filtering. `*` is fine for local development. |

CORS is configured in code (not `appsettings.json`) and allows React dev servers at `http://localhost:5173` and `http://localhost:3000`.

## Database

- **Provider:** SQLite (`tasksystem.db` in `src/server`)
- **ORM:** EF Core 10
- **Schema v1:** see `src/server/database/` for canonical SQL definitions
- **Migrations:** `src/server/Data/Migrations/` (single `InitialCreate` matching v1)

### v1 tables

**`User`**

| Column | Type |
|--------|------|
| `id` | TEXT (PK) |
| `first_name` | TEXT |
| `last_name` | TEXT |
| `email` | TEXT (unique) |

**`Categories`** (seeded)

| Column | Type |
|--------|------|
| `id` | TEXT (PK) |
| `category` | TEXT (unique) |

**`Tasks`**

| Column | Type |
|--------|------|
| `id` | TEXT (PK) |
| `categoryId` | TEXT (FK → `Categories.id`) |
| `title` | TEXT (max 30) |
| `description` | TEXT (max 500) |
| `creator` | TEXT (FK → `"User".id`) |
| `assignee` | TEXT (FK → `"User".id`) |
| `createDate` | TEXT (max 25) |
| `dueDate` | TEXT (max 25) |

Apply migrations manually:

```bash
cd src/server
dotnet ef database update
dotnet ef migrations add <MigrationName> --output-dir Data/Migrations
```

If you previously ran an older schema locally, stop the API, close DBeaver if connected, and delete `src/server/tasksystem.db*` before restarting.

## Tests

Unit tests live in `TaskSystem.Api.Tests` (xUnit + Moq). They cover service CRUD logic, DTO JSON shapes, request validation, and error middleware — no database or HTTP required.

```bash
cd src/server
dotnet test TaskSystem.Api.Tests/TaskSystem.Api.Tests.csproj
```

Pull requests run the same test suite via GitHub Actions (`.github/workflows/test.yml`).

## Architecture

### Backend

- **Layered minimal APIs** — Endpoints delegate to services; services use repositories; repositories use EF Core. No SQL in endpoint handlers.
- **SQLite + EF Core** — Persistent local database with real migrations. Easy to swap to SQL Server or PostgreSQL later by changing the connection string and provider.
- **Repository pattern** — Keeps data access testable and isolated from business rules.
- **`ICurrentUserAccessor`** — Auth is abstracted so a real auth mechanism can replace the header approach without rewriting services.
- **Global exception middleware** — Consistent JSON error responses across endpoints.

### Frontend

- **React SPA** — `AuthProvider` + `ProtectedRoute` guard task pages; API client attaches the user header on task calls.
- **`TasksPage`** loads all users and all categories on mount for form dropdowns (see [Future improvements](#future-improvements) for scaling notes).

### Production MVP features included

- Layered architecture with DI
- EF Core with migrations
- Input validation on request DTOs
- Global error handling with consistent JSON errors
- CORS for frontend integration
- OpenAPI/Swagger documentation
- Seed data for local development
- Documented auth decision and demo identity flow (header-based, not login)
- `ICurrentUserAccessor` abstraction ready for a future auth provider
- Unit tests (services, DTOs, validation, middleware)
- CI pipeline (GitHub Actions tests on pull request)

## Assumptions

1. **Email** is the unique user identifier for login lookup.
2. **No real authentication** — see [Authentication](#authentication). User creation is open; task operations use a self-asserted `X-User-Id` header after the frontend identity flow.
3. **Task ownership** — users can only list/update/delete tasks they **created** (`creator`). Cross-user access returns `404` (not `403`) to avoid leaking resource existence. `assignee` is stored but does not drive access control.
4. **GET /api/tasks** — added because a to-do frontend needs to list tasks, even though it was not explicitly listed in the original requirements.

## Trade-offs

| Choice | Why | Trade-off |
|--------|-----|-----------|
| Header identity instead of real login | Frees time in a 4–6h box for CRUD, DB, tests, and API quality | Not production-secure; any client can send any `X-User-Id` |
| SQLite | Simple local setup, real persistence | Not ideal for high-concurrency production |
| `404` for unauthorized task access | Prevents information leakage | Less explicit than `403` |
| Global exception middleware | Consistent error responses | Less granular per-endpoint control |
| Seed data in dev | Easy demo/testing | Must be disabled or isolated in production |

## Troubleshooting

| Issue | Fix |
|-------|-----|
| `dotnet` not found | Install .NET 10 SDK and restart your terminal |
| Frontend cannot reach API | Confirm API is running on port 5044; check `VITE_API_BASE_URL` |
| `401` on task endpoints | Include a valid `X-User-Id` header (demo user ID above) |
| `/swagger` returns 404 | Ensure `ASPNETCORE_ENVIRONMENT=Development` |
| `table "Tasks" already exists` or schema errors | Stop the API, close DBeaver if connected, delete `src/server/tasksystem.db*`, restart |

## Future improvements

### Backend

- [ ] Proper authentication (JWT, Identity, etc.) — close the security loop; see [Authentication](#authentication)
- [ ] Integration / end-to-end API tests (HTTP + database)
- [ ] Pagination and filtering on task list
- [ ] Soft delete for tasks
- [ ] Structured logging (Serilog)
- [ ] Health checks (`/health`)
- [ ] Rate limiting and request logging
- [ ] Move secrets and CORS origins to environment variables / Azure Key Vault

### Frontend

- [ ] Harden authentication (passwords, JWT/sessions, validated tokens instead of trusted `X-User-Id`)
- [ ] Add **priority** field to tasks
- [ ] Support **drag-and-drop** reordering to assign/update priority
- [ ] CTA for approaching due dates or tasks past due
- [ ] Refactor **category** and **assignee** pickers to scale beyond loading full lists (server-side search, pagination, typeahead API)
- [ ] Adopt React Router **loaders/actions** (`createBrowserRouter`) for route-centric data fetching and bookmarkable URLs
- [ ] Add ability to **create new categories** (requires backend endpoint)
- [ ] Revamp CSS to use **Tailwind CSS** (replace the global `index.css` approach)
