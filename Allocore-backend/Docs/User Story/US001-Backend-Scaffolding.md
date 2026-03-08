# US001 – Allocore .NET Backend Scaffolding

## Description

**As** an architect/engineer responsible for Allocore,  
**I need** to generate a base solution in .NET 8 with Clean Architecture + CQRS,  
**So that** I can evolve the business modules (auth, companies, costs) in an organized and maintainable way.

---

## Step 1: Solution Structure & Project Setup ✅

- [x] Create solution folder `Allocore/` with the following structure:
  ```
  Allocore/
  ├── Docs/
  ├── Allocore.API/
  ├── Allocore.Application/
  ├── Allocore.Domain/
  ├── Allocore.Infrastructure/
  ├── Allocore.sln
  ├── .gitignore
  └── README.md
  ```
- [x] Initialize `Allocore.sln` solution file
- [x] Create `Allocore.API` project (ASP.NET Core Web API, net8.0)
- [x] Create `Allocore.Application` project (Class Library, net8.0)
- [x] Create `Allocore.Domain` project (Class Library, net8.0)
- [x] Create `Allocore.Infrastructure` project (Class Library, net8.0)
- [x] Configure all projects with `<Nullable>enable</Nullable>` and implicit usings
- [x] Add project references:
  - `Allocore.API` → `Allocore.Application`, `Allocore.Infrastructure`
  - `Allocore.Application` → `Allocore.Domain`
  - `Allocore.Infrastructure` → `Allocore.Application`, `Allocore.Domain`

---

## Step 2: NuGet Packages Installation ✅

- [x] **Allocore.Domain**: No external packages (pure domain)
- [x] **Allocore.Application**:
  - `MediatR` (latest stable)
  - `FluentValidation` (latest stable)
  - `FluentValidation.DependencyInjectionExtensions`
- [x] **Allocore.Infrastructure**:
  - `Microsoft.EntityFrameworkCore` (8.x)
  - `Microsoft.EntityFrameworkCore.Design` (8.x)
- [x] **Allocore.API**:
  - `Swashbuckle.AspNetCore` (latest stable)
  - `Asp.Versioning.Mvc` (latest stable)
  - `Asp.Versioning.Mvc.ApiExplorer` (latest stable)

---

## Step 3: Domain Layer (`Allocore.Domain`) ✅

- [x] Create `Common/Entity.cs`:
  ```csharp
  namespace Allocore.Domain.Common;
  
  public abstract class Entity
  {
      public Guid Id { get; protected set; } = Guid.NewGuid();
      public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
      public DateTime? UpdatedAt { get; protected set; }
  }
  ```
- [x] Create `Common/Result.cs`:
  ```csharp
  namespace Allocore.Domain.Common;
  
  public class Result<T>
  {
      public bool IsSuccess { get; }
      public T? Value { get; }
      public string? Error { get; }
      
      private Result(bool isSuccess, T? value, string? error)
      {
          IsSuccess = isSuccess;
          Value = value;
          Error = error;
      }
      
      public static Result<T> Success(T value) => new(true, value, null);
      public static Result<T> Failure(string error) => new(false, default, error);
  }
  
  public class Result
  {
      public bool IsSuccess { get; }
      public string? Error { get; }
      
      protected Result(bool isSuccess, string? error)
      {
          IsSuccess = isSuccess;
          Error = error;
      }
      
      public static Result Success() => new(true, null);
      public static Result Failure(string error) => new(false, error);
  }
  ```
- [x] Create `Entities/Users/User.cs` (placeholder for US002):
  - `User.cs` (empty class with TODO comment for US002)
  - `Role.cs` (empty enum with TODO comment for US002)
- [x] Create `README.md` in Domain folder explaining the layer purpose

---

## Step 4: Application Layer (`Allocore.Application`) ✅

- [x] Create `Abstractions/Persistence/IReadRepository.cs`:
  ```csharp
  namespace Allocore.Application.Abstractions.Persistence;
  
  public interface IReadRepository<T> where T : class
  {
      Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
      Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
  }
  ```
- [x] Create `Abstractions/Persistence/IWriteRepository.cs`:
  ```csharp
  namespace Allocore.Application.Abstractions.Persistence;
  
  public interface IWriteRepository<T> where T : class
  {
      Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
      Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
      Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
  }
  ```
- [x] Create `Behaviors/ValidationBehavior.cs`:
  ```csharp
  namespace Allocore.Application.Behaviors;
  
  using FluentValidation;
  using MediatR;
  
  public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
      where TRequest : IRequest<TResponse>
  {
      private readonly IEnumerable<IValidator<TRequest>> _validators;
      
      public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
      {
          _validators = validators;
      }
      
      public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
      {
          if (!_validators.Any())
              return await next();
          
          var context = new ValidationContext<TRequest>(request);
          var validationResults = await Task.WhenAll(
              _validators.Select(v => v.ValidateAsync(context, cancellationToken)));
          
          var failures = validationResults
              .SelectMany(r => r.Errors)
              .Where(f => f != null)
              .ToList();
          
          if (failures.Count != 0)
              throw new ValidationException(failures);
          
          return await next();
      }
  }
  ```
- [x] Create `Features/Ping/PingQuery.cs`:
  ```csharp
  namespace Allocore.Application.Features.Ping;
  
  using MediatR;
  
  public record PingQuery : IRequest<PingResponse>;
  
  public record PingResponse(string Message, DateTime Timestamp);
  ```
- [x] Create `Features/Ping/PingQueryHandler.cs`:
  ```csharp
  namespace Allocore.Application.Features.Ping;
  
  using MediatR;
  
  public class PingQueryHandler : IRequestHandler<PingQuery, PingResponse>
  {
      public Task<PingResponse> Handle(PingQuery request, CancellationToken cancellationToken)
      {
          return Task.FromResult(new PingResponse("pong", DateTime.UtcNow));
      }
  }
  ```
- [x] Create `DependencyInjection.cs`:
  ```csharp
  namespace Allocore.Application;
  
  using FluentValidation;
  using MediatR;
  using Microsoft.Extensions.DependencyInjection;
  using Allocore.Application.Behaviors;
  
  public static class DependencyInjection
  {
      public static IServiceCollection AddApplication(this IServiceCollection services)
      {
          var assembly = typeof(DependencyInjection).Assembly;
          
          services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
          services.AddValidatorsFromAssembly(assembly);
          services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
          
          return services;
      }
  }
  ```

---

## Step 5: Infrastructure Layer (`Allocore.Infrastructure`) ✅

- [x] Create `Persistence/InMemory/InMemoryRepository.cs`:
  ```csharp
  namespace Allocore.Infrastructure.Persistence.InMemory;
  
  using Allocore.Application.Abstractions.Persistence;
  using Allocore.Domain.Common;
  using System.Collections.Concurrent;
  
  public class InMemoryRepository<T> : IReadRepository<T>, IWriteRepository<T> where T : Entity
  {
      private readonly ConcurrentDictionary<Guid, T> _store = new();
      
      public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
      {
          _store.TryGetValue(id, out var entity);
          return Task.FromResult(entity);
      }
      
      public Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
      {
          return Task.FromResult<IEnumerable<T>>(_store.Values.ToList());
      }
      
      public Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
      {
          _store[entity.Id] = entity;
          return Task.FromResult(entity);
      }
      
      public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
      {
          _store[entity.Id] = entity;
          return Task.CompletedTask;
      }
      
      public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
      {
          _store.TryRemove(entity.Id, out _);
          return Task.CompletedTask;
      }
  }
  ```
- [x] Create `DependencyInjection.cs`:
  ```csharp
  namespace Allocore.Infrastructure;
  
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  
  public static class DependencyInjection
  {
      public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
      {
          // In-memory repositories will be registered here
          // PostgreSQL + EF Core will be added in US002
          return services;
      }
  }
  ```

---

## Step 6: API Layer (`Allocore.API`) ✅

- [x] Create `Controllers/v1/PingController.cs`:
  ```csharp
  namespace Allocore.API.Controllers.v1;
  
  using Asp.Versioning;
  using MediatR;
  using Microsoft.AspNetCore.Mvc;
  using Allocore.Application.Features.Ping;
  
  [ApiController]
  [ApiVersion("1.0")]
  [Route("api/v{version:apiVersion}/[controller]")]
  public class PingController : ControllerBase
  {
      private readonly IMediator _mediator;
      
      public PingController(IMediator mediator)
      {
          _mediator = mediator;
      }
      
      [HttpGet]
      public async Task<ActionResult<PingResponse>> Get(CancellationToken cancellationToken)
      {
          var response = await _mediator.Send(new PingQuery(), cancellationToken);
          return Ok(response);
      }
  }
  ```
- [x] Configure `Program.cs`:
  ```csharp
  using Allocore.Application;
  using Allocore.Infrastructure;
  using FluentValidation;
  using Microsoft.AspNetCore.Diagnostics;
  
  var builder = WebApplication.CreateBuilder(args);
  
  // Add services
  builder.Services.AddApplication();
  builder.Services.AddInfrastructure(builder.Configuration);
  
  builder.Services.AddControllers();
  builder.Services.AddEndpointsApiExplorer();
  builder.Services.AddSwaggerGen(c =>
  {
      c.SwaggerDoc("v1", new() { Title = "Allocore API", Version = "v1" });
  });
  
  builder.Services.AddApiVersioning(options =>
  {
      options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
      options.AssumeDefaultVersionWhenUnspecified = true;
      options.ReportApiVersions = true;
  }).AddApiExplorer(options =>
  {
      options.GroupNameFormat = "'v'VVV";
      options.SubstituteApiVersionInUrl = true;
  });
  
  builder.Services.AddHealthChecks();
  
  // CORS for frontend integration
  builder.Services.AddCors(options =>
  {
      options.AddPolicy("AllowFrontend", policy =>
      {
          policy.WithOrigins("http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod();
      });
  });
  
  var app = builder.Build();
  
  // Global exception handling
  app.UseExceptionHandler(errorApp =>
  {
      errorApp.Run(async context =>
      {
          var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
          context.Response.ContentType = "application/json";
          
          if (exception is ValidationException validationException)
          {
              context.Response.StatusCode = 400;
              var errors = validationException.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
              await context.Response.WriteAsJsonAsync(new { errors });
              return;
          }
          
          context.Response.StatusCode = 500;
          await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred." });
      });
  });
  
  // Configure pipeline
  if (app.Environment.IsDevelopment())
  {
      app.UseSwagger();
      app.UseSwaggerUI();
  }
  
  app.UseHttpsRedirection();
  app.UseCors("AllowFrontend");
  app.UseAuthorization();
  app.MapControllers();
  app.MapHealthChecks("/health");
  app.MapGet("/", () => Results.Redirect("/swagger"));
  
  app.Run();
  ```
- [x] Configure `appsettings.json`:
  ```json
  {
    "Logging": {
      "LogLevel": {
        "Default": "Information",
        "Microsoft.AspNetCore": "Warning"
      }
    },
    "AllowedHosts": "*"
  }
  ```
- [x] Configure `appsettings.Development.json`:
  ```json
  {
    "Logging": {
      "LogLevel": {
        "Default": "Debug",
        "Microsoft.AspNetCore": "Information"
      }
    }
  }
  ```

---

## Step 7: Documentation (`Docs/`) ✅

- [x] Create/Update `Docs/Architecture.md`:
  - Explain Clean Architecture layers
  - Describe CQRS pattern with MediatR
  - Note that Auth and multi-tenancy will be added in US002 and US003
- [x] Create/Update `Docs/DevelopmentHistory.md`:
  - Add entry for v0.1 – Backend Scaffolding (US001)
- [x] Create/Update `Docs/UserStories.md`:
  - Reference US001, US002, US003 from EPIC 001

---

## Step 8: Build & Verification ✅

- [x] Run `dotnet build` and ensure no errors
- [x] Run `dotnet run --project Allocore.API`
- [x] Verify endpoints:
  - `GET /` → Redirects to Swagger ✅
  - `GET /swagger` → Swagger UI loads ✅
  - `GET /health` → Returns "Healthy" ✅
  - `GET /api/v1/ping` → Returns `{ "message": "pong", "timestamp": "..." }` ✅

---

## Technical Details

### Dependencies

| Project | Package | Version |
|---------|---------|---------|
| Allocore.Application | MediatR | 12.x |
| Allocore.Application | FluentValidation | 11.x |
| Allocore.Application | FluentValidation.DependencyInjectionExtensions | 11.x |
| Allocore.Infrastructure | Microsoft.EntityFrameworkCore | 8.x |
| Allocore.Infrastructure | Microsoft.EntityFrameworkCore.Design | 8.x |
| Allocore.API | Swashbuckle.AspNetCore | 6.x |
| Allocore.API | Asp.Versioning.Mvc | 8.x |
| Allocore.API | Asp.Versioning.Mvc.ApiExplorer | 8.x |

> **Note:** `MediatR` 12.x includes DI extensions — no separate package needed.

### Project Structure

```
Allocore/
├── Docs/
│   ├── Architecture.md
│   ├── DevelopmentHistory.md
│   └── UserStories.md
├── Allocore.API/
│   ├── Controllers/
│   │   └── v1/
│   │       └── PingController.cs
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── Allocore.API.csproj
├── Allocore.Application/
│   ├── Abstractions/
│   │   └── Persistence/
│   │       ├── IReadRepository.cs
│   │       └── IWriteRepository.cs
│   ├── Behaviors/
│   │   └── ValidationBehavior.cs
│   ├── Features/
│   │   └── Ping/
│   │       ├── PingQuery.cs
│   │       └── PingQueryHandler.cs
│   ├── DependencyInjection.cs
│   └── Allocore.Application.csproj
├── Allocore.Domain/
│   ├── Common/
│   │   ├── Entity.cs
│   │   └── Result.cs
│   ├── Users/
│   │   ├── User.cs (placeholder)
│   │   └── Role.cs (placeholder)
│   ├── README.md
│   └── Allocore.Domain.csproj
├── Allocore.Infrastructure/
│   ├── Persistence/
│   │   └── InMemory/
│   │       └── InMemoryRepository.cs
│   ├── DependencyInjection.cs
│   └── Allocore.Infrastructure.csproj
├── Allocore.sln
├── .gitignore
└── README.md
```

### Database

- **No database changes in this story** (in-memory repositories only)
- PostgreSQL + EF Core will be configured in US002

### API Contract

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/` | GET | No | Redirects to Swagger UI |
| `/swagger` | GET | No | Swagger documentation |
| `/health` | GET | No | Health check endpoint |
| `/api/v1/ping` | GET | No | Returns pong message with timestamp |

**Response: `GET /api/v1/ping`**
```json
{
  "message": "pong",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### Authentication/Authorization

- **No authentication in this story** (will be added in US002)
- All endpoints are public

---

## Acceptance Criteria

- [ ] Solution compiles with `dotnet build` without errors
- [ ] `dotnet run --project Allocore.API` starts the server
- [ ] Swagger UI accessible at `/swagger`
- [ ] Health check returns 200 OK at `/health`
- [ ] Ping endpoint returns valid JSON at `/api/v1/ping`
- [ ] MediatR pipeline with ValidationBehavior is configured
- [ ] Clean Architecture folder structure is in place
- [ ] Documentation files exist in `Docs/` folder
