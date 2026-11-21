# LinkedIn Communication Drafts: DotNet.AutoNormalize

> **Note**: These are prepared drafts for future public announcement. Do not publish until officially approved.

---

## Post 1: Main Launch Announcement

**Title**: Introducing DotNet.AutoNormalize: Consistent Input Normalization for .NET

How many times have you written `.Trim()` in controller actions or DTO handlers?

Across large .NET applications, text normalization is often an afterthought:
- Trailing spaces causing login failures or duplicate user records.
- Inconsistent empty strings (`""`) instead of `NULL` in SQL columns.
- Decomposed Unicode characters failing string equality comparisons.
- Accidental mutilation of passwords or API keys when global trimmers are applied.

Today, we're introducing **DotNet.AutoNormalize** — an open-source library that automates representation canonicalization across ASP.NET Core Minimal APIs, MVC Controllers, System.Text.Json, and Entity Framework Core.

### Key highlights:
- **Zero-Allocation Fast Path**: Clean strings bypass allocation, completing in under 5 nanoseconds.
- **In-Flight JSON Deserialization**: System.Text.Json modifiers normalize tokens before constructor invocation, supporting immutable C# positional records cleanly.
- **Security-by-Default**: Heuristic shielding preserves passwords, tokens, API keys, and hashes.
- **EF Core Interceptor**: Automatically canonicalizes `Added` and `Modified` entity states during `SaveChanges()`.

We've published initial preview packages on NuGet and open-sourced the repository under the MIT license:
GitHub: `https://github.com/SkyDevLab/DotNet.AutoNormalize`

We would love your thoughts on the API design and architectural choices!

#dotnet #csharp #aspnetcore #opensource #softwarearchitecture

---

## Post 2: Short Technical Teaser (Pre-Launch)

Quick question for .NET architects: How does your team handle input trimming across APIs?

Do you:
1. Write manual `.Trim()` in every controller or service?
2. Use custom model binders?
3. Rely on FluentValidation Transform rules?
4. Use a global `JsonConverter<string>`?

Global string converters often create subtle bugs: they can accidentally mutate passwords and cryptographic keys, or crash on dictionary key collisions.

We’ve been working on an open-source solution that integrates deeply with System.Text.Json type contract modifiers, ASP.NET Core Minimal APIs, and EF Core change tracking with zero heap allocations on clean inputs.

Preview coming soon to the .NET open-source ecosystem.

#dotnet #aspnetcore #csharp #webapi

---

## Post 3: Technical Lessons-Learned

**3 Lessons Learned Building a High-Performance .NET Normalization Engine**

While designing **DotNet.AutoNormalize**, we encountered three fascinating runtime challenges:

1. **The 0.0077 ns Benchmark Trap**:
In early microbenchmarks, our manual trim baseline measured 0.0077 ns per operation (roughly 0.03 CPU cycles). The RyuJIT compiler had recognized our compile-time string literal and constant-folded the call away. Always use dynamic runtime string allocations (`string.Concat`) in BenchmarkDotNet suites to prevent dead-code elimination.

2. **Immutable C# Records Require Pre-Constructor Normalization**:
You cannot safely mutate positional records post-construction via reflection without breaking language immutability invariants. By utilizing `IJsonTypeInfoResolver` modifiers in `System.Text.Json`, you can normalize tokens *before* the record constructor is invoked.

3. **Never Mutate Dictionary Keys**:
Normalizing dictionary values is safe, but normalizing dictionary keys in-place invalidates hashcode bucket indexing and triggers collision exceptions on duplicate transformed keys. Treat dictionary keys as immutable.

Read the full technical deep dive in our repository:
`https://github.com/SkyDevLab/DotNet.AutoNormalize`

#dotnet #csharp #performance #benchmarkdotnet #opensource
