# Benchmark Audit: DotNet.AutoNormalize

## 1. Investigation of the 0.0077 ns Anomaly

### 1.1 Root Cause Analysis
In the initial benchmark run documented in Phase 5, `CleanString_Manual` reported an impossibly low duration of **0.0077 ns** (~7.7 picoseconds):

```csharp
// Problematic implementation in initial benchmark:
private const string CleanString = "CleanStringWithoutWhitespace";

[Benchmark(Baseline = true)]
public string CleanString_Manual() => CleanString.Trim();
```

At 0.0077 ns per operation on a ~4.0 GHz CPU (clock cycle ~0.25 ns), an operation taking 0.0077 ns would execute in **0.03 CPU cycles**. This is physically impossible for instruction execution and indicates **JIT constant-folding and dead-code elimination**.

Because `CleanString` was a compile-time constant literal, RyuJIT evaluated the deterministic string characteristics or eliminated the method call during loop optimization, leaving only an unrolled empty benchmark loop.

### 1.2 Remediated Benchmark Design
To guarantee that the RyuJIT compiler and AVX2 vector intrinsics cannot precompute or eliminate the method invocation:
1. Benchmark strings are allocated at runtime in `[GlobalSetup]` via non-constant concatenation:
   ```csharp
   _cleanString = string.Concat("CleanStringWithoutWhitespace_", "RuntimeAllocated");
   _dirtyString = string.Concat("   ", "Dirty string with leading and trailing spaces", "   ");
   _whitespaceOnlyString = string.Concat("     ", "\t\r\n     ", "    ");
   _decomposedUnicode = string.Concat("Caf", "\u0065\u0301"); // Café decomposed
   ```
2. Methods return the string reference to BenchmarkDotNet's consumer harness to prevent dead-code stripping.
3. MemoryDiagnoser is attached to inspect managed heap allocations.

---

## 2. Audited Benchmark Results

### 2.1 Benchmark Environment
* **BenchmarkDotNet**: v0.14.0
* **OS**: Windows 11 (10.0.26300.9711)
* **Host & JIT**: .NET SDK 9.0.314, .NET 9.0.16 (RyuJIT AVX2, X64)
* **Garbage Collector**: Concurrent Workstation
* **Hardware Intrinsics**: AVX2, AES, BMI1, BMI2, FMA, LZCNT, PCLMUL, POPCNT, AvxVnni, SERIALIZE

---

### 2.2 Scalar String Operations (`ScalarStringBenchmarks`)

| Method | Mean | Error | StdDev | Ratio | RatioSD | Allocated | Alloc Ratio |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **`CleanString_ManualTrim`** | 1.684 ns | 0.043 ns | 0.040 ns | 1.00 | 0.00 | **0 B** | 1.00 |
| **`CleanString_AutoNormalize`** | **4.830 ns** | 0.089 ns | 0.083 ns | 2.87 | 0.08 | **0 B** | **1.00** |
| **`DirtyString_ManualTrim`** | 14.504 ns | 0.171 ns | 0.151 ns | 8.61 | 0.23 | 112 B | 1.00 |
| **`DirtyString_AutoNormalize`** | 17.356 ns | 0.252 ns | 0.236 ns | 10.31 | 0.29 | 112 B | 1.00 |
| **`WhitespaceOnly_Manual`** | 11.238 ns | 0.141 ns | 0.132 ns | 6.67 | 0.18 | **0 B** | 1.00 |
| **`WhitespaceOnly_AutoNormalize`** | 12.497 ns | 0.162 ns | 0.151 ns | 7.42 | 0.20 | **0 B** | 1.00 |
| **`DecomposedUnicode_Manual`** | 135.210 ns | 1.842 ns | 1.723 ns | 80.29 | 2.21 | 32 B | 1.00 |
| **`DecomposedUnicode_AutoNormalize`**| 136.369 ns | 2.011 ns | 1.881 ns | 80.98 | 2.24 | 32 B | 1.00 |

#### Insights:
* **Zero-Allocation Invariant Confirmed**: `CleanString_AutoNormalize` and `WhitespaceOnly_AutoNormalize` allocate **0 bytes**.
* **Overhead**: On clean strings, the entire pipeline (options check + trimming bounds verification + Unicode Form C verification) adds **3.14 nanoseconds** over raw span-based `Trim()`.
* **Dirty Strings**: Trimming + empty-to-null + Unicode canonicalization adds less than **3 nanoseconds** over a manual `.Trim()` check (17.35 ns vs 14.50 ns), with identical 112 B string allocation.

---

### 2.3 Object Graph & DTO Normalization (`DtoAndObjectGraphBenchmarks`)

| Method | Mean | Error | StdDev | Gen0 | Allocated |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **`SingleProperty_AutoNormalize`** | 86.41 ns | 1.107 ns | 0.924 ns | 0.0650 | 408 B |
| **`TypicalDto_AutoNormalize`** (6 properties) | 283.55 ns | 4.788 ns | 3.998 ns | 0.1450 | 912 B |
| **`NestedObject_AutoNormalize`** (3 levels) | 575.78 ns | 2.219 ns | 2.076 ns | 0.2460 | 1,544 B |
| **`Collection_AutoNormalize`** (50 items) | 2,590.02 ns | 51.723 ns | 87.830 ns | 1.3008 | 8,176 B |

#### Insights:
* Metadata caching (`NormalizationMetadataCache`) ensures that property reflection, attribute scanning, and delegate bindings occur once per type. Subsequent graph normalizations execute at sub-microsecond speeds.
* A typical 6-property user DTO normalizes in **283 nanoseconds**.
* A complex nested hierarchy normalizes in **575 nanoseconds**.

---

### 2.4 JSON Deserialization Boundary (`JsonDeserializationBenchmarks`)

| Method | Mean | Error | StdDev | Ratio | Gen0 | Allocated | Alloc Ratio |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **`Deserialize_DefaultSystemTextJson`** (Baseline) | 388.1 ns | 2.77 ns | 2.45 ns | 1.00 | 0.0634 | 400 B | 1.00 |
| **`Deserialize_AutoNormalizeSystemTextJson`** | 601.9 ns | 12.04 ns | 16.07 ns | 1.55 | 0.1516 | 952 B | 2.38 |

#### Insights:
* Normalizing strings *at deserialization time* (`AutoNormalizeJsonTypeInfoModifier` + property-level `JsonConverter<string>`) incurs only **213.8 nanoseconds** overhead for a 6-property payload.
* Normalizes positional records at constructor parameter binding time with **zero post-construction reflection overhead**.

---

### 2.5 Entity Framework Core Persistence Boundary (`EfCorePersistenceBenchmarks`)

| Method | Mean | Error | StdDev | Ratio | Gen0 | Gen1 | Allocated | Alloc Ratio |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **`SaveChanges_DefaultEfCore`** (Baseline) | 12.36 µs | 0.241 µs | 0.330 µs | 1.00 | 3.4790 | 0.8545 | 21.33 KB | 1.00 |
| **`SaveChanges_WithAutoNormalize`** | 22.23 µs | 0.429 µs | 0.557 µs | 1.80 | 5.0049 | 0.9766 | 30.95 KB | 1.45 |

#### Insights:
* `AutoNormalizeSaveChangesInterceptor` inspects only `EntityEntry` instances in `EntityState.Added` and `EntityState.Modified`.
* Entity Framework Core `ChangeTracker` traversal and entity normalization adds **9.87 microseconds** per save operation, which is negligible compared to typical database network roundtrips (1–10 milliseconds).
