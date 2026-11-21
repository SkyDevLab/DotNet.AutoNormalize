# Building a Safe and Consistent Input Normalization Pipeline for .NET

**Author**: DotNet.AutoNormalize Maintainers  
**Target Audience**: Senior .NET Developers, Software Architects, API Designers

---

## 1. The Problem with Inconsistent Input Normalization

In almost every enterprise .NET application, incoming text data suffers from minor representation inconsistencies:
- Leading or trailing spaces accidentally introduced by copy-pasting into web forms.
- Empty strings (`""`) persisting in database columns instead of `NULL`, skewing SQL `NULL` indexes and queries.
- Decomposed Unicode sequences (e.g. `Caf` + combining acute accent `\u0301`) failing equality checks against precomposed Unicode (`Café` / `\u00E9`).
- Mixed casing on canonical identifiers like postal codes or country codes.

Left unhandled, these minor inconsistencies corrupt search indexes, create duplicate records in databases, break caching lookups, and complicate reporting queries.

---

## 2. Why Manual Trimming Becomes Difficult to Maintain

The most common approach developers take is manual trimming:
```csharp
public async Task<IActionResult> Register(RegisterRequest request)
{
    var email = request.Email?.Trim();
    var name = request.Name?.Trim();
    // ... repeat across 50 endpoints
}
```

This manual approach fails in practice for several reasons:
1. **Human Error**: Developers forget to call `.Trim()` on newly added DTO properties or secondary endpoints.
2. **Code Smear**: Repetitive trimming boilerplate pollutes controllers, MediatR handlers, and service classes, distracting from core business logic.
3. **Accidental Mutilation**: A developer writes a global helper that trims all strings on a model, inadvertently trimming a user's password, cryptographic token, or API key, causing authorization failures.
4. **Validation Ordering**: If FluentValidation checks whether an email matches a regex *before* manual trimming occurs in the handler, valid emails with trailing spaces are rejected with 400 Bad Request.

---

## 3. Why a Global String JSON Converter Is Insufficient

A naive reaction to this problem is registering a global `JsonConverter<string>`:
```csharp
// The naive approach:
public class GlobalTrimStringConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetString()?.Trim();
    }
}
```

While tempting, global string converters introduce severe architectural defects:
- **Blind Property Ignorance**: The converter lacks property-level contextual awareness. It cannot distinguish between a `Username` property and a `Password` or `Signature` property.
- **Dictionary Key Collisions**: A global string converter runs on dictionary keys during deserialization. If a payload contains `"key "` and `"key"`, it triggers an `ArgumentException: An item with the same key has already been added`.
- **Breaking Non-Body Ingress**: It only handles JSON request bodies. Query string parameters, route parameters, and form posts remain unnormalized.
- **Overhead on Clean Data**: It re-allocates a new string even when the input was already trimmed.

---

## 4. System.Text.Json Integration via TypeInfo Resolver Modifiers

To solve this cleanly, `DotNet.AutoNormalize.Json` leverages `IJsonTypeInfoResolver` modifiers (`AutoNormalizeJsonTypeInfoModifier`) introduced in .NET 7/8.

Rather than registering a global converter, the modifier inspects property metadata during JSON contract generation:
1. It inspects the `JsonPropertyInfo` for the enclosing type and property name.
2. It evaluates whether the property or class is decorated with `[DoNotNormalize]` or `[PreserveWhitespace]`.
3. It checks the property name against the sensitive field protection list (e.g. `Password`, `ApiKey`).
4. If eligible, it attaches an `AutoNormalizeStringJsonConverter` **specifically to that property**.

### Constructor-Time Normalization for C# Positional Records
C# positional records (`public record RegisterUser(string Name, string Email);`) have init-only properties bound via constructor parameters. Because `System.Text.Json` applies property-scoped converters to constructor parameters *before* invoking the constructor, records are instantiated **already normalized at birth**, with zero unsafe reflection or IL post-construction mutations.

---

## 5. ASP.NET Core MVC Model Binding

While JSON handles request bodies, ASP.NET Core MVC controllers receive data via query strings, route parameters, and form posts.

`DotNet.AutoNormalize.AspNetCore` registers an `AutoNormalizeModelBinderProvider` at the head of `MvcOptions.ModelBinderProviders`.
- When an action parameter (or DTO property) is bound from `[FromQuery]`, `[FromRoute]`, or `[FromForm]`, the `AutoNormalizeModelBinder` intercepts the raw string from `ValueProviderResult`.
- It executes the normalization pipeline (trimming, empty-to-null, Unicode) before passing the value into model validation.
- If the parameter represents a complex object, it delegates down to the inner complex binder and normalizes the resulting graph.

---

## 6. Minimal API Integration

Minimal APIs in modern .NET do not execute through the MVC model binding pipeline.

`DotNet.AutoNormalize.AspNetCore` addresses Minimal APIs through dual integration:
1. **JSON Options**: Automatically configures `Microsoft.AspNetCore.Http.Json.JsonOptions` so that endpoint JSON request bodies are canonicalized during deserialization.
2. **Endpoint Filter**: Provides `AutoNormalizeEndpointFilter`, which intercepts method arguments for route and query parameters before the endpoint delegate executes.

---

## 7. Entity Framework Core SaveChanges Interception

Input normalization at the web tier protects HTTP endpoints, but what about batch background workers, message queue consumers, or legacy background tasks that insert entities directly via EF Core?

`DotNet.AutoNormalize.EntityFrameworkCore` implements `AutoNormalizeSaveChangesInterceptor` (an `ISaveChangesInterceptor`):
- **Selective State Inspection**: It queries `DbContext.ChangeTracker.Entries()`, inspecting only entities in `EntityState.Added` and `EntityState.Modified`.
- **Skip Unchanged & Deleted**: `EntityState.Unchanged` entities are skipped, avoiding unnecessary dirty-tracking marks or redundant SQL `UPDATE` statements. `EntityState.Deleted` entities are skipped entirely.
- **Owned Entities**: EF Core owned types appear as distinct tracked entries and are automatically traversed.
- **CancellationToken Support**: `SavingChangesAsync` checks the cancellation token prior to processing.

---

## 8. Metadata Caching and Object Graph Traversal

For in-memory normalization (`IAutoNormalizer.Normalize(target)`), reflection overhead can become a bottleneck if properties and attributes are re-scanned on every call.

`DotNet.AutoNormalize.Core` uses a two-tier caching architecture:
1. **`NormalizationMetadataCache`**: A thread-safe `ConcurrentDictionary<Type, ObjectTypeMetadata>` that inspects property types, getter/setter delegates, and attributes once per type.
2. **Cycle Detection**: `ObjectGraphNormalizer` maintains a `HashSet<object>(ReferenceEqualityComparer.Instance)` and checks `MaxRecursionDepth` (default 32) to prevent infinite loops on circular object graphs.

---

## 9. Security-by-Default Design

A primary mandate of `DotNet.AutoNormalize` is **safety by default**:

> [!CAUTION]
> A normalization library must never modify cryptographic signatures, authentication tokens, API keys, or passwords.

### Defense-in-Depth Heuristics
By default, properties matching common sensitive names are shielded from all trimming and casing changes:
`Password`, `Passcode`, `Token`, `AuthToken`, `AccessToken`, `RefreshToken`, `Secret`, `ClientSecret`, `SecretKey`, `ApiKey`, `PrivateKey`, `Signature`, `Hash`, `Salt`, `Passphrase`, `Certificate`.

### Explicit Attributes
Developers can override or enforce behavior explicitly:
- `[DoNotNormalize]`: Completely excludes a property or class from all normalization.
- `[PreserveWhitespace]`: Disables trimming and empty-to-null while allowing Unicode normalization.
- `[NormalizeWith(typeof(T))]`: Replaces default rules with a custom normalizer.

---

## 10. Normalization vs. Validation vs. Sanitization

A critical point of confusion in software engineering is conflating representation canonicalization with validation and sanitization:
1. **Normalization**: Canonicalizing representation (e.g. `" hello "` → `"hello"`). It makes no value judgment on whether `"hello"` is acceptable.
2. **Validation**: Asserting that data satisfies domain constraints (e.g. "Email must contain `@` and be at most 255 chars"). Normalization **precedes** validation so validators evaluate clean data.
3. **Sanitization**: Stripping or encoding hostile payloads (e.g. removing `<script>` tags). Normalization **does not sanitize HTML**.

---

## 11. Unicode Normalization (Form C)

Text in Unicode can be represented in multiple equivalent binary sequences. For example:
- **Precomposed**: `é` (U+00E9, 2 bytes in UTF-8).
- **Decomposed**: `e` + ` ́` (U+0065 + U+0301, 3 bytes in UTF-8).

Visually, both render identically. However, binary string comparisons (`string.Equals`, dictionary lookups, SQL queries) treat them as unequal.

`DotNet.AutoNormalize` checks `string.IsNormalized(NormalizationForm.FormC)`:
- If already normalized (the vast majority of strings), it immediately returns the original reference with **zero memory allocations**.
- If decomposed, it invokes `string.Normalize(NormalizationForm.FormC)`, guaranteeing consistent binary representation across your database and cache.

---

## 12. Performance Benchmarks and Trade-Offs

Microbenchmarking string libraries requires extreme care to prevent JIT constant-folding. Using non-constant runtime inputs on BenchmarkDotNet v0.14.0 (.NET 9.0 RyuJIT AVX2):

| Workload | Method | Mean Duration | Managed Allocation |
| :--- | :--- | :--- | :--- |
| **Clean String** | `CleanString_AutoNormalize` | **4.83 ns** | **0 B (Zero Allocation)** |
| **Dirty String** | `DirtyString_AutoNormalize` | **17.35 ns** | **112 B** |
| **Typical DTO (6 props)** | `TypicalDto_AutoNormalize` | **283.55 ns** | **912 B** |
| **JSON Deserialization** | `Deserialize_AutoNormalize` | **601.90 ns** | **952 B** |
| **EF Core SaveChanges** | `SaveChanges_WithAutoNormalize` | **22.23 µs** | **30.95 KB** |

### Trade-Offs
- The fast-path check (`char.IsWhiteSpace` on first/last character + `IsNormalized`) adds **3.14 nanoseconds** over raw `.Trim()`. In return, it guarantees zero heap allocations on clean data.
- EF Core change tracking inspection adds ~9.8 microseconds per save, which is negligible compared to network roundtrips to relational databases (1–10 ms).

---

## 13. Immutable Objects & Known Limitations

1. **In-Memory Positional Records**: Calling `normalizer.Normalize(record)` on an already-instantiated positional record cannot mutate properties post-construction. The engine safely skips `init;` properties without throwing exceptions. Full record normalization occurs during `System.Text.Json` deserialization.
2. **Dictionary Keys**: Dictionaries normalize values only. Keys are never mutated to prevent hashcode corruption or duplicate key exceptions.
3. **Read-Only Collections**: `IReadOnlyList<T>` elements cannot be replaced in-place without copying and are safely skipped.

---

## 14. Lessons Learned

1. **JIT Constant Folding Is Real**: Initial microbenchmarks reported 0.0077 ns for manual trimming because compiler constant folding eliminated the call. Microbenchmarks must always use runtime-allocated non-constants.
2. **Normalize at Ingress**: Normalizing JSON tokens *as they are read* is significantly faster than deserializing an object and then traversing its graph with reflection.
3. **Safety Beats Convenience**: Heuristic sensitive name protection and explicit opt-out attributes prevent security disasters that occur with naive global string trimmers.

---

## Conclusion

By combining property-aware `System.Text.Json` modifiers, ASP.NET Core model binders, and EF Core change tracking interceptors, `DotNet.AutoNormalize` delivers consistent, zero-boilerplate input canonicalization while maintaining high performance and security boundaries.
