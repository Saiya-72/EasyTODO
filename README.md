# Todo App — TypeScript + C# scaffold

A working React/TypeScript frontend (on mock data) and a C# ASP.NET Core backend
with dummy endpoints and step-by-step `TODO(you)` comments for the real implementation.

```
todo-app/
├── docker-compose.yml          # runs frontend + backend together
├── frontend/                   # Vite + React + TypeScript
│   ├── vite.config.ts          # proxies /api -> backend (no CORS headaches)
│   └── src/
│       ├── api/                # <- YOU WRITE THE FETCH CALLS HERE
│       │   ├── http.ts         #    shared helper to write first (instructions inside)
│       │   ├── health.ts       #    one finished example call
│       │   ├── auth.ts
│       │   ├── todos.ts
│       │   └── categories.ts
│       ├── mocks/mockStore.ts  # fake backend used while VITE_USE_MOCKS=true
│       ├── context/            # AuthContext (who's signed in)
│       ├── components/         # UI — done, shouldn't need changes
│       └── types.ts            # shared shapes; mirror of backend DTOs
└── backend/
    ├── Dockerfile              # dev target (dotnet watch) + production target
    └── TodoApi/
        ├── Program.cs          # startup; TODO STEP 1 (DB) and STEP 2 (JWT)
        ├── Controllers/        # dummy endpoints + TODO(you) instructions
        ├── Models/Entities.cs  # database entities
        ├── Dtos/Dtos.cs        # request/response shapes
        ├── Data/AppDbContext.cs# EF Core context (commented out until you add the package)
        ├── Services/TokenService.cs
        └── TodoApi.http        # click-to-send test requests
```

## Run it

### Option A: Docker (everything)

```bash
docker compose up --build
```

- Frontend: http://localhost:5173
- Backend health: http://localhost:5080/api/health

Both containers hot-reload from your source folders. The green dot in the app header means
the frontend can reach the backend.

### Option B: Locally (no Docker)

Requires Node 20+ and the .NET 10 SDK.

```bash
# terminal 1
cd backend/TodoApi
dotnet watch run            # http://localhost:5080

# terminal 2
cd frontend
npm install
npm run dev                 # http://localhost:5173
```

Optional: `cp frontend/.env.example frontend/.env.local` to change settings.

## Mock mode vs real backend

`VITE_USE_MOCKS` controls where `src/api/*` gets data:

| Value | Behavior |
|---|---|
| `true` (default) | In-memory fake data from `mocks/mockStore.ts`. Any username/password signs in. |
| `false` | Runs **your** fetch code. Until it's written, the UI shows a "not implemented yet" banner. |

Set it in `docker-compose.yml` (then `docker compose up -d frontend` to restart) or in `frontend/.env.local`.

## Suggested build order

1. **Frontend `request<T>()` helper:** `frontend/src/api/http.ts`. Use `health.ts` as a reference.
2. **Wire up fetch calls against the dummy backend:** fill in `auth.ts`, `categories.ts`, `todos.ts`,
   set `VITE_USE_MOCKS=false`, and confirm you see the "(from backend)" dummy data.
   This proves the frontend half is done before you touch the database.
3. **Database:** add the EF Core packages (listed in `TodoApi.csproj`), uncomment `AppDbContext`,
   do TODO STEP 1 in `Program.cs`, then run `dotnet ef migrations add InitialCreate`.
4. **CategoriesController:** the simplest real CRUD.
5. **TodosController:** same pattern plus filtering and PATCH (read the `CategoryId` gotcha in `Dtos.cs`).
6. **Auth:** JWT (STEP 2 in `Program.cs`), `TokenService`, `AuthController`, then `[Authorize]`
   on the other controllers, and replace dummy user IDs with `CurrentUserId`.
7. **Delete `Controllers/DummyData.cs`.** The compiler will flag anything still using it.

## API contract

All routes are under `/api`. Everything except `health`, `auth/login`, and `auth/register` requires
`Authorization: Bearer <token>`.

| Method | Route | Body | Success | Errors |
|---|---|---|---|---|
| GET | `/health` | | 200 `{status, time}` | |
| POST | `/auth/register` | `{username, password}` | 201 `{token, user}` | 400, 409 |
| POST | `/auth/login` | `{username, password}` | 200 `{token, user}` | 401 |
| GET | `/auth/me` | | 200 `{id, username}` | 401 |
| GET | `/todos?completed=bool` | | 200 `Todo[]` | 401 |
| POST | `/todos` | `{title, categoryId}` | 201 `Todo` | 400 |
| PATCH | `/todos/{id}` | any of `{title, isCompleted, categoryId}` | 200 `Todo` | 400, 404 |
| DELETE | `/todos/{id}` | | 204 | 404 |
| GET | `/categories` | | 200 `Category[]` | 401 |
| POST | `/categories` | `{name}` | 201 `Category` | 400, 409 |
| DELETE | `/categories/{id}` | | 204 (its todos become uncategorized) | 404 |

Errors use ASP.NET Core's ProblemDetails JSON: `{ title, status, detail?, errors? }`.
