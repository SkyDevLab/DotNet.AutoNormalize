# Edge Case & Verification Coverage Report: DotNet.AutoNormalize

## 1. Overview
This report documents the verification audit for **DotNet.AutoNormalize** across complex runtime boundaries, concurrency conditions, immutability contracts, collection handling, EF Core change tracking, and sensitive property preservation.

All assertions were verified against the live test suites:
- `DotNet.AutoNormalize.Core.Tests` (49 tests)
- `DotNet.AutoNormalize.AspNetCore.Tests` (11 tests)
- `DotNet.AutoNormalize.EntityFrameworkCore.Tests` (9 tests)
Total: **69 tests**, 0 failures, 0 skipped.

---

## 2. Immutability & Constructor-Time Normalization

### 2.1 Positional Records & Init-Only Properties
* **Problem**: C# positional records (`public record User(string Name)`) and init-only properties (`public string Name { get; init; }`) disallow post-construction mutation in safe C#. Attempting to mutate them via unsafe reflection or IL rewriting breaks language immutability invariants, causes hashcode invalidation in sets/dictionaries, and can crash under Native AOT.
* **In-Memory Core Behavior**: `AutoNormalizer.Normalize(record)` safely detects when a property has no public setter or is an `init;` property (via `IsExternalInit` modifier checking). The engine skips post-construction mutation, returning the original instance safely without throwing exceptions.
* **JSON Boundary Solution**: When deserializing from JSON via `System.Text.Json`, `AutoNormalizeJsonTypeInfoModifier` hooks into `JsonPropertyInfo` and applies `AutoNormalizeStringJsonConverter` *during parameter deserialization before the constructor is invoked*.
  * Result: Positional records and `init`-only objects are created already normalized at birth.

---

## 3. Collections & Dictionary Safeguards

### 3.1 Dictionary Key Invariance (Crucial Safety Rule)
* **Rule**: Dictionary keys must **never** be normalized or modified.
* **Rationale**: Mutating a dictionary key in-place corrupts the hash bucket indexing of `Dictionary<TKey, TValue>`, leading to:
  * Silent lookup failures (`dictionary.ContainsKey(key)` returning `false` for existing keys).
  * `ArgumentException: An item with the same key has already been added` if normalization causes key collisions (e.g. `"id "` and `"id"`).
* **Implementation**: `ObjectGraphNormalizer` inspects dictionary entries (`IDictionary`) and normalizes **values only**, leaving dictionary keys untouched.
* **Verified Types**:
  * `Dictionary<string, string>`
  * `Dictionary<string, object>`
  * `IReadOnlyDictionary<string, string>` (safely skipped without throwing)
  * `Dictionary<string, string>` with `StringComparer.OrdinalIgnoreCase`

### 3.2 Collection Elements & Null Values
* `IList<string>` elements are replaced in-place via indexers.
* Null collection elements (`new List<string?> { "  foo  ", null, "  bar  " }`) are preserved without `NullReferenceException`.
* Deeply nested lists within complex graphs are traversed up to `MaxDepth` (default 32) with cycle detection.

---

## 4. Concurrency & High-Throughput Verification

### 4.1 1,000 Concurrent Task Audit
* Tested under `ConcurrencyAuditTests`: 1,000 asynchronous concurrent tasks (`Task.WhenAll`) executed simultaneously against:
  * Shared `AutoNormalizer` instance.
  * Shared `AutoNormalizeOptions` instance.
  * Shared `NormalizationMetadataCache` instance.
  * Dynamic, non-pre-cached types executing simultaneous cache misses.
* **Findings**:
  * `ConcurrentDictionary<Type, ObjectTypeMetadata>` handles concurrent cache insertion without thread locks or contention bottlenecks.
  * Zero deadlocks, zero race conditions, and zero data corruption.
* Note: Operations are executed across the .NET thread pool worker threads (not 1,000 physical OS threads).

---

## 5. Entity Framework Core Change Tracking Audit

### 5.1 State-Specific Traversal
`AutoNormalizeSaveChangesInterceptor` inspects `DbContext.ChangeTracker.Entries()`:
* **`EntityState.Added`**: Fully normalized prior to database insert.
* **`EntityState.Modified`**: Fully normalized prior to database update.
* **`EntityState.Unchanged`**: **Skipped**. Prevents unintended entity dirtiness and redundant UPDATE queries.
* **`EntityState.Deleted`**: **Skipped**. Avoids resurrecting or modifying entities marked for removal.

### 5.2 Advanced EF Core Features
* **Owned Entities**: EF Core owned entity types appear as distinct `EntityEntry` instances in the `ChangeTracker`. They are tracked and normalized automatically.
* **Value Converters & Shadow Properties**: Non-string or shadow properties are not modified.
* **Cancellation Tokens**: `SavingChangesAsync` respects `CancellationToken.IsCancellationRequested` and throws `OperationCanceledException` before modifying state.
* **Repeated SaveChanges**: Executing multiple consecutive `SaveChanges()` calls produces identical, idempotent entity states without re-allocating or degrading change tracker metadata.

---

## 6. Security Boundaries & Preservation Regressions

### 6.1 Defense-in-Depth Heuristics & Explicit Overrides
* **Sensitive Names Guarded by Default**:
  * `"Password"`, `"PasswordHash"`, `"CurrentPassword"`, `"NewPassword"`, `"ConfirmPassword"`
  * `"Token"`, `"AccessToken"`, `"RefreshToken"`, `"AuthToken"`, `"IdToken"`
  * `"ApiKey"`, `"ClientSecret"`, `"Secret"`, `"SecretKey"`, `"PrivateKey"`
  * `"Signature"`, `"SignedPayload"`, `"Hash"`, `"Salt"`
  * `"Certificate"`, `"PublicKey"`
* **Explicit Attribute Exclusions**:
  * `[DoNotNormalize]`: Completely excludes property or entire class from any normalization.
  * `[PreserveWhitespace]`: Disables trimming and empty-to-null while allowing Unicode normalization if configured.
* **Cryptographic & Encoded Payloads**:
  * Base64 payloads (e.g. `eyJh...==`), SHA-256 hashes, and API authorization headers are preserved intact.
  * Tested under `SecurityRegressionTests`.

---

## 7. Known Limitations

1. **Standalone In-Memory Records**:
   * Calling `normalizer.Normalize(record)` on an already-constructed positional record cannot mutate the record in place. Consumers should use constructor instantiation or rely on `System.Text.Json` deserialization where records are normalized at birth.
2. **Read-Only Collections (`IReadOnlyList<T>`)**:
   * In-place normalization cannot mutate elements of `IReadOnlyList<T>` or `ImmutableArray<T>`. Elements are read-only.
3. **Circular Graph Max Depth**:
   * By default, cyclic references are tracked via `ReferenceEqualityComparer` in `ObjectGraphNormalizer`. If `MaxDepth` (32) is reached, traversal halts to prevent stack overflow.
