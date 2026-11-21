# Package Installation Guide: DotNet.AutoNormalize

This guide details the package selection, dependencies, supported target frameworks, and minimal usage examples for all five packages in the **DotNet.AutoNormalize** ecosystem.

There is **no metapackage**. Install only the packages required for your project's architectural layer.

---

## Package Selection Matrix

| Package ID | Best Used For | Key Dependencies |
| :--- | :--- | :--- |
| **`AutoNormalize.Abstractions`** | Shared domain libraries, contracts, models, DTOs | None (Zero external dependencies) |
| **`AutoNormalize.Core`** | Console applications, workers, backend service layers | `AutoNormalize.Abstractions`, `Microsoft.Extensions.Options` |
| **`AutoNormalize.Json`** | Standalone `System.Text.Json` serialization | `AutoNormalize.Core`, `System.Text.Json` |
| **`AutoNormalize.AspNetCore`** | ASP.NET Core Minimal APIs & MVC Web APIs | `AutoNormalize.Json`, `Microsoft.AspNetCore.App` |
| **`AutoNormalize.EntityFrameworkCore`** | EF Core `DbContext` database persistence | `AutoNormalize.Core`, `Microsoft.EntityFrameworkCore` |

---

## 1. `AutoNormalize.Abstractions`

### Package ID
`AutoNormalize.Abstractions`

### Installation
```bash
dotnet add package AutoNormalize.Abstractions
```

### Purpose
Contains the core interfaces (`INormalizer<T>`, `IStringNormalizer`, `IAutoNormalizer`), execution context (`NormalizationContext`), and declarative attributes (`[PreserveWhitespace]`, `[DoNotNormalize]`, `[NormalizeWith]`).

### Target Frameworks
- `netstandard2.0`
- `net8.0`
- `net9.0`

### When to Use
Use in shared class libraries, domain entity definitions, and contract DTO assemblies where you want to annotate models with `[PreserveWhitespace]` or `[DoNotNormalize]` without pulling in the heavy normalization engine or web dependencies.

### Minimal Example
```csharp
using DotNet.AutoNormalize.Abstractions;

public class CustomerDto
{
    public string Name { get; set; } = string.Empty;

    [PreserveWhitespace]
    public string FormattedBio { get; set; } = string.Empty;

    [DoNotNormalize]
    public string SecurityToken { get; set; } = string.Empty;
}
```

---

## 2. `AutoNormalize.Core`

### Package ID
`AutoNormalize.Core`

### Installation
```bash
dotnet add package AutoNormalize.Core
```

### Purpose
The standalone high-performance normalization engine. Provides built-in string normalizers (trimming, empty-to-null, null-to-empty, Unicode Form C/D/KC/KD, casing transforms), reflection metadata caching, and cycle-safe object graph traversal.

### Target Frameworks
- `net8.0`
- `net9.0`

### Dependencies
- `AutoNormalize.Abstractions`
- `Microsoft.Extensions.Options`

### When to Use
Use in background workers, console apps, message handlers, or services that need to normalize in-memory object graphs or strings directly.

### Minimal Example
```csharp
using DotNet.AutoNormalize.Core;
using DotNet.AutoNormalize.Core.Engine;

var options = new AutoNormalizeOptions
{
    TrimStrings = true,
    EmptyStringsToNull = true
};

var normalizer = new AutoNormalizer(options);

var model = new UserProfile { DisplayName = "   Ada   ", Bio = "   " };
normalizer.Normalize(model);

// model.DisplayName == "Ada"
// model.Bio == null
```

---

## 3. `AutoNormalize.Json`

### Package ID
`AutoNormalize.Json`

### Installation
```bash
dotnet add package AutoNormalize.Json
```

### Purpose
Provides `System.Text.Json` contract modifiers (`AutoNormalizeJsonTypeInfoModifier`) and property converters that intercept tokens during deserialization, enabling constructor-time normalization of C# positional records without reflection mutation.

### Target Frameworks
- `net8.0`
- `net9.0`

### Dependencies
- `AutoNormalize.Core`
- `System.Text.Json`

### When to Use
Use when configuring custom `JsonSerializerOptions` instances in clients, microservices, or queue listeners.

### Minimal Example
```csharp
using System.Text.Json;
using DotNet.AutoNormalize.Json.Extensions;

var jsonOptions = new JsonSerializerOptions().AddAutoNormalize();

string payload = """{"FullName": "   Alan Turing   ", "Role": "   Mathematician   "}""";
var person = JsonSerializer.Deserialize<PersonRecord>(payload, jsonOptions);

public record PersonRecord(string FullName, string Role);
```

---

## 4. `AutoNormalize.AspNetCore`

### Package ID
`AutoNormalize.AspNetCore`

### Installation
```bash
dotnet add package AutoNormalize.AspNetCore
```

### Purpose
Unified ASP.NET Core integration. Automatically configures `System.Text.Json` for Minimal APIs and MVC controllers, registers `IAutoNormalizer` in dependency injection, wires MVC model binders for Query/Route/Form fields, and provides `AutoNormalizeEndpointFilter`.

### Target Frameworks
- `net8.0`
- `net9.0`

### Dependencies
- `AutoNormalize.Json`
- `Microsoft.AspNetCore.App` (FrameworkReference)

### When to Use
Use in ASP.NET Core web applications, Minimal APIs, and MVC APIs.

### Minimal Example
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAutoNormalize(options =>
{
    options.TrimStrings = true;
    options.EmptyStringsToNull = true;
});

var app = builder.Build();

app.MapPost("/api/users", (RegisterUserRequest request) => Results.Ok(request));

app.Run();
```

---

## 5. `AutoNormalize.EntityFrameworkCore`

### Package ID
`AutoNormalize.EntityFrameworkCore`

### Installation
```bash
dotnet add package AutoNormalize.EntityFrameworkCore
```

### Purpose
Provides `AutoNormalizeSaveChangesInterceptor` and `DbContextOptionsBuilder.AddAutoNormalize` extensions. Automatically normalizes string properties of entities in `EntityState.Added` and `EntityState.Modified` states prior to `SaveChanges()`, while skipping unchanged entities.

### Target Frameworks
- `net8.0`
- `net9.0`

### Dependencies
- `AutoNormalize.Core`
- `Microsoft.EntityFrameworkCore`

### When to Use
Use in any project that uses Entity Framework Core to interact with relational or document databases.

### Minimal Example
```csharp
using Microsoft.EntityFrameworkCore;

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString)
           .AddAutoNormalize();
});
```

---

## Target Framework & .NET 10 Compatibility Note

- **.NET 8.0 & 9.0**: Officially compiled, multi-targeted, and tested in CI across Windows and Linux.
- **.NET 10.0 Forward Compatibility**: .NET 10 applications consume the `lib/net9.0/` asset via NuGet standard target framework precedence. Explicit `net10.0` TFM targeting will be introduced when .NET 10 reaches General Availability.
