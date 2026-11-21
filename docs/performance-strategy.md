# Performance Strategy & Benchmark Plan: DotNet.AutoNormalize

## 1. Performance Mandate

In high-throughput ASP.NET Core web services processing tens of thousands of requests per second, input normalization must not become a bottleneck.

Our core performance tenets are:
1. **Zero-Allocation on Clean Inputs**: If an input string is already trimmed and in Unicode Form C, `DotNet.AutoNormalize` must return the original string reference without allocating a single byte on the managed heap.
2. **Minimal Per-Request Overhead**: Execution overhead must be measured in sub-microseconds for typical DTO payloads.
3. **No Speculative Optimization**: Do not implement complex dynamic expression compilation or custom IL emitters until backed by empirical BenchmarkDotNet measurements.

---

## 2. Ingress Path Optimization Strategy

### 2.1 The 95% Path: System.Text.Json Modifier
In modern ASP.NET Core applications (.NET 8 & .NET 9), the vast majority of incoming data arrives as JSON payloads.
* Instead of deserializing an object and then traversing its properties with reflection, `DotNet.AutoNormalize.Json` intercepts property values **during deserialization** using `IJsonTypeInfoResolver`.
* In `System.Text.Json`, property setters are already optimized and emitted by the runtime.
* The normalizer wraps the property `Set` delegate once during type metadata initialization (cold path).
* During request processing (hot path), values are normalized in-flight with zero additional reflection calls.

```csharp
// Executed once during contract resolution:
property.Set = (obj, value) => originalSet(obj, normalizer.Normalize((string?)value, context));
```

### 2.2 The In-Memory Traversal Path: Reflection vs. Caching vs. Compiled Delegates

For non-JSON scenarios (e.g. normalizing an in-memory object graph via `IAutoNormalizer.Normalize(target)`), we must evaluate three implementation tiers:

| Tier | Technique | Pros | Cons | Initial Decision |
| :--- | :--- | :--- | :--- | :--- |
| **Tier 1** | Pure Uncached Reflection | Simple, zero startup overhead | Prohibitive overhead (`PropertyInfo.GetValue/SetValue` on every invocation) | **Rejected** |
| **Tier 2** | Cached Property Metadata | Fast, thread-safe, Native AOT compatible | Minor reflection dispatch overhead | **Adopt as Baseline** |
| **Tier 3** | Dynamic Expression Compilation (`Expression.Compile()`) | Native method call speed | High cold-start startup cost, incompatible with Native AOT | **Evaluate via Benchmarks** |

> [!IMPORTANT]
> In accordance with our performance mandate, we will implement **Tier 2 (Cached Property Metadata)** as the baseline and only introduce Tier 3 if BenchmarkDotNet demonstrates a statistically significant throughput advantage that justifies the complexity.

---

## 3. Allocation-Elimination Mechanics

### 3.1 String Immutability and Span Optimization
Strings in .NET are immutable. Standard `.Trim()` allocates a new string substring even if only a single character changed. However, if no characters need trimming, `string.Trim()` returns the original instance:
```csharp
public static string? NormalizeString(string? value, AutoNormalizeOptions options)
{
    if (value is null) return null;
    if (value.Length == 0) return options.EmptyStringsToNull ? null : value;

    // Fast-path: Check if trimming is needed
    string result = value;
    if (options.TrimStrings)
    {
        result = options.TrimMode switch
        {
            WhitespaceTrimMode.Start => result.TrimStart(),
            WhitespaceTrimMode.End => result.TrimEnd(),
            _ => result.Trim()
        };
    }

    if (result.Length == 0 && options.EmptyStringsToNull)
    {
        return null;
    }

    // Fast-path: Check Unicode normalization form before normalizing
    if (options.UnicodeNormalization.HasValue && 
        !result.IsNormalized(options.UnicodeNormalization.Value))
    {
        result = result.Normalize(options.UnicodeNormalization.Value);
    }

    return result;
}
```

**Key Optimization**:
* `result.IsNormalized(NormalizationForm.FormC)` performs a non-allocating check. If the string is already normalized, `Normalize()` is skipped entirely, saving heap allocations.

### 3.2 Reusable / Scoped Normalization Context
* Rather than allocating a new `NormalizationContext` per property, a shared or pooled context can be used during graph traversal.
* Context properties (`PropertyInfo`, `TargetType`) are set on the traversal stack or lightweight struct wrappers where feasible.

---

## 4. BenchmarkDotNet Suite Design

We will establish a dedicated benchmark project: `benchmarks/DotNet.AutoNormalize.Benchmarks`.

### 4.1 Benchmark Scenarios

1. **Scalar String Normalization**:
   * Clean string (already trimmed, FormC).
   * Dirty string (leading/trailing spaces, decomposed Unicode).
   * Whitespace-only string (`"   "`).
   * Target: Zero heap allocations on clean inputs.

2. **JSON Deserialization Overhead**:
   * Deserializing a standard DTO (10 properties: 8 strings, 2 numbers) with `System.Text.Json` default.
   * Deserializing the same DTO with `DotNet.AutoNormalize.Json` active.
   * Target: < 5% CPU overhead compared to raw `System.Text.Json`.

3. **In-Memory Object Graph Traversal**:
   * Baseline: Manual hand-coded trimming (`dto.Name = dto.Name?.Trim(); ...`).
   * Tier 2: Cached Property Metadata.
   * Tier 3: Compiled Expression Trees.
   * Target: Quantify whether Tier 3 provides meaningful benefit over Tier 2.

4. **Deep Graph & Collection Scaling**:
   * DTO with nested child objects and a list of 100 items.
   * Scaling factor comparison across object graph depths (1, 5, 10 levels).
