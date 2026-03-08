# Allocore Project Structure Reference

This document provides a comprehensive overview of the Allocore project structure, architecture, and conventions. It is designed to help LLMs understand the project organization when writing new user stories and implementing features.

## Architecture Overview

Allocore follows the **Clean Architecture** pattern with **CQRS** (Command Query Responsibility Segregation) using MediatR:

1. **Domain Layer** - Core business entities and logic
2. **Application Layer** - Use cases, interfaces, DTOs, and CQRS handlers
3. **Infrastructure Layer** - Implementation of interfaces, data access, external services
4. **API Layer** - Controllers, middleware, and API endpoints

## System Architecture Diagram

```mermaid
graph TB
    %% API Layer
    subgraph "API Layer (Allocore.API)"
        PC[PingController]
        AC[AuthController - US002]
        CC[CompaniesController - US003]
        UC[UsersController - US002]
        MW[Middleware]
        AUTH[Authentication - US002]
    end

    %% Application Layer
    subgraph "Application Layer (Allocore.Application)"
        subgraph "Features"
            PF[Ping Feature]
            AF[Auth Features - US002]
            CF[Company Features - US003]
            UF[User Features - US002]
        end
        
        subgraph "Abstractions"
            IRR[IReadRepository]
            IWR[IWriteRepository]
            IUR[IUserRepository - US002]
            ICR[ICompanyRepository - US003]
        end
        
        subgraph "Behaviors"
            VB[ValidationBehavior]
        end
        
        subgraph "DI"
            DI[DependencyInjection]
        end
    end

    %% Infrastructure Layer
    subgraph "Infrastructure Layer (Allocore.Infrastructure)"
        subgraph "Persistence"
            IMR[InMemoryRepository]
            DBX[ApplicationDbContext - US002]
            MIG[EF Core Migrations - US002]
        end
        
        subgraph "Repositories - US002+"
            URR[UserRepository]
            CRR[CompanyRepository]
        end
        
        subgraph "DI"
            IDI[DependencyInjection]
        end
    end

    %% Domain Layer
    subgraph "Domain Layer (Allocore.Domain)"
        subgraph "Common"
            ENT[Entity Base Class]
            RES[Result Pattern]
        end
        
        subgraph "Entities"
            UE[User - US002]
            RE[Role Enum - US002]
            CE[Company - US003]
            UCE[UserCompany - US003]
        end
    end

    %% External Systems
    subgraph "External Systems"
        POSTGRES[(PostgreSQL - US002)]
        JWT[JWT Authentication - US002]
        SWAGGER[Swagger/OpenAPI]
    end

    %% Connections - API to Application
    PC --> PF
    AC --> AF
    CC --> CF
    UC --> UF

    %% Connections - Application to Infrastructure
    PF --> VB
    VB --> IMR
    AF --> URR
    CF --> CRR

    %% Connections - Infrastructure to Domain
    IMR --> ENT
    URR --> UE
    CRR --> CE

    %% Connections - Infrastructure to External
    DBX --> POSTGRES
    AUTH --> JWT
    MW --> SWAGGER

    %% Styling
    classDef apiLayer fill:#e1f5fe
    classDef appLayer fill:#f3e5f5
    classDef infraLayer fill:#e8f5e8
    classDef domainLayer fill:#fff3e0
    classDef externalLayer fill:#ffebee
    classDef futureLayer fill:#f5f5f5,stroke-dasharray: 5 5

    class PC,AC,CC,UC,MW,AUTH apiLayer
    class PF,AF,CF,UF,IRR,IWR,IUR,ICR,VB,DI appLayer
    class IMR,DBX,MIG,URR,CRR,IDI infraLayer
    class ENT,RES,UE,RE,CE,UCE domainLayer
    class POSTGRES,JWT,SWAGGER externalLayer
```

## Project Structure

```
Allocore/
├── Allocore.Domain/                 # Domain Layer
│   ├── Common/                      # Base classes
│   │   ├── Entity.cs                # Base entity with Id, CreatedAt, UpdatedAt
│   │   └── Result.cs                # Result pattern for operation outcomes
│   ├── Entities/                    # Business entities
│   │   └── Users/                   # User-related entities
│   │       ├── User.cs              # User entity (placeholder for US002)
│   │       └── Role.cs              # Role enum (placeholder for US002)
│   ├── README.md                    # Layer documentation
│   └── Allocore.Domain.csproj
│
├── Allocore.Application/            # Application Layer
│   ├── Abstractions/                # Interfaces
│   │   └── Persistence/             # Repository interfaces
│   │       ├── IReadRepository.cs   # Generic read operations
│   │       └── IWriteRepository.cs  # Generic write operations
│   ├── Behaviors/                   # MediatR pipeline behaviors
│   │   └── ValidationBehavior.cs    # FluentValidation integration
│   ├── Features/                    # CQRS features organized by domain
│   │   └── Ping/                    # Ping feature
│   │       ├── PingQuery.cs         # Query + Response records
│   │       └── PingQueryHandler.cs  # Query handler
│   ├── DependencyInjection.cs       # Application layer DI registration
│   └── Allocore.Application.csproj
│
├── Allocore.Infrastructure/         # Infrastructure Layer
│   ├── Persistence/                 # Data access
│   │   └── InMemory/                # In-memory implementations
│   │       └── InMemoryRepository.cs # Generic in-memory repository
│   ├── DependencyInjection.cs       # Infrastructure layer DI registration
│   └── Allocore.Infrastructure.csproj
│
├── Allocore.API/                    # API Layer
│   ├── Controllers/                 # API controllers
│   │   └── v1/                      # Version 1 controllers
│   │       └── PingController.cs    # Health/ping endpoint
│   ├── Properties/                  # Launch settings
│   ├── Program.cs                   # Application startup and configuration
│   ├── appsettings.json             # Production configuration
│   ├── appsettings.Development.json # Development configuration
│   └── Allocore.API.csproj
│
├── Docs/                            # Documentation
│   ├── Architecture.md              # This file - Project structure reference
│   ├── DevelopmentHistory.md        # Version changelog
│   ├── Project_Summary.md           # Brand and product overview
│   ├── Roadmap.md                   # Product roadmap
│   ├── ReleaseNotes/                # Version release notes
│   │   └── v0.1.0-backend-scaffolding.md
│   └── User Story/                  # User story documentation
│       ├── US001-Backend-Scaffolding.md
│       ├── US002-JWT-Authentication.md
│       └── US003-Company-MultiTenant.md
│
├── Allocore.sln                     # Solution file
├── .gitignore                       # Git ignore rules
└── README.md                        # Project README
```

## Key Components

### Domain Layer

The Domain layer contains the core business entities and logic. It has **no dependencies** on other layers.

#### Common Classes

- **Entity** - Base class for all entities with `Id`, `CreatedAt`, `UpdatedAt`, and equality comparison
- **Result** - Result pattern for operation outcomes with `IsSuccess`, `Error`, and generic `Result<T>`

#### Entities (Current & Planned)

| Entity | Status | Description |
|--------|--------|-------------|
| User | Placeholder (US002) | User account with authentication |
| Role | Placeholder (US002) | User roles enum (User, Admin) |
| Company | Planned (US003) | Multi-tenant company entity |
| UserCompany | Planned (US003) | User-Company relationship |

### Application Layer

The Application layer contains use cases, interfaces, and CQRS handlers. It depends **only on the Domain layer**.

#### Abstractions

- **IReadRepository<T>** - Generic read operations (`GetByIdAsync`, `GetAllAsync`)
- **IWriteRepository<T>** - Generic write operations (`AddAsync`, `UpdateAsync`, `DeleteAsync`)

#### Behaviors

- **ValidationBehavior<TRequest, TResponse>** - MediatR pipeline behavior that runs FluentValidation before handlers

#### Features

Features are organized by domain area using CQRS pattern:

```
Features/
└── {FeatureName}/
    ├── {FeatureName}Query.cs        # Query record + Response
    ├── {FeatureName}QueryHandler.cs # Query handler
    ├── {FeatureName}Command.cs      # Command record (for mutations)
    └── {FeatureName}CommandHandler.cs
```

**Current Features:**
- **Ping** - Simple health check query returning `{ message: "pong", timestamp: "..." }`

### Infrastructure Layer

The Infrastructure layer implements interfaces from the Application layer. It depends on **Application and Domain layers**.

#### Persistence

- **InMemoryRepository<T>** - Generic in-memory repository using `ConcurrentDictionary`
- **ApplicationDbContext** - EF Core DbContext (planned for US002)

### API Layer

The API layer handles HTTP concerns. It depends on **Application and Infrastructure layers**.

#### Controllers

- Controllers use API versioning (`/api/v{version}/[controller]`)
- All controllers inject `IMediator` for CQRS
- Controllers are organized by version (`Controllers/v1/`)

**Current Controllers:**
- **PingController** - `GET /api/v1/ping` - Returns pong response

#### Configuration (Program.cs)

- MediatR + FluentValidation registration
- API Versioning (v1 default)
- Swagger/OpenAPI documentation
- Health checks (`/health`)
- CORS for frontend (`localhost:3000`)
- Global exception handling

## Conventions and Patterns

### CQRS Pattern

```
HTTP Request → Controller → MediatR.Send() → Handler → Response
                                  ↓
                          ValidationBehavior
                                  ↓
                            Validator (if exists)
```

- **Queries**: Read operations, no side effects, return data
- **Commands**: Write operations, may have side effects, return Result
- **Handlers**: Process requests, orchestrate domain logic

### Repository Pattern

- Interfaces defined in `Application/Abstractions/Persistence/`
- Implementations in `Infrastructure/Persistence/`
- Generic `IReadRepository<T>` and `IWriteRepository<T>` for common operations
- Specific repositories for complex queries (e.g., `IUserRepository`)

### Validation

- FluentValidation validators in `Application/Features/{Feature}/`
- Naming convention: `{Request}Validator.cs`
- Auto-registered via `AddValidatorsFromAssembly()`
- Validation errors throw `ValidationException` (caught by global handler)

### Error Handling

- Global exception handler in `Program.cs`
- `ValidationException` → HTTP 400 with error details
- Unhandled exceptions → HTTP 500 with generic message
- Result pattern for domain operation outcomes

### API Versioning

- URL-based versioning: `/api/v{version}/[controller]`
- Default version: 1.0
- Version reported in response headers

## Database

- **Current**: In-memory repositories (US001)
- **Planned**: PostgreSQL with EF Core (US002+)
- Code-first approach with migrations
- Connection string in `appsettings.json`

## Authentication & Authorization (Planned - US002)

- JWT Bearer authentication
- Refresh token rotation
- Role-based authorization (Admin, User)
- Policies: `CanManageUsers`, `RequireVerifiedEmail`

## Multi-Tenancy (Planned - US003)

- Company entity with tenant isolation
- UserCompany relationship (many-to-many)
- Role within company (Owner, Manager, Viewer)
- All business data scoped to CompanyId

## Adding New Features

### 1. Domain Layer
- Add entities in `Entities/{Feature}/`
- Extend `Entity` base class
- Define relationships and business rules

### 2. Application Layer
- Create feature folder in `Features/{Feature}/`
- Add Query/Command records with `IRequest<TResponse>`
- Add Handler classes implementing `IRequestHandler<TRequest, TResponse>`
- Add Validators if needed
- Define repository interfaces in `Abstractions/Persistence/`

### 3. Infrastructure Layer
- Implement repository interfaces in `Persistence/`
- Add entity configurations for EF Core
- Create migrations if database changes

### 4. API Layer
- Add controller in `Controllers/v1/`
- Use `[ApiVersion("1.0")]` attribute
- Inject `IMediator` and use `Send()` for CQRS
- Apply authorization attributes as needed

## Key Dependencies

| Package | Version | Project | Purpose |
|---------|---------|---------|---------|
| MediatR | 13.x | Application | CQRS pattern |
| FluentValidation | 12.x | Application | Request validation |
| FluentValidation.DependencyInjectionExtensions | 11.x | Application | DI integration |
| Microsoft.EntityFrameworkCore | 8.x | Infrastructure | ORM (planned) |
| Microsoft.EntityFrameworkCore.Design | 8.x | Infrastructure | Migrations |
| Swashbuckle.AspNetCore | 10.x | API | Swagger/OpenAPI |
| Asp.Versioning.Mvc | 8.x | API | API versioning |
| Asp.Versioning.Mvc.ApiExplorer | 8.x | API | Version discovery |

## Documentation Standards

- User stories in `Docs/User Story/US*.md`
- Development history in `Docs/DevelopmentHistory.md`
- Release notes in `Docs/ReleaseNotes/`
- Architecture reference in `Docs/Architecture.md` (this file)

## Upcoming Features

| Version | User Story | Description |
|---------|------------|-------------|
| v0.2 | US002 | JWT Authentication & User Management |
| v0.3 | US003 | Company & UserCompany (Multi-Tenant Core) |

---

This reference provides LLMs with a comprehensive understanding of the Allocore project structure and conventions, enabling accurate and consistent implementation of new features.
