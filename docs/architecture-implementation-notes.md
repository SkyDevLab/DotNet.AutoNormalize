# Architecture Implementation Notes: Phase 3 (Core & Abstractions)

## 1. Implemented Components

The Phase 3 implementation delivers the core independent normalization vertical slice:

```text
DotNet.AutoNormalize/
├── src/
│   ├── DotNet.AutoNormalize.Abstractions/
│   │   ├── INormalizer.cs                         # Generic normalization contract
│   │   ├── IStringNormalizer.cs                   # Specialized string normalization contract
│   │   ├── IAutoNormalizer.cs                     # Entry-point for object graph normalization
│   │   ├── NormalizationContext.cs                # Metadata context (PropertyInfo, TargetType, Services)
│   │   └── Attributes/
│   │       ├── PreserveWhitespaceAttribute.cs     # Whitespace preservation opt-out
│   │       ├── DoNotNormalizeAttribute.cs         # Complete normalization exclusion
│   │       └── NormalizeWithAttribute.cs          # Custom normalizer specification
│   │
│   └── DotNet.AutoNormalize.Core/
│       ├── Configuration/
│       │   ├── AutoNormalizeOptions.cs            # Validated configuration options
│       │   ├── WhitespaceTrimMode.cs              # Both, Start, End enum
│       │   └── CasingTransform.cs                 # None, LowerCase, UpperCase enum
│       ├── Normalizers/
│       │   ├── TrimNormalizer.cs                  # Fast-path whitespace trimming
│       │   ├── EmptyStringToNullNormalizer.cs     # Empty/whitespace string to null
│       │   ├── NullToEmptyStringNormalizer.cs     # Null to empty string conversion
│       │   ├── UnicodeNormalizer.cs               # Unicode FormC/D/KC/KD with IsNormalized check
│       │   └── CasingNormalizer.cs                # Invariant casing transformations
│       └── Engine/
│           ├── StringNormalizerPipeline.cs        # Orchestrated string rule pipeline
│           ├── PropertyNormalizationMetadata.cs   # Property reflection analysis & attribute cache
│           ├── TypeNormalizationMetadata.cs       # Type-level property registry
│           ├── NormalizationMetadataCache.cs      # Thread-safe ConcurrentDictionary cache
│           ├── ObjectGraphNormalizer.cs           # Cycle-safe, depth-bounded recursive visitor
│           └── AutoNormalizer.cs                  # IAutoNormalizer implementation
│
└── tests/
    └── DotNet.AutoNormalize.Core.Tests/           # 35 comprehensive xUnit tests
```

---

## 2. Fast-Path Optimization Mechanics

To eliminate unnecessary heap allocations and CPU cycles, the core engine implements three independent fast paths:

### 2.1 Fast-Path 1: Already-Trimmed Strings
Before invoking `string.Trim()`, `TrimStart()`, or `TrimEnd()`, `TrimWithFastPath` inspects the leading and trailing characters:
```csharp
if (!char.IsWhiteSpace(input[0]) && !char.IsWhiteSpace(input[input.Length - 1]))
{
    return input; // No whitespace characters present; zero allocations
}
```
If no whitespace exists at the target boundary, the original string reference is immediately returned without allocating a new substring.

### 2.2 Fast-Path 2: Already-Normalized Unicode Strings
Calling `string.Normalize(NormalizationForm)` can allocate a new string even if the string is already in the target form. We guard the call with `string.IsNormalized()`:
```csharp
if (!current.IsNormalized(form))
{
    current = current.Normalize(form);
}
```
If the string already conforms to Form C (the overwhelming majority of standard Latin/ASCII inputs), no memory allocation or character table traversal occurs.

### 2.3 Fast-Path 3: Unchanged Strings
When evaluating object properties, if the normalized string reference is identical (`ReferenceEquals`) or has identical content, `PropertyInfo.SetValue` is skipped:
```csharp
if (!ReferenceEquals(currentVal, normalizedVal) && currentVal != normalizedVal)
{
    propMeta.PropertyInfo.SetValue(target, normalizedVal);
}
```

---

## 3. Immutability, C# Records & Init-Only Properties

C# 9+ positional records and types with `init;` properties cannot be modified after instantiation:
* `PropertyNormalizationMetadata` inspects the setter return parameter modifiers for `System.Runtime.CompilerServices.IsExternalInit`.
* If `IsInitOnly` is true, `CanWrite` is set to `false`.
* The in-memory visitor (`ObjectGraphNormalizer`) detects `CanWrite == false` and safely skips mutating the property. It does not throw exceptions or attempt illegal reflection writes.
* *(Note: In Phase 4, JSON deserialization integrates via `IJsonTypeInfoResolver`, populating records and init-only properties directly with normalized values at constructor invocation time).*

---

## 4. Graph Safety & Cycle Protection

Real-world domain models frequently contain circular references (e.g. `User.Manager.Employees` or self-referential tree nodes).
1. **Cycle Detection**: The visitor maintains an execution-scoped `HashSet<object>(ReferenceEqualityComparer.Instance)`. Any object reference encountered more than once within the same traversal call aborts further recursion along that path.
2. **Recursion Depth Guard**: A configurable `MaxRecursionDepth` (default: 32) prevents stack overflow crashes on maliciously deeply nested hierarchies.
3. **Thread Safety**: The metadata cache (`NormalizationMetadataCache`) uses `ConcurrentDictionary<Type, TypeNormalizationMetadata>`, while traversal state (`HashSet<object>`) is strictly call-scoped on the stack to guarantee zero concurrency race conditions.
