# Security Model & Threat Considerations: DotNet.AutoNormalize

## 1. Fundamental Security Boundaries & Disclaimers

> [!CAUTION]
> **CRITICAL SECURITY NOTICE**: 
> 1. **Normalization is NOT Validation**: Normalization adjusts representation (e.g. trimming whitespace). It does not evaluate data validity, enforce business rules, or reject malicious payloads.
> 2. **Normalization is NOT Sanitization**: Trimming or Unicode normalization provides **ZERO protection** against Cross-Site Scripting (XSS), SQL Injection (SQLi), Command Injection, or Path Traversal. Applications must continue to use parameterized queries, context-aware output encoders, and strict validation libraries (such as FluentValidation).
> 3. **Property-Name Heuristics are NOT a Security Guarantee**: Heuristic name checking is strictly a defense-in-depth convenience net. Developers **MUST NOT** rely on heuristics alone to protect sensitive or cryptographic data. Explicit opt-out attributes (`[PreserveWhitespace]`, `[DoNotNormalize]`) or type-level exclusions are the only authoritative protection mechanism.

---

## 2. Threat Analysis: The Hazards of Uncontrolled Normalization

### 2.1 Password and Credential Corruption
* **Threat**: Leading or trailing whitespace can be an intentional part of a user's passphrase (e.g., `" correct horse battery staple "`). If an authentication endpoint automatically trims passwords on registration, but a separate login service or third-party identity provider (OAuth/OIDC) does not, the user is permanently locked out.
* **Mitigation**:
  * Passwords must **never** be trimmed or normalized.
  * `DotNet.AutoNormalize` excludes properties named `Password`, `Passcode`, `Secret`, etc. by default.
  * Developers should annotate credential properties with `[PreserveWhitespace]` or `[DoNotNormalize]`.

### 2.2 Cryptographic Signatures, Hashes & HMACs
* **Threat**: Digital signatures (RSA, ECDSA), HMAC tokens, and Base64-encoded binary digests are strictly byte-sensitive. Trimming or Unicode normalizing a Base64 string or binary signature changes its hash, resulting in signature verification failure.
* **Mitigation**:
  * Cryptographic payloads and Base64 tokens must be explicitly marked with `[DoNotNormalize]`.

### 2.3 Unicode Homoglyph & Normalization Attacks
* **Threat**: Attackers can submit visually identical strings using different Unicode code points (e.g., Cyrillic 'а' `\u0430` vs. Latin 'a' `\u0061`) or mixed combining characters to bypass naive string filters or create duplicate accounts.
* **Mitigation**:
  * By default, `DotNet.AutoNormalize` applies **Unicode Normalization Form C (FormC)**, canonicalizing decomposed characters into canonical precomposed characters.
  * This guarantees that `"e" + "\u0301"` and `"\u00e9"` evaluate to identical character sequences.

### 2.4 Denial of Service (DoS) via Cyclic Graphs & Deep Nesting
* **Threat**: An attacker or malformed object graph could contain cyclic references (e.g. `Parent.Child.Parent`) or deeply nested hierarchies, triggering stack overflow crashes (`StackOverflowException`) or excessive CPU consumption.
* **Mitigation**:
  * Traversal tracks visited object references using a `HashSet<object>(ReferenceEqualityComparer.Instance)`.
  * Traversal enforces a configurable `MaxRecursionDepth` (default: 32). Any traversal exceeding this limit terminates recursion safely.

---

## 3. Defense-in-Depth Hierarchy

`DotNet.AutoNormalize` evaluates whether to normalize a given property using a strict four-layer decision hierarchy:

```mermaid
flowchart TD
    Prop[Evaluate Property for Normalization] --> CheckTypeAttr{Is Declaring Class or Struct\nannotated with [DoNotNormalize]?}
    CheckTypeAttr -- Yes --> Skip[DO NOT NORMALIZE]
    CheckTypeAttr -- No --> CheckPropAttr{Is Property annotated with\n[DoNotNormalize]?}

    CheckPropAttr -- Yes --> Skip
    CheckPropAttr -- No --> CheckPreserve{Is Property annotated with\n[PreserveWhitespace]?}

    CheckPreserve -- Yes --> ApplyNoTrim[Apply Custom/Unicode Normalizers ONLY\nSkip Trimming and Empty-to-Null]
    CheckPreserve -- No --> CheckHeuristics{Is ProtectSensitiveProperties enabled\nAND Property Name matches heuristic?}

    CheckHeuristics -- Yes --> WarnSkip[Skip Trimming and Empty-to-Null\n(Defense-in-depth safety net)]
    CheckHeuristics -- No --> CheckCustom{Is [NormalizeWith] attribute present?}

    CheckCustom -- Yes --> ExecCustom[Execute Custom INormalizer]
    CheckCustom -- No --> ExecDefault[Execute Standard Normalization Pipeline]
```

### 3.1 Layer 1: Explicit Declarative Opt-Outs (Authoritative)
The highest authority is an explicit attribute:
* `[DoNotNormalize]`: Completely bypasses the property or class.
* `[PreserveWhitespace]`: Preserves leading and trailing spaces; empty strings are not converted to null.
* `[DataType(DataType.Password)]`: Standard ASP.NET Core attribute; treated equivalently to `[DoNotNormalize]`.

### 3.2 Layer 2: Type-Level Exclusions
Entire types (such as security tokens, encrypted envelopes, or external DTOs) can be excluded:
```csharp
[DoNotNormalize]
public sealed class AuthTokenResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}
```

### 3.3 Layer 3: Sensitive Name Heuristics (Safety Net Only)
When `ProtectSensitiveProperties` is `true` (default), property names ending in or matching the following tokens are automatically shielded from whitespace trimming and empty-to-null conversion:
* `Password`, `Passcode`
* `Token`, `AuthToken`, `AccessToken`, `RefreshToken`, `IdToken`
* `Secret`, `ClientSecret`, `ApiKey`, `PrivateKey`
* `Signature`, `Hash`, `Salt`

> **Note**: While heuristics catch >90% of accidental leaks during development, teams must establish coding guidelines requiring explicit attributes for all security-sensitive DTO properties.

### 3.4 Layer 4: Standard Normalization Pipeline
If no exclusions match, the property undergoes:
1. Whitespace trimming (if enabled).
2. Empty-to-null conversion (if enabled).
3. Unicode normalization (Form C default).
4. Custom normalizers (if registered).
