# Architecture: DynamicLambdaGenerator

## Overview

This solution implements a **Layered Architecture** with a generic CRUD framework capable of generating dynamic LINQ expressions at runtime. Adding a new entity only requires an empty controller, an empty repository, two DTOs, and an AutoMapper profile — all business logic, filtering, sorting, and pagination is handled by the generic base classes.

---

## Solution Structure

```
DynamicLambdaGenerator/
├── Common.Core/        # Generic framework: base CRUD, dynamic filters, DI contracts
├── Common.Domain/      # EF Core entities, DbContext, migrations
├── Common.Business/    # Concrete services and repositories
├── Common.WebApi/      # HTTP API: controllers, DTOs, mappings, middleware
└── Common.Tests/       # xUnit unit tests with Moq + AutoFixture
```

---

## Layer Descriptions

### 1. Common.Core — The Generic Framework

The heart of the solution. Provides base classes and interfaces reusable for any entity.

#### Key namespaces

| Namespace | Responsibility |
|---|---|
| `Generic.Controllers` | `GenericControllerBase`, `CustomBaseController` |
| `Generic.Services` | `BaseService<TEntity, TKey>` |
| `Generic.Repository` | `BaseRepository<TEntity>` |
| `Generic.DynamicQueryFilter` | Expression builder classes |
| `Generic.QueryLanguage` | `QueryableHelper` (dynamic sorting) |
| `Data.Context` | `BaseAppDbContext` (EF Core + Identity) |
| `Data.Interfaces` | `IBaseService`, `IRepository`, `IEntity`, `IPagination`, etc. |
| `Data.Wrappers` | `PaginatedResult<T>` |
| `CustomExceptions` | `CustomException`, `NoDbRecordException`, `CrudOperationException` |

#### Controller hierarchy

```
ControllerBase (ASP.NET Core)
  └── CustomBaseController<T, TKey>        ← handles HTTP responses, calls IBaseService
        └── GenericControllerBase<TEntity, TRequestDto, TResponseDto, TQueryFilter, TValidator>
              └── ClientController        ← empty: just DI wiring
```

`GenericControllerBase` exposes these endpoints automatically:

| HTTP Method | Route | Action |
|---|---|---|
| `GET` | `/{id}` | Get by PK |
| `POST` | `/query` | Filtered/sorted/paginated query |
| `POST` | `/` | Create |
| `PUT` | `/` | Full update |
| `PATCH` | `/` | Partial update |
| `DELETE` | `/{id}` | Delete |

All endpoints require JWT authorization (`[Authorize]`).

#### Service hierarchy

```
IBaseService<TEntity, TKey>
  └── BaseService<TEntity, TKey>       ← CRUD + filtering + pagination + mapping
        └── ClientService              ← override if needed
```

`BaseService` responsibilities:
- `AddAsync` / `AddRangeAsync`
- `DeleteAsync` (throws `NoDbRecordException` if not found)
- `UpdateAsync` / `UpdateAsync<TDto>` (maps DTO → entity then saves)
- `Get<TQueryFilter>` → returns `IQueryable<TEntity>` via `DynamicExpression`
- `Get<TDto, TQueryFilter>` → paginated, sorted, projected to DTO
- `GetByPKAsync` / `GetByPKAsync<TDto>`
- `MapDtoToEntity` / `MapEntityToDto` (via AutoMapper)
- `ValidateDto` (via FluentValidation through `IValidationService`)

#### Repository

```
IRepository<TEntity>
  └── BaseRepository<TEntity>          ← EF Core operations on DbSet
        └── ClientRepository           ← empty: just DI wiring
```

Exposes: `Get(int id)`, `GetAsync`, `GetAll`, `Get(IDynamicExpression)`, `Add`, `AddAsync`, `AddRangeAsync`, `Delete`, `Update`, `Any`, `AnyAsync`, `GetHealth`.

`IUnitOfWork` is implemented by `BaseAppDbContext` — `UnitOfWork.SaveChangesAsync()` is called after every write operation.

#### DynamicQueryFilter — Expression Builder

The core "magic" of the project. Builds `Expression<Func<T, bool>>` predicates from QueryFilter DTO properties at runtime.

```
DynamicExpression<T, TQueryFilter>         ← base, implements AND/OR operators
  ├── TrueDynamicExpression<T, TQueryFilter>      ← x => true (identity/AND neutral)
  ├── ListDynamicExpression<T, TQueryFilter>       ← Id IN (1,2,3) via OR chain
  ├── RangeDynamicExpression<T, TQueryFilter>      ← >=, <=, >, < on numeric/date
  └── DateRangeExpression<T, TQueryFilter>         ← from/to date range (legacy)
```

**`SetPredicate(TQueryFilter filter)`** execution flow:
1. Reflects only properties declared directly on `TQueryFilter` (not base classes).
2. Validates that all properties are nullable (throws `InvalidOperationException` if not).
3. Skips null properties (no filter applied for them).
4. For each non-null property:
   - **Nested object** → recurses into sub-properties.
   - **`List<T>` with `list` prefix** → `ListDynamicExpression` (OR chain of equalities).
   - **`string` with `contains` prefix** → `string.Contains()` method call.
   - **`string` (other)** → equality (`==`).
   - **Numeric / `DateTime`** → `RangeDynamicExpression` (detects prefix: `greaterThan`, `lessThan`, `greaterThanOrEqual`, `lessThanOrEqual`).
5. All conditions are combined with `&&` (AND) using the `&` operator override.

**Prefix matching** (configured via `DynamicFiltersConfiguration` / `appsettings.json`):

| Config key | Default value | Effect |
|---|---|---|
| `GreaterThanOrEqualFieldName` | `greaterThanOrEqual` | `entity.Field >= value` |
| `LessThanOrEqualFieldName` | `lessThanOrEqual` | `entity.Field <= value` |
| `GreaterThanFieldName` | `greaterThan` | `entity.Field > value` |
| `LessThanFieldName` | `lessThan` | `entity.Field < value` |
| `ContainsFieldName` | `contains` | `entity.Field.Contains(value)` |
| `ListFieldName` | `list` | `entity.Field IN (list)` |

Prefix matching is **case-insensitive**. After stripping the prefix, the remaining name is used to resolve the entity property via reflection.

#### Dynamic Sorting — `QueryableHelper<T>`

Parses the `SortingFields` string from `IPagination`. Format: `fieldName [asc|desc]` separated by commas. Applies `.OrderBy` / `.ThenBy` dynamically via `Expression.PropertyOrField`.

#### Pagination — `BaseRequest` / `IPagination`

`BaseRequest` is the base for all query filter DTOs. Properties:
- `Page` (default: 1, minimum: 1)
- `PageSize` (default: 50, minimum: 1)
- `SortingFields` — comma-separated sort descriptors
- `Disabled` — bypasses pagination

Pagination is always applied via `.Skip().Take()` unless `Disabled = true`.

---

### 2. Common.Domain — Entity Model

Contains EF Core entities and the concrete `AppDbContext`.

#### Entities

| Entity | Key fields |
|---|---|
| `Client` | Id, Name, Email, PhoneNumber, BirthDate, Sessions |
| `Photographer` | Id, Name, Email, PhoneNumber |
| `Location` | Id, Name, Address |
| `Session` | Id, Date, Notes, ClientId, PhotographerId, LocationId |

All entities implement `IEntity` (exposes `int Id`).

#### `AppDbContext` → `BaseAppDbContext` → `IdentityDbContext`

- Inherits from `IdentityDbContext<ApplicationUserBase, ApplicationRole, string>` for built-in user/role management.
- `ApplyEntityConfigurations(ModelBuilder)` dynamically scans the domain assembly and applies all `IEntityTypeConfiguration<T>` implementations via reflection — avoids manual registration.
- `DesignTimeAppDbContextFactory` bootstraps EF CLI tools and handles seed data on startup.
- `SeedManager` seeds initial data.

---

### 3. Common.Business — Application Services

Thin layer of concrete implementations. Each class is mostly empty because the base classes do all the work.

```
ClientService   : BaseService<Client, int>
LocationService : BaseService<Location, int>
PhotographerService : BaseService<Photographer, int>
SessionService  : BaseService<Session, int>

ClientRepository    : BaseRepository<Client>
LocationRepository  : BaseRepository<Location>
PhotographerRepository : BaseRepository<Photographer>
SessionRepository   : BaseRepository<Session>
```

This layer exists to:
- Allow entities to override specific CRUD behavior when generic logic is insufficient.
- Register concrete types in the DI container without coupling `Common.Core` to domain types.

---

### 4. Common.WebApi — HTTP API

#### Startup

`Program.cs` initializes the application and wires up services via extension methods in `StartupConfigExtensions`:

| Extension | Registers |
|---|---|
| `SetupConfiguration` | Environment-based appsettings + env vars |
| `AddConfigurations` | `SecurityHeaders`, `DynamicFiltersConfiguration` (IOptions) |
| `RegisterServices` | All services and repositories (Scoped) |
| `AddPersistence` | EF Core DbContext + Identity |
| `AddAuthentication` | JWT Bearer |
| `AddSwagger` | Swagger with XML docs |
| `SetCorsPolicy` | CORS from config |
| `AddSecurityResponseHeadersMiddleware` | Security headers middleware |

#### Controllers

Empty controllers — only constructor DI:

```
ClientController         : GenericControllerBase<Client, ClientRequestDto, ClientResponseDto, ClientQueryFilter, ClientRequestDtoValidator>
LocationController       : GenericControllerBase<Location, ...>
PhotographerController   : GenericControllerBase<Photographer, ...>
SessionController        : GenericControllerBase<Session, ...>
```

#### DTOs and Models (per entity)

| File | Purpose |
|---|---|
| `*RequestDto` | Input DTO (create/update); implements `IEntity` |
| `*ResponseDto` | Output DTO; implements `IEntity` |
| `*QueryFilter` | Inherits `*DynamicFieldsQueryFilter`; direct-match filter fields |
| `*DynamicFieldsQueryFilter` | Inherits `BaseRequest + IDynamicQueryFilter`; prefixed dynamic fields |
| `*RequestDtoValidator` | FluentValidation validator for the request DTO |

#### Mappings

`MappingProfile` defines AutoMapper maps for all entities:
```csharp
CreateMap<Client, ClientRequestDto>().ReverseMap();
CreateMap<Client, ClientResponseDto>().ReverseMap();
// ...
```

AutoMapper's `ProjectTo<TDto>()` is used during query execution to project directly to DTO inside a single database query (no double-fetch).

---

### 5. Common.Tests — Unit Tests

Framework: **xUnit + Moq + AutoFixture + MockQueryable + FluentAssertions**.

#### Infrastructure

| File | Purpose |
|---|---|
| `TestFixture` | Factory for mapper, mock entities, `MockDynamicFilter<T>` helper |
| `TestDemoContext` | In-memory `AppDbContext` for integration-style tests |
| `AutoFixtureCustomization` | Registers IOptions, IConfiguration, IMapper, InMemory DbContext into the AutoFixture container |
| `AutoMoq` attribute | Combines AutoFixture + AutoMoq to auto-generate test parameters |

#### Test categories and coverage

| Category | Classes | Tests per class | Scenarios covered |
|---|---|---|---|
| Controller tests | 4 | 8 | Get/GetAll/Create/Update/Patch/Delete, 404 cases |
| Service tests | 4 | ~22 | CRUD, dynamic filters (equal, contains, range, list, sort, pagination) |
| Repository tests | 4 | 10 | Get/Add/Delete/Update, async variants, dynamic filter |

Total: **154 tests, 0 failures**.

---

## Data Flow: Filtered Query

```
HTTP POST /api/v1/client/query  { filter: { ... } }
  │
  ▼
GenericControllerBase.Query(TQueryFilter filter)
  │
  ▼
CustomBaseController.Get<TResponseDto, TQueryFilter>(filter)
  │
  ▼
BaseService.Get<TDto, TQueryFilter>(filter)
  ├── DynamicExpression<TEntity, TQueryFilter>(filter, config)
  │     └── Reflects filter properties → builds Expression<Func<T,bool>>
  ├── BaseRepository.Get(IDynamicExpression) → IQueryable<TEntity>.Where(predicate)
  ├── query.CountAsync()           ← total count BEFORE pagination
  ├── QueryableHelper.ApplySorting(query, filter)
  ├── ApplyPagination(filter, query)  → .Skip().Take()
  └── mapper.ProjectTo<TDto>(query).ToListAsync()  ← single SQL query
  │
  ▼
PaginatedResult<TResponseDto>  →  Response<TResponseDto> (JSON)
```

---

## Key Design Patterns

| Pattern | Where used |
|---|---|
| **Layered Architecture** | Overall project structure |
| **Generic Repository** | `BaseRepository<TEntity>` |
| **Unit of Work** | `IUnitOfWork` / `BaseAppDbContext.SaveChangesAsync()` |
| **Specification (adapted)** | `DynamicExpression<T, TQueryFilter>` with `&` / `\|` operators |
| **DTO / Mapper** | AutoMapper `ProjectTo<>` in service layer |
| **Options pattern** | `IOptions<DynamicFiltersConfiguration>` injected into services |
| **Fluent Validation** | `AbstractValidator<TDto>` per request DTO |
| **Global Exception Handler** | `IExceptionHandler` middleware in WebApi |
| **Decorator / Filter (Swagger)** | `ProducesGenericResponseTypeAttribute` for typed Swagger docs |
