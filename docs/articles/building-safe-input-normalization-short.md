# Building a Safe and Consistent Input Normalization Pipeline for .NET (Summary)

**Target**: Dev.to / .NET Community Blog  
**Read Time**: 5 minutes

---

## The Hidden Bug in Every Web API

Ever had a user fail to log in because their mobile keyboard inserted a trailing space after their email? Or wondered why your database has both `""` and `NULL` in the same column?

We repeatedly write:
```csharp
var email = request.Email?.Trim();
var name = request.Name?.Trim();
```

Across 50 endpoints and hundreds of DTOs, manual trimming becomes impossible to maintain.

---

## Why Global `JsonConverter<string>` Breaks Everything

Many developers try registering a global string converter:
```csharp
public class GlobalTrimConverter : JsonConverter<string> { ... }
```
This causes major issues:
1. It silently trims passwords and API keys, breaking authentication.
2. It trims dictionary keys, causing duplicate key exceptions and hash table corruption.
3. It does not touch Query strings, Route parameters, or EF Core.

---

## A Better Way: DotNet.AutoNormalize

**DotNet.AutoNormalize** solves this at the application boundary:

```csharp
var builder = WebApplication.CreateBuilder(args);

// One line configures Minimal APIs, MVC, and System.Text.Json
builder.Services.AddAutoNormalize(options =>
{
    options.TrimStrings = true;
    options.EmptyStringsToNull = true;
    options.UnicodeNormalization = NormalizationForm.FormC;
});
```

### Key Architectural Highlights
1. **System.Text.Json Property Modifiers**: Rather than a global converter, it hooks property contracts, normalizing constructor parameters *before* positional record instantiation.
2. **Zero-Allocation Fast Path**: Clean strings bypass allocation, completing in **4.8 nanoseconds** with **0 bytes allocated**.
3. **Sensitive Field Defense**: Names like `Password`, `ApiKey`, and `Token` are automatically shielded from trimming.
4. **EF Core Interceptor**: Automatically canonicalizes entities in `Added` and `Modified` states during `SaveChanges()`.

---

## Summary

Normalization is representation canonicalization. It does not replace validation (FluentValidation) or HTML sanitization, but it ensures your APIs and databases deal with clean, standardized data.

Explore the open-source project on GitHub: `https://github.com/SkyDevLab/DotNet.AutoNormalize`
