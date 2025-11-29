# ModularMonolith

A production-ready C# .NET 8 modular monolith architecture demonstrating best practices for organizing complex systems into independent, loosely-coupled modules while maintaining strong architectural boundaries.

## Table of Contents

- [Overview](#overview)
- [Module Structure](#module-structure)
- [Inter-Module Dependencies](#inter-module-dependencies)
- [Overall Architecture](#overall-architecture)
- [Docker Setup](#docker-setup)
- [Running the API](#running-the-api)
- [Architecture Tests](#architecture-tests)
- [Scripts](#scripts)

---

## Overview

This repository implements a modular monolith pattern where the application is decomposed into business-capability-aligned modules. Each module encapsulates its domain model, application logic, and infrastructure adapters while exposing only well-defined published interfaces to other modules.

The architecture is validated through comprehensive architecture tests using **ArchUnitNET**, ensuring modules follow strict dependency rules and maintain clear boundaries.

---

## Module Structure

The repository consists of three primary business modules:

### 1. **Greetings Module**
**Location:** [`Greetings/`](./Greetings/)

Handles greeting-related operations and integrates with the WeatherReporting module.

**Projects:**
- [`Greetings.DomainModel`](./Greetings/Greetings.DomainModel/) - Core business logic and domain entities
- [`Greetings.Application`](./Greetings/Greetings.Application/) - Application services and use cases
- [`Greetings.PublishedInterfaces`](./Greetings/Greetings.PublishedInterfaces/) - Public API exposed to other modules
- [`Greetings.Storage.Adapter`](./Greetings/Greetings.Storage.Adapter/) - Data persistence adapter
- [`Greetings.WeatherReportingApi.Adapter`](./Greetings/Greetings.WeatherReportingApi.Adapter/) - Integration with WeatherReporting module

### 2. **WeatherReporting Module**
**Location:** [`WeatherReporting/`](./WeatherReporting/)

Manages weather report generation, publishing, and retrieval from external sources.

**Projects:**
- [`WeatherReporting.DomainModel`](./WeatherReporting/WeatherReporting.DomainModel/) - Weather report entities and domain logic
- [`WeatherReporting.Application`](./WeatherReporting/WeatherReporting.Application/) - Application services for weather operations
- [`WeatherReporting.PublishedInterfaces`](./WeatherReporting/WeatherReporting.PublishedInterfaces/) - Public interfaces for weather reports
- [`WeatherReporting.ExternalSources.Adapter`](./WeatherReporting/WeatherReporting.ExternalSources.Adapter/) - External weather data integration
- [`WeatherReporting.Publishing.Adapter`](./WeatherReporting/WeatherReporting.Publishing.Adapter/) - Message publishing infrastructure
- [`WeatherReporting.Storage.Adapter`](./WeatherReporting/WeatherReporting.Storage.Adapter/) - Database persistence
- [`WeatherReporting.UnitTests`](./WeatherReporting/tests/WeatherReporting.UnitTests/) - Module-level unit tests

### 3. **WeatherModeling Module**
**Location:** [`WeatherModeling/`](./WeatherModeling/)

Models and processes weather data through asynchronous messaging.

**Projects:**
- [`WeatherModeling.DomainModel`](./WeatherModeling/WeatherModeling.DomainModel/) - Weather data models and transformations
- [`WeatherModeling.Application`](./WeatherModeling/WeatherModeling.Application/) - Application services for weather modeling
- [`WeatherModeling.Messaging.Adapter`](./WeatherModeling/WeatherModeling.Messaging.Adapter/) - Asynchronous message queue integration

### Shared Infrastructure
**Location:** [`SharedInfrastructure/`](./SharedInfrastructure/)

Common infrastructure utilities shared across all modules, including [`QueueFactory.cs`](./SharedInfrastructure/QueueFactory.cs) for message queue creation.

---

## Inter-Module Dependencies

The architecture enforces strict dependency rules to maintain module independence. See [`docs/module-dependency-rules.MD`](./docs/module-dependency-rules.MD) for comprehensive rules.

### Dependency Overview

**Greetings Module** depends on:
- `WeatherReporting.PublishedInterfaces` - To retrieve weather reports for greeting context

**WeatherReporting Module** depends on:
- `SharedInfrastructure` - For queue infrastructure

**WeatherModeling Module** depends on:
- `SharedInfrastructure` - For queue infrastructure

### Key Principles

1. **Published Interfaces Only**: Modules only reference the `PublishedInterfaces` project of other modules
2. **No Circular Dependencies**: The dependency graph is acyclic
3. **Domain Isolation**: `DomainModel` projects are not allowed to reference other modules
4. **Composition Root**: The API project ([`ModularMonolith.Api/Program.cs`](./ModularMonolith.Api/Program.cs)) is the only place where modules are composed together

---

## Overall Architecture

### Composition Root
**Location:** [`ModularMonolith.Api/Program.cs`](./ModularMonolith.Api/Program.cs)

The API serves as the composition root where:
- All modules are registered in the dependency injection container
- API endpoints are mapped from each module
- Configuration determines whether modules operate in standard or alternative modes

### Hexagonal Architecture Per Module

Each module follows the Hexagonal (Ports & Adapters) architecture pattern:

```
Module Structure:
├── DomainModel
│   └── Pure business logic (no external dependencies)
├── Application
│   └── Use cases and orchestration
├── PublishedInterfaces
│   └── External contracts (IProvideXxx interfaces)
└── Adapters (zero or more)
    ├── Storage - Data persistence
    ├── Messaging - Async communication
    └── External - Third-party integrations
```

### Solution Structure

```
ModularMonolith.sln
├── ModularMonolith.Api/           # ASP.NET Core entry point (port 1983)
├── Greetings/                     # Business Module
├── WeatherReporting/              # Business Module
├── WeatherModeling/               # Business Module
├── SharedInfrastructure/          # Shared cross-module utilities
├── ArchitectureTests/             # Automated architecture validation
└── ArchTestSourceGenerator/       # Source generator for test automation
```

---

## Docker Setup

The application uses Docker Compose to orchestrate multiple services including databases and the API.

**Location:** [`docker-compose.yaml`](./docker-compose.yaml)

### Services

| Service | Purpose | Database |
|---------|---------|----------|
| `greetings-module-db-server` | Greetings data persistence | SQL Server 2022 |
| `weather-db-server` | WeatherReporting & WeatherModeling data | MySQL |
| `modular-monolith-api` | ASP.NET Core application | - |
| `zookeeper` | Message broker coordination | - |

### Database Migrations

Flyway is used to manage database schema migrations:
- `greetings-flyway-migrations` - Applies Greetings DB migrations
- `weather-modeling-flyway-migrations` - Applies WeatherModeling DB migrations
- `weather-reporting-flyway-migrations` - Applies WeatherReporting DB migrations

NOTE: The `databases` folder in the repo root contains the database creation scripts 
for WeatherReporting and WeatherModeling modules because these modules share the MySql database server
but have their own MySql databases. 
The databases are provisioned when the database container bootstraps.
Since only one folder can be mounted to docker-initdb.d folder, both database creation scripts 
have to be in one folder. Hence the `databases` folder in the root! 

The migrations are located in the individual module folders as you'd expect!

### Key Configuration

- **API Port:** 1983
- **SQL Server Port:** 7978 (Greetings DB)
- **MySQL Port:** 3306 (Weather DBs)
- **Dockerfile:** [`ModularMonolith.Api/Dockerfile`](./ModularMonolith.Api/Dockerfile)

---

## Running the API

### Prerequisites

- .NET 8 SDK
- Docker and Docker Compose
- PowerShell (for scripts)

### Quick Start with Docker Compose

```bash
# Build and run all services
docker-compose up -d

# Check service health
docker-compose ps

# View logs
docker-compose logs -f modular-monolith-api

# Stop services
docker-compose down
```

### Local Development (Without Docker)

```bash
# Restore NuGet packages
dotnet restore

# Build the solution
dotnet build

# Run the API (requires local database setup)
dotnet run --project ModularMonolith.Api/ModularMonolith.Api.csproj
```

### API Access

- **Base URL:** `http://localhost:1983`
- **Swagger UI:** `http://localhost:1983/swagger/ui/index.html`

### Configuration

**Location:** [`ModularMonolith.Api/appsettings.json`](./ModularMonolith.Api/appsettings.json)

Key settings:
- `UseWeatherReportingApi` - Boolean flag to enable/disable WeatherReporting API adapter mode

---

## Architecture Tests

Comprehensive architecture tests ensure modules comply with dependency rules and maintain clear boundaries.

**Location:** [`ArchitectureTests/`](./ArchitectureTests/)

### Architecture Test Specifications

**Documentation:** [`docs/module-dependency-rules.MD`](./docs/module-dependency-rules.MD)

### Test Files

- [`WhenValidatingModuleBoundaries.cs`](./ArchitectureTests/WhenValidatingModuleBoundaries.cs) - Primary module dependency tests
- [`WhenValidatingModuleBoundaries2.cs`](./ArchitectureTests/WhenValidatingModuleBoundaries2.cs) - Additional validation scenarios

### Test Framework

Uses **ArchUnitNET** ([`ArchitectureTests.csproj`](./ArchitectureTests/ArchitectureTests.csproj)) to:
- Validate module projects only reference PublishedInterfaces of other modules
- Verify DomainModel projects have no external module dependencies
- Ensure Application projects follow proper layering
- Check Adapter projects follow naming conventions

### Running Tests

```bash
# Run all tests
dotnet test

# Run architecture tests specifically
dotnet test ArchitectureTests/ArchitectureTests.csproj

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test
dotnet test --filter "WhenValidatingModuleBoundaries"
```

### Source Generator for Architecture Tests

**Location:** [`ArchTestSourceGenerator/`](./ArchTestSourceGenerator/)

Planned incremental source generator to auto-generate architecture tests. Currently in Phase 1 (module discovery).

**Documentation:** [`docs/arch-test-source-generator-spec.MD`](./docs/arch-test-source-generator-spec.MD)

---

## Scripts

Automation scripts for managing the modular structure.

**Location:** [`scripts/`](./scripts/)

### 1. Create New Module
**File:** [`create-new-module.ps1`](./scripts/create-new-module.ps1)

Creates a new business module with complete scaffolding.

**Usage:**
```powershell
./scripts/create-new-module.ps1 -ModuleName "YourModuleName"
```

**What it does:**
- Creates module folder structure (`src/` and `tests/`)
- Generates three core projects:
  - `YourModuleName.DomainModel`
  - `YourModuleName.Application`
  - `YourModuleName.PublishedInterfaces`
- Creates a `DoNotDelete.cs` placeholder in each project (required for assembly loading)
- Adds projects to the solution
- Updates ArchitectureTests with project references

### 2. Modular Monolith Generator
**File:** [`mod-mono-gen.ps1`](./scripts/mod-mono-gen.ps1)

Generates a complete modular monolith solution from scratch (interactive).

**Usage:**
```powershell
./scripts/mod-mono-gen.ps1
```

**What it does:**
- Prompts for application name
- Creates solution and folder structure
- Initializes Git repository
- Creates modules interactively
- Adds standard packages (Moq, FluentAssertions)
- Creates ArchitectureTests project
- Builds and validates the solution

**Generation Specification:** [`docs/solution-generator-spec.md`](./docs/solution-generator-spec.md)

---

## Architecture Diagrams

Reference architecture diagrams are available in the [`docs/`](./docs/) folder:

- **Step 1:** Initial monolith structure
- **Step 2:** Async weather modeling module introduction
- **Step 3:** Split WeatherAPI adapter pattern
- **Type Dependencies:** WeatherReporting module dependency visualization

---

## Development Workflow

### Adding a New Feature

1. **Identify the module** where the feature belongs
2. **Add domain logic** to `[Module].DomainModel`
3. **Add application logic** to `[Module].Application`
4. **Expose public contracts** in `[Module].PublishedInterfaces` if needed
5. **Add adapters** for external integrations (Storage, Messaging, etc.)
6. **Update module registration** in `ModularMonolith.Api/Program.cs` if it's a new module
7. **Run architecture tests** to validate boundaries: `dotnet test`

### Inter-Module Communication

1. **Reference PublishedInterfaces** only: `dotnet add reference [OtherModule].PublishedInterfaces`
2. **Use dependency injection** to resolve dependencies at composition root
3. **Avoid circular dependencies** - validate with architecture tests

---

## Best Practices

1. **Keep modules independent** - Only share code through PublishedInterfaces
2. **Validate architecture** - Run tests before committing
3. **Use adapters for external concerns** - Database, messaging, APIs
4. **Isolate domain logic** - Keep DomainModel projects pure
5. **Document public interfaces** - Use XML comments on PublishedInterfaces
6. **Follow naming conventions** - `[Module].[Concern]` for all projects

---

## Technology Stack

- **.NET:** 8.0
- **Web Framework:** ASP.NET Core with Swagger/OpenAPI
- **Databases:** SQL Server (Greetings), MySQL (Weather)
- **Messaging:** In-memory queue with Zookeeper coordination
- **Migrations:** Flyway
- **Testing:** xUnit, Moq, ArchUnitNET
- **Architecture:** Hexagonal (Ports & Adapters)

---

