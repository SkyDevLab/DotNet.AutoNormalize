# BenchmarkDotNet Performance Report: DotNet.AutoNormalize

## 1. Environment & Configuration

* **BenchmarkDotNet**: v0.14.0
* **Operating System**: Windows 11 (10.0.26300.9711)
* **Processor / Intrinsics**: X64 RyuJIT AVX2, AES, BMI1, BMI2, FMA, LZCNT, PCLMUL, POPCNT, AvxVnni, SERIALIZE
* **.NET SDK**: 9.0.314
* **Runtime**: .NET 9.0.16 (9.0.1626.22923)
* **GC**: Concurrent Workstation
* **Diagnostics**: `[MemoryDiagnoser]` enabled

---

## 2. Empirical Benchmark Measurements

### 2.1 Scalar String Normalization (`ScalarStringBenchmarks`)

| Method | Mean | Error | StdDev | Ratio | Allocated | Allocation Ratio |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **`CleanString_ManualTrim`** | 1.684 ns | 0.043 ns | 0.040 ns | 1.00 | **0 B** | Baseline |
| **`CleanString_AutoNormalize`** | **4.830 ns** | 0.089 ns | 0.083 ns | 2.87 | **0 B** | **Zero Allocation** |
| **`DirtyString_ManualTrim`** | 14.504 ns | 0.171 ns | 0.151 ns | 8.61 | 112 B | 1.00 |
| **`DirtyString_AutoNormalize`** | 17.356 ns | 0.252 ns | 0.236 ns | 10.31 | 112 B | 1.00 |
| **`WhitespaceOnly_Manual`** | 11.238 ns | 0.141 ns | 0.132 ns | 6.67 | **0 B** | 1.00 |
| **`WhitespaceOnly_AutoNormalize`** | 12.497 ns | 0.162 ns | 0.151 ns | 7.42 | **0 B** | 1.00 |
| **`DecomposedUnicode_Manual`** | 135.210 ns | 1.842 ns | 1.723 ns | 80.29 | 32 B | 1.00 |
| **`DecomposedUnicode_AutoNormalize`**| 136.369 ns | 2.011 ns | 1.881 ns | 80.98 | 32 B | 1.00 |

### 2.2 DTO and Object Graph Normalization (`DtoAndObjectGraphBenchmarks`)

| Method | Mean | Error | StdDev | Gen0 | Allocated |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **`SingleProperty_AutoNormalize`** | 86.41 ns | 1.107 ns | 0.924 ns | 0.0650 | 408 B |
| **`TypicalDto_AutoNormalize`** (6 properties) | 283.55 ns | 4.788 ns | 3.998 ns | 0.1450 | 912 B |
| **`NestedObject_AutoNormalize`** (Hierarchy) | 575.78 ns | 2.219 ns | 2.076 ns | 0.2460 | 1,544 B |
| **`Collection_AutoNormalize`** (50 items) | 2,590.02 ns | 51.723 ns | 87.830 ns | 1.3008 | 8,176 B |

### 2.3 JSON Deserialization Boundary (`JsonDeserializationBenchmarks`)

| Method | Mean | Error | StdDev | Ratio | Gen0 | Allocated | Alloc Ratio |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **`Deserialize_DefaultSystemTextJson`** | 388.1 ns | 2.77 ns | 2.45 ns | 1.00 | 0.0634 | 400 B | 1.00 |
| **`Deserialize_AutoNormalizeSystemTextJson`** | 601.9 ns | 12.04 ns | 16.07 ns | 1.55 | 0.1516 | 952 B | 2.38 |

### 2.4 EF Core Persistence Boundary (`EfCorePersistenceBenchmarks`)

| Method | Mean | Error | StdDev | Ratio | Gen0 | Gen1 | Allocated | Alloc Ratio |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **`SaveChanges_DefaultEfCore`** | 12.36 µs | 0.241 µs | 0.330 µs | 1.00 | 3.4790 | 0.8545 | 21.33 KB | 1.00 |
| **`SaveChanges_WithAutoNormalize`** | 22.23 µs | 0.429 µs | 0.557 µs | 1.80 | 5.0049 | 0.9766 | 30.95 KB | 1.45 |

---

## 3. Key Architectural Validations

1. **Zero-Allocation for Clean Strings**:
   When strings are already clean, `DotNet.AutoNormalize` returns the original string instance with **0 bytes allocated**. The inspection logic adds only 3.14 nanoseconds.
2. **Elimination of Constant Folding Anomalies**:
   Microbenchmarks use non-constant runtime inputs, eliminating RyuJIT dead-code stripping.
3. **Constructor-Time Record Normalization**:
   Position records deserialize and normalize in 601.9 ns, avoiding reflection mutation of immutable objects.
4. **Negligible EF Core Overhead**:
   Under 10 microseconds added to `SaveChanges` operations, orders of magnitude below database network I/O.
