# Changelog

All notable changes to DotNet.AutoNormalize will be documented in this file.

The format is based on Keep a Changelog, and this project adheres to Semantic Versioning.

## [1.0.0] - 2026-11-21

### General Availability (GA) Release
- Official stable release of `AutoNormalize` across all five packages on NuGet.org:
  - `AutoNormalize.Abstractions`
  - `AutoNormalize.Core`
  - `AutoNormalize.Json`
  - `AutoNormalize.AspNetCore`
  - `AutoNormalize.EntityFrameworkCore`
- Fully sanitized CommonMark documentation and assets for NuGet.org rendering.
- Zero-allocation clean-string fast path (4.83 ns, 0 bytes allocated).
- Full multi-target compatibility for .NET 8.0, .NET 9.0, and .NET Standard 2.0.

## [1.0.0-preview.1] - 2026-11-21

### Added
- **DotNet.AutoNormalize.Abstractions**:
  - `INormalizer<T>`, `IStringNormalizer`, and `IAutoNormalizer` interfaces.
  - `[PreserveWhitespace]`, `[DoNotNormalize]`, and `[NormalizeWith]` attributes.
  - Multi-targeting for `netstandard2.0`, `net8.0`, and `net9.0`.
- **DotNet.AutoNormalize.Core**:
  - Built-in normalizers: trimming, empty-to-null, null-to-empty, Unicode Form C/D/KC/KD, and casing transforms.
  - High-performance `AutoNormalizer` with reflection metadata caching and recursion cycle detection.
  - Sensitive property name heuristic safety guard.
- **DotNet.AutoNormalize.Json**:
  - `AutoNormalizeJsonTypeInfoModifier` and property-level string converter for `System.Text.Json`.
  - Native normalization of C# positional records and `init` properties at constructor binding time.
- **DotNet.AutoNormalize.AspNetCore**:
  - `AddAutoNormalize` extensions on `IServiceCollection` for unified Minimal APIs, MVC, and JSON options configuration.
  - MVC `IModelBinderProvider` for Query, Route, Form, and Header string values.
  - Minimal API `AutoNormalizeEndpointFilter` for endpoint parameter normalization.
- **DotNet.AutoNormalize.EntityFrameworkCore**:
  - `AutoNormalizeSaveChangesInterceptor` and `DbContextOptionsBuilder.AddAutoNormalize` for pre-persistence change tracking normalization.
- **Samples & Verification**:
  - 4 runnable sample applications (`BasicUsage`, `MinimalApi`, `WebApi`, `EntityFrameworkCore`).
  - Automated public API approval test suite and package consumer validation suite.
