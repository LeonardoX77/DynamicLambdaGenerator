# Gaps, Bugs & Improvement Plan

This document catalogs known bugs, missing test coverage, and architectural improvement opportunities identified through static code analysis and test execution.

---

## 1. Confirmed Bugs

### BUG-01: `string[]` array type in QueryFilter is silently ignored

**File:** [Common.WebApi/Application/Models/Client/ClientDynamicFieldsQueryFilter.cs](../Common.WebApi/Application/Models/Client/ClientDynamicFieldsQueryFilter.cs)

```csharp
public string[]? ListName { get; set; }  // bug: array, not List<T>
```

**Root cause:** `DynamicExpression.BuildSpecification()` only handles `List<T>` generics:
```csharp
if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>)
    && propertyName.ToLower().StartsWith(_dynamicFiltersConfiguration.ListFieldName))
```
A `string[]` is not a `List<T>`, so it falls through all conditions and returns `TrueDynamicExpression` (i.e., no filter is applied). The field is **silently ignored** — no error, no filtering.

**Fix:** Change the type to `List<string>?`, or add array handling in `BuildSpecification`:
```csharp
// Add check for arrays
if (propertyType.IsArray && propertyName.ToLower().StartsWith(_dynamicFiltersConfiguration.ListFieldName))
{
    var list = ((IEnumerable)value).Cast<object>().ToList();
    if (list.Count > 0)
        return new ListDynamicExpression<T, TQueryFilter>(propertyNameWithoutPrefix, list);
}
```

---

### BUG-02: `ListName` as `string?` in Location/Photographer filters does equality instead of list lookup

**Files:** `LocationDynamicFieldsQueryFilter`, `PhotographerDynamicFieldsQueryFilter`

```csharp
public string? ListName { get; set; }  // intended as list, but type is string
```

**Root cause:** When `ListName` is `string?`, `BuildSpecification` routes it to string equality (`entity.Name == value`) rather than a list-contains filter, because the `List<T>` check fails and the `contains` prefix check also fails (prefix is `list`, not `contains`). The result is correct syntax but wrong semantics — it filters by a single name equality, not by a list.

**Fix:** Change the type to `List<string>?` to align with `ClientDynamicFieldsQueryFilter.ListId`.

---

### BUG-03: `DateRangeExpression.SetPredicate()` calls `.Compile()` inside the lambda — not EF-translatable

**File:** [Common.Core/Generic/DynamicQueryFilter/DynamicExpressions/DateRangeExpression.cs](../Common.Core/Generic/DynamicQueryFilter/DynamicExpressions/DateRangeExpression.cs)

```csharp
return entity => fromExpression.Compile()(entity) && toExpression.Compile()(entity);
```

**Problem:** `Compile()` produces a delegate, not an expression tree. EF Core cannot translate delegate calls to SQL, so using `DateRangeExpression` with a real database would throw a runtime exception. Additionally, `Compile()` is called on every predicate evaluation, which is expensive.

**Note:** `RangeDynamicExpression` handles dates correctly with expression trees. `DateRangeExpression` appears to be an older, now-superseded implementation.

**Fix:** Either remove `DateRangeExpression` (use `RangeDynamicExpression` instead) or rewrite `SetPredicate()` using expression trees as `RangeDynamicExpression` does.

---

### BUG-04: `AutoFixtureCustomization._options` is never assigned (CS0649 warning)

**File:** [Common.Tests/Infrastructure/AutoMoq/AutoFixtureCustomization.cs](../Common.Tests/Infrastructure/AutoMoq/AutoFixtureCustomization.cs)

```csharp
private readonly IOptions<DynamicFiltersConfiguration> _options;  // never assigned
```

`_options` is passed to `TestDemoContext(dbContextOptions, _options)` where it will be `null`. The `BaseAppDbContext` constructor calls `options.Value` during `ApplyEntityConfigurations`, which would throw `NullReferenceException` when an in-memory context is used with model scanning.

The tests currently pass because the in-memory test database uses `EnableNullChecks(false)` and `ApplyEntityConfigurations` fails silently. But this is fragile.

**Fix:** Assign `_options` from `GetDynamicFiltersConfiguration()` in the constructor, or use `Options.Create(new DynamicFiltersConfiguration())` as default.

---

### BUG-05: `ListDynamicExpression` builds OR chains instead of using `Contains` — poor scalability

**File:** [Common.Core/Generic/DynamicQueryFilter/DynamicExpressions/ListDynamicExpression.cs](../Common.Core/Generic/DynamicQueryFilter/DynamicExpressions/ListDynamicExpression.cs)

```csharp
_ids.ForEach(id =>
{
    var newSpecification = new DynamicExpression<...>(Expression.Equal(...));
    specification = specification == null ? newSpecification : specification | newSpecification;
});
```

For a list of N items this creates an expression tree with N nested OR nodes. For large lists (hundreds of IDs), this can cause stack overflow or very slow EF query generation compared to `Contains()` (translated to `IN (...)` in SQL).

**Fix:** Use `Expression.Call` with `Enumerable.Contains` or `List<T>.Contains` to generate a single `IN (...)` predicate:
```csharp
var containsMethod = typeof(List<object>).GetMethod("Contains");
var listExpr = Expression.Constant(_ids);
var containsExpr = Expression.Call(listExpr, containsMethod, propertyExpression);
return new DynamicExpression<T, TQueryFilter>(Expression.Lambda<Func<T, bool>>(containsExpr, parameter));
```

---

## 2. Missing Test Coverage

### Controller layer

| Scenario | Affected controllers |
|---|---|
| `Query` returns 204 No Content when result is empty | All 4 |
| `Create` fails validation → 400 Bad Request | All 4 |
| `Delete` throws `NoDbRecordException` → 404 Not Found | All 4 |
| `Patch` vs `Update` — verify `CrudAction.UPDATE_PATCH` vs `CrudAction.UPDATE` | All 4 |

### Service layer

| Scenario | Notes |
|---|---|
| `AddRangeAsync` success path | Not tested |
| `GetAllAsync()` (no filter) | Not tested |
| `GetAllAsync<TDto>()` (projected) | Not tested |
| `ValidateDto` throws when validation fails | Not tested |
| `UpdateAsync(TEntity)` when entity does not exist | Partially covered via `UpdateAsync<TDto>` |

### Repository layer

| Scenario | Notes |
|---|---|
| `GetAll()` with data (only empty case tested) | Should test with populated `DbSet` |
| `GetHealth()` → returns `true` when DB is reachable | Not tested |
| `AnyAsync(expression)` | Not tested |
| `AddRangeAsync()` | Not tested |
| Dynamic filter returning multiple results | Only single-result test exists |

### DynamicExpression / filter engine

| Scenario | Notes |
|---|---|
| Non-nullable property in QueryFilter → `InvalidOperationException` | Not tested |
| Nested property filtering (e.g., `Session.Client.Name`) | Not tested |
| OR operator (`\|`) between two expressions | Not tested directly |
| `TrueDynamicExpression` as identity element | No direct test |
| `DateRangeExpression` | No test at all |
| `string[]` ListName (BUG-01) → expected behavior | No regression test |
| Empty `List<T>` in ListId → returns all | Not explicitly unit tested |
| `ContainsName` with empty string | Edge case not tested |

### Infrastructure / Cross-cutting

| Scenario | Notes |
|---|---|
| `UserServiceBase` | No tests |
| `ValidationService` | No tests |
| `BaseAppDbContext.ApplyEntityConfigurations` | No tests |
| Global exception handler middleware | No tests |
| Security headers middleware | No tests |
| JWT authentication pipeline | No tests |

---

## 3. Architecture and Design Improvements

### ARCH-01: No OR-predicate support in dynamic filters

All filter conditions are combined with `&&` (AND). There is no way to express OR conditions (e.g., "clients with name = 'A' OR email = 'B'") through the QueryFilter API.

**Suggestion:** Introduce an `OrGroup` concept or allow `[OrFilter]` attribute on properties to combine specific fields with OR instead of AND.

---

### ARCH-02: `IBaseService` violates Interface Segregation Principle

The interface exposes 15+ methods including internal mapping utilities (`MapDtoToEntity`, `MapEntityToDto`, `MapEntityToDto<IQueryable>`). Consumers (controllers) only need the read/write data methods; the mapping methods should be internal to `BaseService`.

**Suggestion:** Split into `IReadService<TEntity, TKey>`, `IWriteService<TEntity, TKey>`, and keep mapping as protected members of `BaseService`.

---

### ARCH-03: Key type is hard-coded to `int` in the controller chain

`CustomBaseController<T, TKey>` accepts a generic `TKey`, but `GenericControllerBase` fixes it to `int`:
```csharp
public class GenericControllerBase<...> : CustomBaseController<TEntity, int>
```
Routes (`/{id}`) and `IBaseService<TEntity, int>` are also fixed. This prevents using `Guid`, `string`, or composite keys.

**Suggestion:** Propagate `TKey` as a generic parameter through `GenericControllerBase`.

---

### ARCH-04: `DateRangeExpression` is superseded but not removed

`RangeDynamicExpression` fully covers date filtering with proper expression trees. `DateRangeExpression` is broken for EF use (see BUG-03) and is never referenced in the production filter pipeline.

**Suggestion:** Remove `DateRangeExpression` or mark it `[Obsolete]`.

---

### ARCH-05: No `NOT` predicate support

The `&` (AND) and `|` (OR) operators are implemented on `DynamicExpression`, but there is no `!` (NOT) operator. Filtering "all clients NOT in this list" requires a workaround.

**Suggestion:** Add:
```csharp
public static DynamicExpression<T, TQueryFilter> operator !(DynamicExpression<T, TQueryFilter> expr)
{
    return new DynamicExpression<T, TQueryFilter>(
        Expression.Lambda<Func<T, bool>>(
            Expression.Not(expr.Predicate().Body),
            expr.Predicate().Parameters));
}
```

---

### ARCH-06: No aggregate/grouping filter support (documented as pending)

The README acknowledges this as a future feature. Dynamic GroupBy with filtering would require either:
- A separate `GroupByExpression` class, or
- Exposing raw `IQueryable<T>` to the caller for further composition.

---

### ARCH-07: Migration management tightly coupled to startup

`DesignTimeAppDbContextFactory.SeedData(app.Services)` is called in `Program.cs`. Running migrations at startup can cause issues in multi-instance deployments (race conditions on migration apply).

**Suggestion:** Move migration apply to a dedicated CLI step or a health-check-gated startup task, not inline in `app.Run()` path.

---

### ARCH-08: `appsettings.json` contains secrets/connection string in plain text

The `ConnectionString` is referenced from `appsettings.json`. In production, secrets must be managed via:
- Azure Key Vault / AWS Secrets Manager
- Environment variables (already supported by `SetupConfiguration`)
- .NET User Secrets (for local development)

The `appsettings.Development.json` should never contain real credentials.

---

## 4. Minor Code Quality Items

| Item | Location | Suggestion |
|---|---|---|
| `BaseRequest.IsValid()` always returns `true` | `BaseRequest` | The `Page` and `PageSize` getters always return non-null defaults; `HasValue` is always true. Either remove `IsValid()` or document it is a no-op when using `BaseRequest`. |
| `Serilog.Extensions.Hosting` version mismatch | `Common.WebApi.csproj` | Pinned to `9.0.2` but `10.0.0` is resolved (nuget warning). Update pinned version to `10.0.0`. |
| No logging in service layer | `BaseService` | Only the controller layer has `ILogger`. Add structured logging for CRUD operations in `BaseService`. |
| `_options` field in `BaseAppDbContext` is redundant | `BaseAppDbContext` | `_options` is only used in `ApplyEntityConfigurations`. It could be a local variable in that method, injected via construction. |
| `CheckNonNullableProperties` throws `InvalidOperationException` | `DynamicExpression` | This is a developer error detected at runtime (first query execution). Consider adding a design-time analyzer or at least a clear error message linking to documentation. |
