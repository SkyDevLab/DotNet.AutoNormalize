# Release Notes: DotNet.AutoNormalize v1.0.0-preview.1

**Release Date**: November 21, 2026  
**Status**: Preview Release (v1.0.0-preview.1)

---

## 1. Highlights

DotNet.AutoNormalize v1.0.0-preview.1 is the initial public preview of an input normalization library for ASP.NET Core and Entity Framework Core.

### Key Capabilities
- **Zero-Allocation Fast Path**: Clean strings that do not require trimming or Unicode re-encoding return original references with 0 bytes allocated and ~3.1 ns overhead.
- **System.Text.Json In-Flight Deserialization**: Normalizes string properties as JSON tokens are read from the stream, eliminating post-deserialization object traversals.
- **C# Positional Record Support**: Normalizes constructor arguments *at birth* for immutable `record` types without unsafe reflection mutations.
- **ASP.NET Core Web Integration**: Provides unified `builder.Services.AddAutoNormalize()` configuration across Minimal APIs, MVC controllers, and model binders (Query, Route, Form).
- **Entity Framework Core Change Tracker Interceptor**: Automatically canonicalizes entities in `EntityState.Added` and `EntityState.Modified` states prior to `SaveChanges()`, skipping unchanged entities.
- **Defense-in-Depth Sensitive Guard**: Automatically protects passwords, tokens, API keys, certificates, and cryptographic hashes from accidental modification.

---

## 2. Packages Included in this Release

1. **`DotNet.AutoNormalize.Abstractions` (1.0.0-preview.1)**
   - Target frameworks: `netstandard2.0`, `net8.0`, `net9.0`.
   - Contracts: `INormalizer<T>`, `IStringNormalizer`, `IAutoNormalizer`, `[PreserveWhitespace]`, `[DoNotNormalize]`, `[NormalizeWith]`.
2. **`DotNet.AutoNormalize.Core` (1.0.0-preview.1)**
   - Target frameworks: `net8.0`, `net9.0`.
   - Components: `AutoNormalizeOptions`, built-in normalizers, metadata caching, and cycle-safe graph traversal.
3. **`DotNet.AutoNormalize.Json` (1.0.0-preview.1)**
   - Target frameworks: `net8.0`, `net9.0`.
   - Components: `AutoNormalizeJsonTypeInfoModifier`, property converters, constructor parameter normalization.
4. **`DotNet.AutoNormalize.AspNetCore` (1.0.0-preview.1)**
   - Target frameworks: `net8.0`, `net9.0`.
   - Components: `AddAutoNormalize` service extensions, MVC model binders, Minimal API endpoint filters.
5. **`DotNet.AutoNormalize.EntityFrameworkCore` (1.0.0-preview.1)**
   - Target frameworks: `net8.0`, `net9.0`.
   - Components: `AutoNormalizeSaveChangesInterceptor`, `DbContextOptionsBuilder.AddAutoNormalize`.

---

## 3. Package Checksums (SHA-256)

```text
32cd54926f46120d0d3c6038d5d2041c327cca35c74ab07bb156cd7a4d277462  DotNet.AutoNormalize.Abstractions.1.0.0-preview.1.nupkg
72c4697effe96de6ba93fa20b5a757ed73d0692b95276ba05aec1e3179da483f  DotNet.AutoNormalize.Abstractions.1.0.0-preview.1.snupkg
5685b6dc09b0d928f5dab4390445093afbcff7d273c5db31bae6f93c2baba1c2  DotNet.AutoNormalize.AspNetCore.1.0.0-preview.1.nupkg
9a54f9764c29e94ada2f1ca81aaf34e7dfdb224ac19adef393f6e40fd692e29e  DotNet.AutoNormalize.AspNetCore.1.0.0-preview.1.snupkg
5be526ef927bec2b5fc1dc8a32f6bbd2625ec9e204e0a8683f3ac4e08e8052f0  DotNet.AutoNormalize.Core.1.0.0-preview.1.nupkg
3acd94941a1933da556bb6a32420ac6261ac8f5bd56c6fd2af6ea1b46f156d0d  DotNet.AutoNormalize.Core.1.0.0-preview.1.snupkg
17bc805a185aaccfc411a94a59fa502e407fcbcfe9c76c61449ef9e6287773ef  DotNet.AutoNormalize.EntityFrameworkCore.1.0.0-preview.1.nupkg
06dd860d33f046afea53961fc6e8ed2856062dee329d9073cf6a78fd8012376b  DotNet.AutoNormalize.EntityFrameworkCore.1.0.0-preview.1.snupkg
c2957f8f71c94059cdec0b36247c5c6c09cc4b77927cae5473263947e644d2ad  DotNet.AutoNormalize.Json.1.0.0-preview.1.nupkg
fc9e845873707fbba6e81095f5b51191fecc38b6ba13994032bd1e847237efc6  DotNet.AutoNormalize.Json.1.0.0-preview.1.snupkg
```

---

## 4. Verification Evidence
- 77/77 tests passing across the solution test suite and package consumer validation suite.
- 0 compiler warnings under `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`.
- Verified on .NET 9.0 SDK with forward compatibility for .NET 10.
