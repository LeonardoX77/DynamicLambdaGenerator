# Dynamic Lambda Expressions, CRUD operations and IQueryable sorting/paging

## Project architecture
This project implements a design pattern known as the **Layered Architecture Pattern**. This pattern that implements controllers, services, repositories, data, model, and logic in different layers. The Layered Architecture Pattern organizes an application into logical layers, where each layer has a specific responsibility and communicates with other layers in an orderly manner.

Here is how the layers are structured in the project:

1. **Common.WebApi**:
   - **Responsibility**: Manages the user interface and user interaction.
   - **Typical Components**: Controllers, mappings, and DTOs.

2. **Common.Business**:
   - **Responsibility**: Coordinates application logic and business rules.
   - **Typical Components**: Application logic, service implementations, and generic repositories from the Core layer.

3. **Common.Core**:
   - **Responsibility**: Provides generic data context, **base** services and repositories that **implement typical CRUD operations, filtering, sorting and pagination.
   - **Typical Components**: Services, repositories, data context, classes for dynamic lambda expressions generation.

4. **Common.Domain**:
   - **Responsibility**: Contains the ORM models, database context implementation, and migrations.
   - **Typical Components**: ORM models, database context implementation, and migrations.

5. **Common.Tests**:
   - **Responsibility**: Ensures code quality and functionality through testing.
   - **Typical Components**: Unit tests.

This pattern promotes the separation of concerns, facilitates maintenance and scalability of the system, and improves code reuse.
<br><br>

## THE GOAL: Dynamic Lambda Expressions, CRUD operations and IQueryable sorting/paging

Now, imagine you have to add more than 20 new entities to your application with controller and service CRUD operations, **search by different field names**, sorting and paging data, let's take for example one of them\: **Clients**. To achieve this, whith this architecture, you must only implement:

**An empty Controller**: GenericControllerBase will implement all CRUD operations (post, put, patch, get, delete, filter (GetAll filtered), sorting and pagination) **for you!**
```c#
    [Route("api/v1/[controller]")]
    public class ClientController : GenericControllerBase<Client, ClientRequestDto, ClientResponseDto, ClientQueryFilter, ClientRequestDtoValidator>
    {
        public ClientController(
            ILogger<ClientController> logger,
            IBaseService<Client, int> clientService) : base(logger, clientService)
        {
        }
    }
```

**A Repository**: Same thing, you have all necessary logic in `BaseRepository`
```c#
    public class ClientRepository : BaseRepository<Client>
    {
        public ClientRepository(AppDbContext ctx) : base(ctx)
        {
        }
    }
```
**A DTO to send back to client**
```c#
    public class ClientResponseDto : IEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        ...
    }
```
**An Automapper mapping class**
```c#
    public class ClientMappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Client, ClientRequestDto>().ReverseMap();
            CreateMap<Client, ClientResponseDto>().ReverseMap();
        }
    }
```
<br>

### Filtering

You decide which entity fields you want to filter by, just declaring the same entity field names in the DTO your controller is expecting:
```c#
    public class ClientQueryFilter : BaseRequest
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public DateTime? BirthDate { get; set; }
    }
```

### Advanced filtering
If you need advanced filtering, such as something like this:
```c#
   DbContext.Clients.Where(c => c.Id >= 10 && c.Id <= 30);
   DbContext.Clients.Where(c => listIDs.Contains(c.Id));
   DbContext.Clients.Where(c => c.DateStart >= DateTime.Now && c.DateEnd <= DateTime.Now.AddMinutes(5));
   DbContext.Clients.Where(c => c.Summary.Contains("some text"));
```

You can just declare a Queryfilter class and declare some fields with predefined prefixes (Min, Max, GreaterThan, LessThan, List, Contains) followed by the entity field name you want to filter by:
```c#

    public class ClientQueryFilter : ClientDynamicFieldsQueryFilter
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        ...
    }

    public class ClientDynamicFieldsQueryFilter : BaseRequest, IDynamicQueryFilter
    {
        // List of IDs
        public List<int>? ListId { get; set; }

        // >=, <= numbers
        public int? LessThanOrEqualId { get; set; }
        public int? GreaterThanOrEqualId { get; set; }
        public int? GreaterThanId { get; set; }
        public int? LessThanId { get; set; }

        // Contains name, List names
        public string? ContainsName { get; set; }
        public string[]? ListName { get; set; }

        // >=, <= dates
        public DateTime? GreaterThanOrEqualBirthDate { get; set; }
        public DateTime? LessThanOrEqualBirthDate { get; set; }
        public DateTime? GreaterThanBirthDate { get; set; }
        public DateTime? LessThanBirthDate { get; set; }
    }
```

#### Filter names Configuration
You can decide which name prefixes to use (GreaterThan, LessThan, List, Contains) in `appsettings.json` `DynamicFiltersConfiguration` section:

```json
  "DynamicFiltersConfiguration": {
    "LessThanOrEqualFieldName": "lessThanOrEqual",
    "GreaterThanOrEqualFieldName": "greaterThanOrEqual",
    "GreaterThanFieldName": "greaterThan",
    "LessThanFieldName": "lessThan",
    "ContainsFieldName": "contains",
    "ListFieldName": "list"
  }
```


## The Magic
The main class that generates the magic is `DynamicExpression` in mthod `SetPredicate(TQueryFilter filter)`
You can have a look at unit tests starting with `Get_ByDynamicFilter...` to se how it works :)

## Features

1. **Centralization of Common Logic**:
   - BaseService and BaseRepository encapsulates and centralizes common logic for CRUD operations (Create, Read, Update, Delete). This avoids code duplication in each specific domain service. If a case arises that cannot be implemented generically, any functionality can be inherited and overridden.

2. **Decoupling of DTOs and Entities**:
   - Effective decoupling of DTOs and entities is achieved, ensuring that the presentation and business layers remain independent and manageable.

3. **Use of Generic Types and Type Abstraction**:
   - By using generic types, BaseService and BaseRepository can handle any type of entity without needing to know specific details about the entity. This makes BaseService and BaseRepository extremely flexible and reusable.

4. **Automatic Mapping with AutoMapper**:
   - Automatic mapping with AutoMapper through the projection (ProjectTo()) of data directly into DTOs in a single atomic operation improves efficiency and performance when handling large volumes of data.

5. **Generation of Dynamic Lambda Expressions**:
   - The generation of dynamic Lambda expressions in IQueryable directly from DTOs allows:
     - Use of special fields (Max, Min, Count, From, To, Contains, List) that automate the generation of simple queries, reducing the estimated development time.
     - Automation of sorting and pagination of records.

6. **Decouplig Data layer from Model/Domain**:
   - Using a Base DbContext to manage Entity migrations and IEntityTypeConfiguration for metter EF Code First management

6. **Simple UI**:
   - This project implements Swagger, so you can test all application functionality.

## Requirements
**SQL Server**
This software use SQL Server database, so to work properly you need to install and change the ConnectionString from appsettings.json file

**Code Coverage reports**
In order to generate code coverage reports properly with coverlete tool you need to install reportgenerator separately:

```bash
   dotnet tool install -g dotnet-reportgenerator-globaltool
```

## SET-UP
- Open Solution and build it.
- Run database migrations, this will create the database and needed data:
```bash
dotnet ef database update --project "./Common.Domain/Common.Domain.csproj" --startup-project "./Common.WebApi/Common.WebApi.csproj"
```

## Example DEMO:
### Photographic Session Management:
This example ilustrates how some entities can be managed (CRUD), filtered, sorted and paginated with query filters and transform/return DTOs automatically.

**Entities**: Client, Photographer, Session, Location

**Description**: Allows photographers to manage their sessions with clients. Users can register information about the sessions (date, time, location, type of session), client data, and assigned photographers. Notes or comments about each session can also be added.

## Pending tasks or things that could be extended:
You can extend very easy the implementation of new filters extending from `DynamicExpression<T, TQueryFilter>` (i.e. see `ListDynamicExpression` or `RangeDynamicExpression` implementations)
A feature that would be nice to have is for example filtering aggregate queries (grouping) and harnessing the existing filtering, sorting and paging functionality

## DISCLAIMER
All the source code in this project is 100% my own work, and it is the result of several years of experience in software development. A very important part of this involves not noly coding but reading documentation of all the frameworks and technologies that are part of this project, as well as many hours of effort and dedication that have helped me refine and choose the best programming techniques.
For any questions regarding this source code, you can ask me or create a Pull Request :).

Thank you and Happy Coding! :)