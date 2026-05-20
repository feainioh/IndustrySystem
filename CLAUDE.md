# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```bash
dotnet build IndustrySystem.sln
```

The main application entry point is the WPF project:
```
src/Presentation/IndustrySystem.Presentation.Wpf/IndustrySystem.Presentation.Wpf.csproj
```

There is also a standalone Motion Designer app:
```
src/Presentation/IndustrySystem.MotionDesigner/IndustrySystem.MotionDesigner.csproj
```

No test projects exist in this solution.

## Architecture

This is a **.NET 9 industrial lab automation system** following clean architecture with four layers:

### Layer dependency graph

```
Presentation (WPF, Prism + DryIoc)
    └── Application (Volo.Abp services + AutoMapper)
        ├── Application.Contracts (DTOs, service interfaces)
        └── Domain (entities, enums, repository interfaces)
            └── Domain.Shared (shared constants, no dependencies)
    └── Infrastructure
        ├── SqlSugar (MySQL ORM, DbContext, generic IRepository<T>)
        ├── Communication (Modbus TCP, HTTP, CAN/EtherCAT stubs, external sync channels)
        ├── Logging (NLog wrapper)
        └── MotionProgram (JSON-based motion program execution)
```

### Key frameworks & versions

- **.NET 9** (`net9.0` / `net9.0-windows7.0`), C# with nullable enabled
- **Prism 9 + DryIoc** — DI container and MVVM framework for WPF
- **Volo.Abp 8** — modularity (module system, DDD helpers, AutoMapper integration)
- **SqlSugarCore 5** — ORM targeting MySQL
- **ModernWpf 0.9** + **MaterialDesignThemes 5** — UI theming
- **NLog 5** — structured file logging (logs/ directory, rolling archives)
- **NModbus4** — Modbus TCP communication
- **Polly 8** — retry/resilience policies
- **Castle.Core 5** — dynamic proxy/interception

### Critical: Composition root

The ABP module classes (e.g., `IndustrySystemApplicationModule`, `IndustrySystemInfrastructureSqlSugarModule`) provide logical grouping, but the **actual DI registration happens in `App.xaml.cs:RegisterTypes()`**. This is where all services, repositories, ViewModels, Views, and dialogs are registered with Prism's DryIoc container. The ABP modules are initialized for framework services (`AbpApplicationFactory.Create<T>()`) but do NOT handle application-level registrations. Always check `App.xaml.cs` for the real wiring.

### Domain layer

- **Entities** live under `src/Domain/IndustrySystem.Domain/Entities/` organized by domain concept:
  - `Experiments/` — `Experiment`, `ExperimentTemplate`, `ExperimentGroup`, plus 10 parameter entity types (Reaction, RotaryEvaporation, Detection, Filtration, Drying, Quenching, Extraction, Sampling, Centrifugation, CustomDetection)
  - `Devices/` — `Device`, plus motor types (`CanMotor`, `EthercatMotor`)
  - `Materials/`, `Inventory/`, `Shelves/` — material master data, inventory records, shelf/slot/container configuration
  - `Users/`, `Roles/`, `Permissions/` — user management with role-based access control (RBAC via join tables `UserRole` and `RolePermission`)
- All entities use `Guid` primary keys; composite keys for join tables
- Repositories follow the generic `IRepository<T>` / `SqlSugarRepository<T>` pattern

### Application layer

- **Contracts project** (`Application.Contracts`) contains DTOs and service interfaces only — no implementation
- **Application project** contains service implementations with ABP `[RemoteService]` attributes
- AutoMapper profiles in `MappingProfile.cs` handle entity↔DTO mapping
- Key app services follow the naming convention `I<Entity>AppService` / `<Entity>AppService`

### Infrastructure layer

- **SqlSugar** — database access via `ISqlSugarClient` (SqlSugarScope with MySQL). `SqlSugarDatabaseInitializer` performs code-first table creation and seed data (admin user, roles, permissions, sample materials/inventory/shelves/containers)
- **Communication** — multiple transport protocols: `ModbusTcpClient` (NModbus4, production-ready), `SimpleHttpClient` (production-ready), `CanClient`/`EthercatClient`/`SimpleTcpClient` (stubs). Plus external sync channels (WebApi polling, raw TCP socket, SignalR Hub) with dedup, retry, conflict resolution, and dead-letter queue
- **Logging** — thin NLog abstraction via `IAppLogger`/`NLogAppLogger`
- **MotionProgram** — JSON-based motion program parsing and execution with `IMotionProgramJsonExecutor`

### Presentation layer

- Two separate WPF applications sharing the application/domain layers:
  - **IndustrySystem.Presentation.Wpf** — main lab management application (experiments, inventory, devices, user admin)
  - **IndustrySystem.MotionDesigner** — visual flowchart designer for creating motion control programs (drag-and-drop nodes with 40+ action types)
- Both use Prism with `PrismApplication` as the base class
- Views/ViewModels follow the Prism convention: `ViewModels/<Name>ViewModel.cs`, `Views/<Name>View.xaml`
- Modal dialogs use Prism's `IDialogService` with registered dialog ViewModels
- Navigation uses Prism `IRegionManager` with `ShellMainRegion` in the Shell window

### Authentication flow

1. App starts → `App.xaml.cs:OnStartup()` initializes DB, then shows `LoginView` as a modal dialog
2. `AuthService.SignInAsync()` validates credentials against the user DB (SHA256 password hash)
3. On success, `Shell` window is created with the authenticated user's roles/permissions in `AuthState`
4. Logout destroys the Shell window and re-shows the Login dialog (no app restart needed)
5. Single-instance enforcement via named `Mutex`

## External sync subsystem

Documented in `docs/external-sync-integration-plan.md`. Supports three protocols (WebApi polling, raw TCP socket with delimiter framing, SignalR Hub subscription). Each endpoint is independently configurable with retry policies, dedup windows, conflict resolution (ExternalWins/LocalWins/NewerWins), and dead-letter queue. Disabled by default (`ExternalSync.Enabled: false`). The sync pipeline is async-only with explicit deadlock avoidance (no `.Result`/`.Wait()`/`lock + await`).

## Localization & theme

- Chinese (zh-CN) is the default culture, set in `App.OnStartup()`
- String resources in `Resources/Strings.resx` and `Resources/Localization/` (zh-CN + en XAML files)
- Two themes: `Classic` and `LiquidGlass` (defined in `Resources/Styles/`), toggled via `AppVisualThemeService`

## Copilot instruction

When WPF input box text is not displaying, check control height and whether the MaterialDesign style's available text display area is sufficient.
