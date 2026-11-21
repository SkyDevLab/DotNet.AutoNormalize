# API Compatibility & Conformance Report: Phase 3

## 1. Conformance with Frozen API Specification

The implementation of `DotNet.AutoNormalize.Abstractions` and `DotNet.AutoNormalize.Core` was audited against the frozen API contract defined in `docs/api-design-proposal.md`.

| Frozen API Member | Implementation Status | Conformance Notes |
| :--- | :--- | :--- |
| `INormalizer<T>` | Fully Implemented | Matches frozen signature exactly. |
| `IStringNormalizer` | Fully Implemented | Inherits `INormalizer<string?>`. |
| `IAutoNormalizer` | Fully Implemented | Implements generic and runtime-typed overloads. |
| `NormalizationContext` | Fully Implemented | Supports `PropertyInfo`, `TargetType`, `Services`, `Items`. |
| `PreserveWhitespaceAttribute` | Fully Implemented | Valid targets: `Property \| Field \| Parameter`. |
| `DoNotNormalizeAttribute` | Fully Implemented | Valid targets: `Property \| Field \| Class \| Struct \| Parameter`. |
| `NormalizeWithAttribute` | Fully Implemented | Validates normalizer type implements `IStringNormalizer` or `INormalizer<string?>`. |
| `AutoNormalizeOptions` | Fully Implemented | Includes all configured properties, options validation, and sensitive heuristics. |
| `WhitespaceTrimMode` | Fully Implemented | `Both`, `Start`, `End`. |
| `CasingTransform` | Fully Implemented | `None`, `LowerCase`, `UpperCase`. |

### Implementation Adjustments
During test authoring for sensitive properties, `"SecretKey"` and `"Passphrase"` were added to the default `SensitivePropertyNames` set to provide defense-in-depth protection for common cloud authentication DTO naming patterns.

---

## 2. Multi-Targeting Verification

Both projects were verified via MSBuild against their configured target frameworks:

### 2.1 `DotNet.AutoNormalize.Abstractions`
* **Target Frameworks**: `netstandard2.0;net8.0;net9.0`
* **Result**: Clean build with 0 warnings and 0 errors across all three target frameworks.
* **Compatibility Achievement**: Downstream libraries targeting `.NET Standard 2.0` can reference attributes (`[PreserveWhitespace]`, `[DoNotNormalize]`) without depending on the .NET 8 or .NET 9 runtimes.

### 2.2 `DotNet.AutoNormalize.Core`
* **Target Frameworks**: `net8.0;net9.0`
* **Result**: Clean build with 0 warnings and 0 errors across both target frameworks.

---

## 3. Native AOT & Trimming Readiness

1. **No Dynamic Code Generation**: The engine intentionally avoids `System.Reflection.Emit` and runtime `Expression.Compile()`.
2. **Metadata Cache**: Property reflection occurs through standard reflection inspection, cached in a thread-safe dictionary.
3. **No Unsafe Type Pointers**: Managed code only, 100% portable across Windows, Linux, and macOS runtimes.

---

## 4. Known Limitations

1. **Post-Construction In-Memory Records**: Positional records and types with only `init;` properties cannot be modified in place by `IAutoNormalizer.Normalize()` after object construction. This is by design in C#. Full record normalization occurs during JSON deserialization in Phase 4.
2. **Read-Only Collections**: If an object exposes a read-only list (`IReadOnlyList<string>` with no `IList` setter or indexer), in-place modification is impossible without re-instantiation.
3. **Struct In-Place Normalization**: Boxing a value-type struct (`object target = new MyStruct()`) creates a copy. Mutating the boxed struct's properties does not mutate the unboxed caller struct. Callers should normalize class-based DTOs.
