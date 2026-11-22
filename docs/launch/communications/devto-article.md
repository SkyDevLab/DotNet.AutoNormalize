---
title: Stop Writing .Trim() in Every Controller: Building AutoNormalize for Modern .NET
published: true
description: Why naive string trimming breaks passwords and immutable records in ASP.NET Core, and how we solved it with System.Text.Json modifiers and zero allocations.
tags: dotnet, csharp, aspnetcore, opensource
cover_image: https://raw.githubusercontent.com/SkyDevLab/DotNet.AutoNormalize/main/assets/branding/github-social-preview.png
canonical_url: https://github.com/SkyDevLab/DotNet.AutoNormalize
---

How many times have you reviewed a pull request in ASP.NET Core and spotted something like this?

```csharp
var email = request.Email?.Trim();
var firstName = string.IsNullOrWhiteSpace(request.FirstName) ? null : request.FirstName.Trim();
```

Across enterprise .NET codebases, input canonicalization is often treated as an afterthought. It gets scattered across controllers, MediatR handlers, FluentValidation transform rules, and repository methods. 

Inevitably, someone forgets a `.Trim()` on an endpoint:
* `"john@example.com "` and `"john@example.com"` end up as duplicate accounts in your database.
* An empty input `""` is stored instead of a clean database `NULL`, breaking SQL `NULL` semantics.
* Decomposed Unicode characters cause string equality comparisons to mysteriously fail.

To solve this, developers often reach for a global `JsonConverter<string>`. But as many discover the hard way, **global string converters introduce serious production bugs**.

Here is why naive solutions break, and how we architected [**AutoNormalize**](https://skydevlab.github.io/DotNet.AutoNormalize/)—an open-source input canonicalization engine for ASP.NET Core, `System.Text.Json`, and EF Core that recently hit **v1.0.0** on NuGet. (You can also try the live normalization rules and zero-allocation benchmarks in our [interactive web sandbox](https://skydevlab.github.io/DotNet.AutoNormalize/)).

---

## The 3 Hidden Traps of String Normalization

### Trap 1: The Password & Secret Corruption Trap
If you register a global string converter or reflectively trim every string property in a request, you immediately introduce a security vulnerability:
* A user sets their password to `"  correct horse battery staple  "`.
* Your global converter strips the spaces: `"correct horse battery staple"`.
* The user later logs in via a mobile client that doesn't trim, and authentication fails.
* Worse, base64 payloads, cryptographic salts, and API tokens with significant whitespace get silently mangled.

**The Fix:** Defense-in-depth heuristic shielding. AutoNormalize automatically shields sensitive property names (`Password`, `ApiKey`, `Token`, `Secret`, `Salt`, `Signature`) by default, while providing explicit declarative attributes (`[DoNotNormalize]`, `[PreserveWhitespace]`) for fine-grained control.

```csharp
public class RegisterUserRequest
{
    public string FullName { get; set; } = string.Empty; // Trims whitespace

    [PreserveWhitespace]
    public string FormattedCode { get; set; } = string.Empty; // Kept intact

    public string Password { get; set; } = string.Empty; // Shielded automatically
}
```

---

### Trap 2: Immutable C# Positional Records
Modern C# promotes immutable data transfer objects:

```csharp
public record CreateCustomerCommand(string Name, string Email);
```

If you attempt post-deserialization normalization via reflection or object traversal, you will hit a wall. Positional records have `init`-only properties and are instantiated via a primary constructor. Mutating them post-construction requires unsafe reflection or creating a new copy via `with` expressions, allocating unnecessary memory.

**The Fix:** In-Flight `System.Text.Json` Modifiers.
Rather than touching the object *after* it is constructed, AutoNormalize hooks `IJsonTypeInfoResolver.Modifiers` in `System.Text.Json`. We attach a property-scoped converter during serialization metadata contract building:

```csharp
// Attaches property-level normalization BEFORE the constructor is called
options.TypeInfoResolver = options.TypeInfoResolver?.WithAddedModifier(
    AutoNormalizeJsonTypeInfoModifier.Create(normalizeOptions));
```

The string token is canonicalized directly off the `Utf8JsonReader` stream and passed into the constructor parameters at birth. It is 100% Native AOT compatible and preserves record immutability invariants.

---

### Trap 3: The Dictionary Key Hash Collision Disaster
If an API accepts a `Dictionary<string, string>`, normalizing dictionary keys in-place can corrupt the underlying hash buckets. If keys `"key "` and `"key"` are normalized to the same string, your dictionary will throw an `ArgumentException: An item with the same key has already been added`.

**The Fix:** Strict boundary invariants. AutoNormalize strictly normalizes dictionary *values*, never dictionary *keys*.

---

## Performance: 4.83 ns & 0 Bytes Allocated

If every incoming API request and database entity passes through a normalization engine, overhead must be virtually unmeasurable.

For clean strings (strings that do not contain leading/trailing whitespace and are already in Unicode Normalization Form C), AutoNormalize implements a zero-allocation fast path:

1. Inspects the first and last characters via `char.IsWhiteSpace()`.
2. Checks Unicode canonicalization status via `string.IsNormalized(NormalizationForm.FormC)`.
3. If clean, it returns the existing string reference immediately without allocating a single byte on the managed heap.

BenchmarkDotNet results on .NET 9.0 (Windows 11, RyuJIT AVX2):

| Workload | Method | Mean Duration | Managed Allocation |
| :--- | :--- | :--- | :--- |
| **Clean String** | `CleanString_AutoNormalize` | **4.83 ns** | **0 B (Zero Allocation)** |
| **Dirty String** | `DirtyString_AutoNormalize` | **17.35 ns** | **112 B** |
| **Whitespace to Null** | `WhitespaceOnly_AutoNormalize` | **12.49 ns** | **0 B** |
| **Typical DTO (6 props)**| `TypicalDto_AutoNormalize` | **283.55 ns** | **912 B** |

*(Fun benchmarking lesson: During early microbenchmarks, our baseline measured 0.0077 ns—about 0.03 CPU cycles! RyuJIT had detected our compile-time string literal and constant-folded the call away. Always use runtime dynamic allocations like `string.Concat` in BenchmarkDotNet suites to prevent dead-code elimination.)*

---

## How to Use It in ASP.NET Core

AutoNormalize is designed to be configured once at application startup.

### 1. Install the NuGet Package
```bash
dotnet add package AutoNormalize.AspNetCore
```

### 2. Register in `Program.cs`
One line wires up JSON options, Minimal API endpoint filters, and MVC model binders for query/route/form inputs:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register AutoNormalize across your entire HTTP ingress pipeline
builder.Services.AddAutoNormalize(options =>
{
    options.TrimStrings = true;
    options.EmptyStringsToNull = true;
    options.UnicodeNormalization = NormalizationForm.FormC;
});

var app = builder.Build();

// Positional records are normalized automatically at birth:
app.MapPost("/api/users", (RegisterUserCommand command) =>
{
    return Results.Ok(command);
});

app.Run();

public record RegisterUserCommand(string FullName, string Email);
```

If you use Entity Framework Core, install `AutoNormalize.EntityFrameworkCore` to automatically intercept `SaveChanges()` and canonicalize entities before database persistence:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString)
           .AddAutoNormalize();
});
```

---

## Conclusion & Open Source

Input canonicalization should be automatic, safe-by-default, and invisible to your everyday application logic. 

**AutoNormalize** is fully open-source under the MIT license and multi-targets `.NET 8.0`, `.NET 9.0`, and `.NET Standard 2.0`.

* 🌐 **Interactive Demo & Docs**: [skydevlab.github.io/DotNet.AutoNormalize](https://skydevlab.github.io/DotNet.AutoNormalize/)
* 📦 **NuGet**: [AutoNormalize.AspNetCore](https://www.nuget.org/packages/AutoNormalize.AspNetCore/1.0.0)
* ⭐ **GitHub**: [SkyDevLab/DotNet.AutoNormalize](https://github.com/SkyDevLab/DotNet.AutoNormalize)
* 💬 **Discussions**: [Architecture Discussion & Feedback](https://github.com/SkyDevLab/DotNet.AutoNormalize/discussions/1)

We’d love to hear how your team currently handles input normalization. Have you encountered the password trimming or immutable record pitfalls in your own projects? Let's discuss in the comments below!
