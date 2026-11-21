# .NET 10 Evaluation & Compatibility Strategy

## 1. Overview
As part of Phase 6, `DotNet.AutoNormalize` evaluated support for **.NET 10** alongside the active Long Term Support (**LTS**) release (.NET 8) and Standard Term Support (**STS**) release (.NET 9).

This document outlines the architectural compatibility, NuGet asset selection rules, API surface readiness, and roadmap for first-class `net10.0` TFM targeting.

---

## 2. Framework Targeting Analysis

| Package | Current Target Frameworks | .NET 10 Consumer Resolution | Status |
| :--- | :--- | :--- | :--- |
| **`DotNet.AutoNormalize.Abstractions`** | `netstandard2.0;net8.0;net9.0` | Selects `lib/net9.0/` asset | Fully Compatible |
| **`DotNet.AutoNormalize.Core`** | `net8.0;net9.0` | Selects `lib/net9.0/` asset | Fully Compatible |
| **`DotNet.AutoNormalize.Json`** | `net8.0;net9.0` | Selects `lib/net9.0/` asset | Fully Compatible |
| **`DotNet.AutoNormalize.AspNetCore`** | `net8.0;net9.0` | Selects `lib/net9.0/` asset | Fully Compatible |
| **`DotNet.AutoNormalize.EntityFrameworkCore`** | `net8.0;net9.0` | Selects `lib/net9.0/` asset | Fully Compatible |

---

## 3. Forward Compatibility via NuGet Asset Precedence

### 3.1 NuGet Resolution Semantics
According to the official [.NET Target Framework Moniker Resolution Specification](https://learn.microsoft.com/en-us/nuget/create-packages/supporting-multiple-target-frameworks), when a consumer project targets `net10.0` and references a package without an explicit `lib/net10.0` folder:
1. NuGet inspects the package's available framework folders.
2. NuGet selects the highest framework version that is compatible with `net10.0`.
3. In `DotNet.AutoNormalize`, `lib/net9.0/` is resolved as the best match.
4. The assembly loads with zero warnings and runs natively on the .NET 10 CLR.

### 3.2 Runtime & JIT Compatibility
- **RyuJIT & Hardware Intrinsics**: `DotNet.AutoNormalize.Core` relies on standard BCL primitives (`char.IsWhiteSpace`, `string.IsNormalized`, `string.AsSpan()`) that benefit from .NET 10's JIT vectorization improvements without code changes.
- **Reflection & Metadata**: `System.Reflection` metadata APIs, `IsExternalInit` detection, and type caching behave identically on .NET 10.
- **System.Text.Json**: `IJsonTypeInfoResolver` modifier pipeline in `DotNet.AutoNormalize.Json` remains 100% backward and forward-compatible with .NET 10's STJ architecture.
- **ASP.NET Core & EF Core**: The DI service registration, MVC model binder providers, Minimal API endpoint filters, and EF Core `SaveChangesInterceptor` interfaces remain contractually identical in .NET 10.

---

## 4. Native AOT & Trimming Considerations
In .NET 10, Native AOT and trimming adoption is expanded across ASP.NET Core Minimal APIs.
* `DotNet.AutoNormalize.Abstractions` and `DotNet.AutoNormalize.Json` use statically inspectable contract resolvers and avoid runtime `Reflection.Emit`.
* The `AutoNormalizeJsonTypeInfoModifier` integrates directly with source-generated or runtime type resolvers via `WithAddedModifier`, ensuring full compatibility with Native AOT in both .NET 9 and .NET 10.

---

## 5. Explicit `net10.0` TFM Roadmap
Once .NET 10 reaches General Availability (GA):
1. `Directory.Build.props` will update target frameworks to:
   ```xml
   <TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>
   ```
2. Central package management in `Directory.Packages.props` will add `10.0.0` versions for Microsoft extensions and EF Core.
3. Automated CI builds on GitHub Actions will incorporate the official .NET 10 SDK matrix.

### Conclusion
`DotNet.AutoNormalize` is fully forward-compatible with .NET 10 applications today through standard NuGet asset resolution, and requires zero breaking changes to adopt first-class `net10.0` targeting upon .NET 10 GA.
