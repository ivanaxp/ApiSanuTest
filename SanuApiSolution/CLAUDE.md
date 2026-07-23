# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

SanuApi is an ASP.NET Core 8.0 REST API for a fitness gym management system. It manages customers, trainers, classes, memberships, goals, and health data. The database is PostgreSQL hosted on Render.com (dev) and Neon (production), accessed via Dapper ORM.

## Common Commands

```powershell
# Build the solution
dotnet build

# Run the API locally (Swagger UI at https://localhost:5001)
dotnet run --project SanuApi.Api

# Run all tests
dotnet test

# Run a specific test project
dotnet test SanuApi.Api.Tests
dotnet test SanuApi.Application.Tests

# Run a specific test class or method
dotnet test --filter "FullyQualifiedName~CustomerServiceTests"
dotnet test --filter "FullyQualifiedName~CustomerServiceTests.GetAll_ReturnsAll"
```

## Architecture

Clean Architecture with four layers. Dependencies flow inward only: Api → Application → Domain ← Infrastructure.

```
SanuApi.Api           # Controllers, Program.cs, DI configuration
SanuApi.Application   # Services, DTOs, service interfaces
SanuApi.Domain        # Entities (Dapper.Contrib attributes), repository interfaces
SanuApi.Infrastructure # Repository implementations, PostgresConnection
```

### Dependency Injection (Program.cs)
- `IDbConnection` → `NpgsqlConnection` (scoped, from connection string)
- Services and repositories registered as scoped
- CORS allows all origins
- Enums serialized as strings via `JsonStringEnumConverter`

### Data Access Pattern
Repositories use Dapper and Dapper.Contrib directly with raw SQL. Domain entities use `[Table]`, `[Key]`, and `[Write(false)]` attributes. No EF Core migrations — schema is managed externally in the hosted PostgreSQL databases.

### Testing Pattern
- NUnit + Moq across both test projects
- `SanuApi.Api.Tests`: controller-level tests mocking service interfaces
- `SanuApi.Application.Tests`: service-level tests mocking repository interfaces
- Test classes follow naming: `{Entity}{Layer}Tests.cs`

## Key Entities

- **Customer**: gym member with personal info, DNI, health data, classes, memberships, goals
- **Trainer**: staff who manage classes (`TrainerClasses` join entity)
- **Classes**: fitness classes with schedule
- **Membership**: subscription type linked to customers via `CustomerMembership`
- **Goal**: fitness goal linked to customers via `CustomerGoal`
- **HealthCustomer**: health metrics per customer
- **Absences**: attendance tracking

## Configuration

- `appsettings.json`: development PostgreSQL (Render.com)
- `appsettings.Production.json`: production PostgreSQL (Neon)
- Connection string key: `"DefaultConnection"`
- Swagger UI is served at the root path `/` via a redirect in `Program.cs`
