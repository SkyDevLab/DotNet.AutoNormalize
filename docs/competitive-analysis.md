# Competitive Analysis & Ecosystem Research: DotNet.AutoNormalize

## 1. Executive Summary

In contemporary .NET and ASP.NET Core development, handling incoming string data consistently across APIs, background workers, and database operations remains an unstandardized, ad-hoc, and error-prone process. Developers repeatedly write custom `.Trim()` calls in mapping profiles, write one-off `JsonConverter<string>` implementations, configure fragile model binders, or sprinkle trimming logic across service layers.

This document examines existing community libraries, common bespoke patterns, and architectural integration points within ASP.NET Core and Entity Framework Core. It identifies the operational deficiencies of existing solutions and articulates the clear architectural justification for **DotNet.AutoNormalize**.

---

## 2. Existing Libraries and Approaches

### 2.1 Samhammer.AspNetCore.StringTrimming
* **Repository & Author**: Samhammer AG ([GitHub](https://github.com/SamhammerAG/Samhammer.AspNetCore.StringTrimming))
* **Scope**: Automatic trimming of leading and trailing whitespace in ASP.NET Core MVC model binding and `System.Text.Json`.
* **Strengths**:
  * Simple drop-in configuration for MVC applications.
  * Provides an `[NoTrim]` attribute for property-level opt-outs.
* **Weaknesses & Limitations**:
  * **Trimming-only**: Incapable of Unicode normalization (FormC, FormD), empty-string-to-null conversion, or domain-specific normalizations (e.g., email lowercase canonicalization).
  * **Lack of Minimal APIs First-Class Support**: Primarily architected around MVC's `IModelBinderProvider` and legacy MVC controller conventions. Does not provide native `IEndpointFilter` or modern endpoint parameter binding pipeline integration.
  * **No Unified Normalization Engine**: Trimming logic is tightly coupled to ASP.NET Core web abstractions; cannot be reused in console apps, message queues (MassTransit, RabbitMQ), MediatR pipelines, or domain models.
  * **No Object Graph Traversal**: Lacks support for deep normalization of complex nested object graphs, collections, and dictionaries outside the web deserializer.
  * **No Sensitive Property Guard**: No built-in heuristics to prevent accidental trimming of passwords, API keys, cryptographic signatures, or base64 tokens unless manually decorated with `[NoTrim]`.

### 2.2 DimonSmart.StringTrimmer / StringTrimmerGenerator
* **Repository & Author**: Dmitry Dorogoy ([GitHub](https://github.com/DimonSmart/StringTrimmer))
* **Scope**: Trimming string properties on classes using reflection and Roslyn Source Generators.
* **Strengths**:
  * Uses source generators to eliminate reflection overhead at runtime for designated classes.
* **Weaknesses & Limitations**:
  * **Manual Invocation**: Requires developers to explicitly invoke generated methods (e.g., `dto.Trim()`) at their endpoint or service boundaries.
  * **Zero Pipeline Integration**: Does not integrate with ASP.NET Core model binding, Minimal APIs, `System.Text.Json`, or EF Core.
  * **Limited Normalization Capabilities**: Restricted to `.Trim()`. Does not support Unicode normalization forms, custom normalizers (`INormalizer<T>`), or configurable string-to-null rules.
  * **Immutable Record & Init-only Incompatibility**: Source generation primarily assumes mutable properties with public setters; struggles with init-only (`init;`) properties and primary constructor positional records without re-instantiation.

### 2.3 Bespoke Global `JsonConverter<string>`
A common pattern promoted on blogs and StackOverflow is registering a global `JsonConverter<string>`:

```csharp
public class GlobalTrimStringConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        return string.IsNullOrWhiteSpace(str) ? null : str.Trim();
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        => writer.WriteStringValue(value);
}
```

* **Severe Pitfalls**:
  * **Indiscriminate Mutation**: Operates at the token level without knowledge of the declaring class or target property. It will silently mutate passwords, secrets, base64 strings, cryptographic hashes, and markdown content containing intentional indentation.
  * **Cannot Support Property Attributes**: A global `JsonConverter<string>` does not have access to property-level attributes like `[PreserveWhitespace]` or `[NormalizeWith]` during deserialization.
  * **MVC vs. Minimal API Divergence**: Applies only to JSON payloads. Query strings, route parameters, and form submissions remain unnormalized unless paired with a separate custom model binder.

### 2.4 FluentValidation `.Transform()`
* **Scope**: Validation library with transformation capabilities.
* **Weaknesses**:
  * Violates separation of concerns: Validation evaluates whether data meets business and invariant rules (`IsValid`); transformation mutates data.
  * Modifying DTOs during validation can cause subtle side effects, especially in validation re-runs or concurrent validation pipelines.
  * Requires explicit `.Transform(x => x?.Trim())` on every single rule definition across hundreds of validators.

### 2.5 Ad-Hoc EF Core Value Converters and Interceptors
* **Scope**: Normalizing strings at the database boundary via `ConfigureConventions` (`ValueConverter<string, string>`) or `ISaveChangesInterceptor`.
* **Weaknesses**:
  * **Late Normalization**: If normalization occurs only at `SaveChanges()`, the application's service layer, domain events, authorization checks, and validation layers execute against dirty, unnormalized inputs.
  * **Query Translation Side Effects**: Applying a global `ValueConverter<string, string>` to all string properties can alter SQL generation, prevent index utilization, or prevent sargable query lookups in EF Core.

---

## 3. Comprehensive Competitive Matrix

| Capability / Dimension | Samhammer StringTrimming | DimonSmart StringTrimmer | Hand-Rolled JsonConverter | FluentValidation Transform | **DotNet.AutoNormalize** |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Leading & Trailing Trim** | Yes | Yes | Yes | Yes (Manual) | **Yes (Configurable)** |
| **Empty Strings to Null** | No | No | Sometimes (Hardcoded) | No (Manual) | **Yes (Configurable)** |
| **Unicode Normalization (FormC/D/KC/KD)** | No | No | No | No | **Yes (Built-in)** |
| **Custom Normalizer Pipeline (`INormalizer<T>`)** | No | No | No | No | **Yes (Extensible)** |
| **System.Text.Json TypeInfo Modifier Support** | Basic Converter | No | Low-level Converter | No | **Yes (Metadata-Aware)** |
| **ASP.NET Core MVC (Query, Route, Form)** | Yes | No | No | No | **Yes (ModelBinderProvider)** |
| **ASP.NET Core Minimal APIs (`IEndpointFilter`)**| No | No | No | No | **Yes (Native Filters)** |
| **EF Core SaveChanges Interceptor** | No | No | No | No | **Yes (Dedicated Package)** |
| **Deep Nested Object Graph Traversal** | No | Shallow | No | No | **Yes (Compiled Delegates)** |
| **Collections, Arrays & Dictionaries** | No | No | No | Manual per item | **Yes** |
| **C# Records & Init-Only Properties** | Partial | Partial | Yes (Deserializer only)| No | **Yes** |
| **Cycle Detection / StackOverflow Protection** | N/A | No | N/A | N/A | **Yes (Reference Tracking)**|
| **Sensitive Property Heuristics (Passwords/Keys)**| No | No | No (Dangerous) | N/A | **Yes (Safe by Default)** |
| **Property Opt-Out Attributes** | `[NoTrim]` | No | No | N/A | **`[PreserveWhitespace]`, `[DoNotNormalize]`** |
| **Standalone Engine (Non-Web)** | No | Yes | No | Yes | **Yes (`IAutoNormalizer`)** |
| **Performance Strategy** | Reflection / Converter | Source Gen / Reflection | Converter | Delegate | **Compiled Lambdas + Cached Metadata** |

---

## 4. Key Gaps in the Ecosystem

1. **Absence of an Industry Standard**: The .NET ecosystem has AutoMapper for mapping, FluentValidation for validation, MediatR for messaging, and Polly for resilience. However, there is no canonical, universally adopted library for **input normalization**.
2. **Context-Blind JSON Deserialization**: Existing JSON-based approaches lack property metadata context, leading developers to either risk breaking sensitive fields (passwords) or abandon global normalization entirely.
3. **Fragmented Application Boundaries**: Developers must stitch together custom model binders for MVC, JSON converters for API payloads, endpoint filters for Minimal APIs, and interceptors for EF Core, leading to inconsistent normalization across different transport mechanisms.
4. **Lack of Unicode Awareness**: Most solutions only call `string.Trim()`, completely ignoring Unicode composition differences (e.g., combining characters vs. precomposed glyphs), which leads to duplicate records and string comparison failures in multilingual systems.
5. **Absence of Safe-by-Default Security Guards**: No existing tool automatically identifies and excludes sensitive security properties (e.g., `Password`, `Token`, `Secret`, `ApiKey`) from normalization.

---

## 5. Conclusion & Product Justification

The research confirms a clear, unoccupied, and high-value position in the .NET ecosystem. **DotNet.AutoNormalize** is justified not as another string utility, but as an architectural framework that unifies:
1. A high-performance, decoupled Core normalization engine.
2. Property-aware JSON serialization integration via modern `System.Text.Json` type contract modifiers.
3. Unified transport-layer integrations for ASP.NET Core MVC and Minimal APIs.
4. Enterprise-grade safety guards protecting sensitive and cryptographic data.
