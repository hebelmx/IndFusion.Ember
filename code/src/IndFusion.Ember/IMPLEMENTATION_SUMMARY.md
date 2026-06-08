# IndFusion.Ember — Implementation Summary

## Overview

`IndFusion.Ember` is an independent, packable NuGet library providing a
transport-agnostic real-time communication abstraction following Clean
(Hexagonal) Architecture and Railway-Oriented Programming. It was extracted from
the former `ExxerCube.Prisma.SignalR.Abstractions` and is currently implemented
over SignalR, with the abstraction layer designed for additional transports.

- **Package id:** `IndFusion.Ember`
- **Version:** 0.2.0
- **Target framework:** .NET 10.0
- **License:** MIT

## Solution Structure

```
IndFusion.Ember.sln
└── code/src/
    ├── IndFusion.Ember/                       # Main package project (Microsoft.NET.Sdk)
    │   ├── Abstractions/
    │   │   ├── Hubs/                  { IExxerHub.cs, ExxerHub.cs }
    │   │   ├── Health/               { IServiceHealth.cs, ServiceHealth.cs }
    │   │   └── Dashboards/           { IDashboard.cs, Dashboard.cs }
    │   ├── Infrastructure/
    │   │   ├── Connection/           { ConnectionState.cs, ReconnectionStrategy.cs }
    │   │   └── Messaging/            { MessageBatcher.cs, MessageThrottler.cs }
    │   ├── Presentation/
    │   │   └── Blazor/               { DashboardComponent.cs,
    │   │                               ConnectionStateIndicator.cs / .razor }
    │   ├── Extensions/               { ServiceCollectionExtensions.cs,
    │   │                               MudBlazorExtensions.cs }
    │   └── GlobalUsings.cs
    └── IndFusion.Ember.Tests/                  # Test project (xUnit v3 on MTP)
```

## Core Abstractions

### 1. `ExxerHub<T>`
- Generic SignalR hub abstraction with type-safe messaging
- Railway-Oriented Programming (`Result`/`Result<T>` via `IndQuestResults`)
- Cancellation token support
- Connection lifecycle management

### 2. `ServiceHealth<T>`
- Real-time health monitoring with status-change events
- Type-safe health data
- Integrates with `Microsoft.Extensions.Diagnostics.HealthChecks`

### 3. `Dashboard<T>`
- Real-time dashboard abstraction
- SignalR connection management
- Message batching and throttling
- Connection-state tracking with a reconnection strategy

## Infrastructure Components

### Connection Management
- `ConnectionState` enum (Disconnected, Connecting, Connected, Reconnecting, Failed)
- `ReconnectionStrategy` with exponential backoff

### Messaging
- `MessageBatcher<T>` — batches messages to reduce transport traffic
- `MessageThrottler<T>` — throttles messages to prevent UI overload

## Blazor Integration

- `DashboardComponent<T>` — base component for real-time dashboards
- `ConnectionStateIndicator` — MudBlazor connection-status indicator

> **Note:** the project uses `Microsoft.NET.Sdk` (not `Microsoft.NET.Sdk.Razor`),
> so `.razor` files are not compiled into components; only the `.cs` partials
> compile. Converting to the Razor SDK is a flagged, deferred decision.

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| `IndQuestResults` | 1.1.0 | `Result<T>` pattern |
| `Microsoft.AspNetCore.SignalR.Client` | 10.0.8 | SignalR client transport |
| `MudBlazor` | 9.5.0 | Blazor UI components |
| `Microsoft.AspNetCore.App` (FrameworkReference) | — | ASP.NET Core / SignalR server |

## NuGet Package Configuration

- `IsPackable=true`, `GenerateDocumentationFile=true`, `TreatWarningsAsErrors=true`
- `PackageLicenseExpression=MIT`
- README (`README.md`) and icon (`icon.png`) packed at the package root

## Architecture Compliance

- **Hexagonal Architecture** — ports (Abstractions) separated from adapters (Infrastructure)
- **Railway-Oriented Programming** — operations return `Result`/`Result<T>`; no exceptions for control flow
- **Independent package** — no project-specific dependencies; ships standalone
- `ConfigureAwait(false)` in library code
- Source-generated `[LoggerMessage]` logging (CA1848)

## DI Registration

```csharp
// Register the Ember SignalR abstractions
services.AddSignalRAbstractions();

// Register health tracking for a given service type
services.AddServiceHealth<MyService>();
```

## Testing & Mutation Stack

- **xUnit v3** (`xunit.v3`) running on the **Microsoft Testing Platform (MTP)** — not VSTest
- **Stryker.NET** 4.14.2 with the MTP test-runner for mutation testing
- 199 tests passing; mutation score is the coverage signal of record (MTP-native
  line coverage is blocked pending an MTP 2.x-compatible coverage extension)

See `docs/tasks/mutation-coverage-improvement-plan.md` for survivor analysis.
