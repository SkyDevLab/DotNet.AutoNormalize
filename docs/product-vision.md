# Product Vision: DotNet.AutoNormalize

## 1. Vision Statement

**DotNet.AutoNormalize** is an open-source, high-performance input normalization framework for modern .NET applications. It provides consistent, declarative, and safe-by-default normalization of incoming data across all application boundaries—including ASP.NET Core Minimal APIs, MVC controllers, JSON payloads, and domain objects—without boilerplate code or risk to sensitive data.

Just as **FluentValidation** solved the fragmentation of domain and model validation, and **Polly** standardized resilience, **DotNet.AutoNormalize** aims to become the standard .NET dependency for input canonicalization.

---

## 2. The Core Problem

In enterprise web applications, incoming user input is notoriously inconsistent:
* Copy-pasted names, addresses, and emails frequently contain trailing or leading spaces (e.g., `" john.doe@company.com  "`).
* Empty text inputs from HTML forms or JSON frontends arrive as `""` (empty string) instead of `null`, complicating database `NULL` handling and unique constraints.
* Accented and international characters arrive in conflicting Unicode normalization forms (e.g., combining characters like `e` + `\u0301` vs. precomposed `\u00e9`), breaking equality comparisons and unique database indexes.
* Developers attempt to solve this by sprinkling `.Trim()` calls across DTO mappers, adding brittle `JsonConverter<string>` implementations that corrupt password fields, or writing repetitive domain sanitation methods.

---

## 3. Guiding Principles

### 3.1 Principle 1: Safety First (Do No Harm)
* **Never corrupt sensitive fields**: Passwords, cryptographic keys, authentication tokens, Base64 strings, and digital signatures must never be trimmed, normalized, or converted.
* **Deterministic Opt-Outs**: Developers must have explicit attributes (`[PreserveWhitespace]`, `[DoNotNormalize]`) and automatic heuristic rules to protect sensitive properties by name or convention.
* **Non-Destructive Execution**: Normalization should never cause `NullReferenceException` on optional properties or silently lose data.

### 3.2 Principle 2: Zero-Friction Developer Experience
* A developer should get sensible, industry-standard normalization globally with a single line of setup:
  ```csharp
  builder.Services.AddAutoNormalize();
  ```
* Advanced configurations (Unicode forms, custom property rules, custom normalizers) must follow familiar ASP.NET Core `Action<AutoNormalizeOptions>` idiomatic patterns.

### 3.3 Principle 3: Clear Boundary Definition (Normalization != Validation != Sanitization)
* **Normalization is not Validation**: Normalization canonicalizes representation (e.g., trimming whitespace, selecting Unicode Form C). It does not decide if an input is valid or reject a request.
* **Normalization is not HTML Sanitization or SQL Injection Defense**: Trimming a string does not make it safe from XSS or SQL injection. The documentation and library will explicitly disclaim security sanitization to avoid misleading developers.

### 3.4 Principle 4: Performance & Allocation Consciousness
* In high-throughput ASP.NET Core web APIs, request pipeline overhead must be measured in nanoseconds.
* When a string is already normalized, the library must avoid allocating new string objects (returning the original string reference).
* Reflection must never run on a hot per-request path. Dynamic access must be powered by cached metadata, compiled expression trees, or type contract resolvers.

### 3.5 Principle 5: Extensibility by Contract
* Developers must be able to define custom, reusable normalizers (e.g., `EmailNormalizer`, `PhoneNormalizer`, `SlugNormalizer`) via a simple interface:
  ```csharp
  public interface INormalizer<T>
  {
      T Normalize(T value, NormalizationContext context);
  }
  ```

---

## 4. Scope and Feature Boundaries

### In Scope (v1.0.0)
* **Core Engine**: Composable string normalizers (Trimming, Empty-to-Null, Null-to-Empty, Unicode Form C/D/KC/KD, Case transformations).
* **Deep Object Graph Normalization**: Normalization of classes, records, nested child objects, arrays, lists, and collections with cycle detection.
* **System.Text.Json Integration**: Metadata-aware JSON contract modifiers applying normalization during JSON deserialization without token-level side effects.
* **ASP.NET Core Web Integration**:
  * Unified `AddAutoNormalize()` configuration.
  * MVC model binding for Query parameters, Route parameters, and Form data.
  * Minimal API endpoint filters for automatic DTO and parameter normalization.
* **Declarative Attributes**: `[PreserveWhitespace]`, `[DoNotNormalize]`, `[NormalizeWith(typeof(TNormalizer))]`, `[NormalizeOptions(...)]`.
* **Heuristic Sensitive Property Protection**: Default filters ignoring properties named `Password`, `Token`, `ApiKey`, `Secret`, `Hash`, `Salt`, `Signature`, `Cert`, etc.

### Out of Scope (Explicit Non-Goals)
* **HTML Sanitization**: Removing `<script>` tags or HTML entities (belongs in specialized libraries like `HtmlSanitizer`).
* **Input Validation**: Returning `400 Bad Request` or validation error messages (belongs in `FluentValidation` or `System.ComponentModel.DataAnnotations`).
* **Cross-Site Scripting (XSS) Prevention**: Encoding output for HTML/JavaScript contexts.
* **SQL Injection Prevention**: Escaping database parameters (belongs in parameterized SQL / ORMs).
* **Direct Stream Parsing**: Mutating raw `HttpRequest.Body` streams prior to deserialization.

---

## 5. Success Metrics

1. **Adoption**: Broad adoption across the .NET community as a standard API utility.
2. **Performance**: Zero-allocation overhead on clean strings; < 5 microseconds per request on standard DTO graphs.
3. **Reliability**: 100% test coverage across nullability, immutability, cyclic references, and multithreaded concurrent requests.
4. **Developer Delight**: Clear, zero-surprise ergonomics where `builder.Services.AddAutoNormalize()` "just works" predictably.
