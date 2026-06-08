# Mutation Coverage Improvement Plan

> Updated 2026-06-07. Mutation testing now runs on **Stryker.NET 4.14.2** with the
> **Microsoft Testing Platform runner** against **xUnit v3** tests. Run it with:
> `dotnet stryker` from `code/src/IndFusion.Ember/` (config: `stryker-config.json`).

## Current Status (real baseline — the previous 46% was stale ExxerCube data)

| Metric | Initial (this session) | After ExxerHub pass | Target |
|---|---|---|---|
| **Mutation score** | 40.29% | **47.57%** | 80% |
| Killed | 80 | 95 | — |
| Survived | 50 | 51 | <20 |
| Timeout | 3 | 3 | — |
| NoCoverage | 73 | 57 | 0 |
| Total mutants | 303 | 303 | — |

What moved the needle: fixed `TestHubHelper` (it looked up the **public** `Hub.Context/Clients/Groups`
properties with `BindingFlags.NonPublic`, so the mocks were silently never applied — every hub send
returned "Clients property is null"). With that fixed, new `ExxerHub` tests cover the null-guards,
success paths, and connect/disconnect logging.

## What remains — analyzed by reading each survivor's source line

The 44 survivors + 57 no-coverage mutants are **not** genuine missing-behavior tests. Breakdown of the
44 survivors:

| Category | Count | Killable? |
|---|---|---|
| `ConfigureAwait(false)` → `(true)` | 16 | No — equivalent mutant (no observable behavior) |
| `_logger.Xxx();` call removal (in SUT bodies) | 17 | Only by asserting exact log calls — brittle |
| `Dispose()`/`GC.SuppressFinalize`/`ThrowIfNull` removal | ~9 | No — equivalent / downstream throws anyway |
| `timeSinceLastSend >= _throttleInterval` boundary | 1 | Yes, but needs clock injection (production refactor) |
| `?? new ReconnectionStrategy()` left operand | 1 | No — strategy isn't observable from outside |

So the behavioral test coverage is effectively complete; the sub-51% score is dominated by **equivalent
mutants and brittle log-call mutants**, not real gaps.

> Note on the `*Log` classes: ignoring them is a **no-op** — the source-generated `[LoggerMessage]` partial
> methods (attribute-only, no body) produce ~1 mutant total. The brittle "logging" mutants are the
> `_logger.Xxx()` **call statements inside the SUT methods**, not the Log boilerplate.

### The 57 no-coverage mutants (out of scope for this session's choices)
- **23 Blazor** (`DashboardComponent`, `ConnectionStateIndicator`) — need **bUnit** + the Razor-SDK switch.
- **~Dashboard `ConnectAsync`/`DisconnectAsync` paths** — need a mockable `HubConnection` (it's a concrete
  class; requires a thin wrapper interface to test).

### Flaky-timing finding (higher value than chasing equivalents)
Identical Stryker runs scored 47.57% then 50.97% with **no code change** — a ±3pt swing. `MessageBatcher`
and `MessageThrottler` use real `Timer`/`Task.Delay`/`DateTime.UtcNow`, so some mutants flip
Killed/Survived/Timeout between runs. **Inject an abstract clock + timer** to make these deterministic;
that both stabilizes the score and unlocks the one genuine boundary mutant above.

## Recommended next steps (in order)
1. **Inject a clock/timer abstraction** into `MessageThrottler`/`MessageBatcher` — stabilizes the flaky
   score and makes the throttle boundary testable.
2. **Add a `HubConnection` wrapper interface** so `Dashboard.ConnectAsync`/`DisconnectAsync` become unit-testable.
3. **Add bUnit + switch to `Microsoft.NET.Sdk.Razor`** for the Blazor components (also makes `.razor` compile).
4. Decide whether to formally exclude `ConfigureAwait` + log-call mutants (per-line `// Stryker disable once`,
   if syntax-verified) or simply document them as equivalents — so the headline score reflects real coverage.

## Notes
- `.razor` files are **not compiled** today: the project uses `Microsoft.NET.Sdk`, not
  `Microsoft.NET.Sdk.Razor`. The Blazor markup has no compile-time or test coverage.
- MTP-native line coverage is currently unavailable: `Microsoft.Testing.Extensions.CodeCoverage` 18.6.2
  is built against MTP 1.x and is binary-incompatible with the MTP 2.1.0 that xUnit v3 3.2.2 requires.
  Mutation score is the coverage signal of record until a 2.x-compatible package ships.
