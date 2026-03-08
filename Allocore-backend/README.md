# Allocore Backend

**Allocore** is a cost allocation platform that helps companies manage providers, services, employees, cost centers, and projects in a unified system.

## Technology Stack

- **.NET 8** Web API with Clean Architecture
- **PostgreSQL** database (configured in US002)
- **MediatR** for CQRS pattern
- **FluentValidation** for request validation
- **Entity Framework Core** for data access
- **Swagger/OpenAPI** for API documentation
- **API Versioning** (v1)

## Project Structure

```
Allocore/
├── Allocore.API/            # HTTP endpoints, controllers, middleware
├── Allocore.Application/    # Use cases, CQRS handlers, business logic
├── Allocore.Domain/         # Entities, value objects, domain rules
├── Allocore.Infrastructure/ # Database, external services, repositories
└── Docs/                    # Architecture docs, user stories
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- PostgreSQL (for US002+)

### Running the API

```bash
dotnet build
dotnet run --project Allocore.API
```

The API will start at `http://localhost:5103`

### Available Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/` | GET | Redirects to Swagger UI |
| `/swagger` | GET | API documentation |
| `/health` | GET | Health check endpoint |
| `/api/v1/ping` | GET | Test endpoint (returns pong + timestamp) |

## Architecture

This project follows **Clean Architecture** principles with CQRS pattern:

- **Domain Layer**: Pure business logic, no external dependencies
- **Application Layer**: Use cases orchestrating domain logic via MediatR
- **Infrastructure Layer**: Database, external services implementation
- **API Layer**: HTTP interface, request/response handling

### Key Features

- **CQRS with MediatR**: Commands and queries separated with pipeline behaviors
- **Validation Pipeline**: FluentValidation integrated via MediatR behavior
- **Global Exception Handling**: Centralized error handling with JSON responses
- **CORS Configuration**: Ready for frontend integration (localhost:3000)
- **Health Checks**: Built-in health monitoring endpoint

See `Docs/Architecture.md` for detailed documentation.

## Documentation

- [Architecture](Docs/Architecture.md) - System architecture overview
- [Development History](Docs/DevelopmentHistory.md) - Version changelog
- [User Stories](Docs/UserStories.md) - Feature specifications
- [Release Notes](Docs/ReleaseNotes/) - Version release notes
