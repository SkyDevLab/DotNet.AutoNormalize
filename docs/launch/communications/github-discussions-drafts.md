# GitHub Discussions Drafts: DotNet.AutoNormalize

> **Note**: For publishing in the repository's GitHub Discussions tab under `Announcements` or `General`.

---

## 1. Project Introduction & Welcome

**Category**: `Announcements`  
**Title**: Welcome to DotNet.AutoNormalize: Consistent Input Canonicalization for .NET

Welcome to the DotNet.AutoNormalize repository!

### Why this project exists
Across .NET backends, developers frequently struggle with representation inconsistencies on user-submitted text data:
- Trimming leading/trailing whitespace.
- Converting empty or whitespace-only strings to `null`.
- Canonicalizing Unicode character sequences into standard Form C.
- Shielding sensitive passwords and API keys from accidental modification.

DotNet.AutoNormalize was built to address these concerns consistently at the boundary — across ASP.NET Core Minimal APIs, MVC controllers, System.Text.Json deserialization, and Entity Framework Core persistence.

### How to get involved
- Browse the [Documentation Index](docs/installation.md).
- Check out runnable samples in the `samples/` directory.
- Open a topic in `Q&A` or `Ideas` if you have suggestions.

---

## 2. Early Architecture & API Feedback Request

**Category**: `Ideas`  
**Title**: RFC: API Design & System.Text.Json TypeInfo Resolver Modifiers

We have published our frozen public API specification in [docs/api-design-proposal.md](docs/api-design-proposal.md) and would love community feedback on:

1. **Service Registration Syntax**:
   ```csharp
   builder.Services.AddAutoNormalize(options =>
   {
       options.TrimStrings = true;
       options.EmptyStringsToNull = true;
       options.UnicodeNormalization = NormalizationForm.FormC;
   });
   ```
2. **Positional Record Constructor Ingress**:
   We use `AutoNormalizeJsonTypeInfoModifier` to normalize parameters prior to constructor invocation. Are there edge cases in custom converters where this might interact with other modifiers?
3. **Sensitive Field Defense**:
   Currently, we provide a default heuristic list (`Password`, `Token`, `ApiKey`, `Secret`, etc.) plus explicit opt-outs (`[DoNotNormalize]`, `[PreserveWhitespace]`).

Please share your thoughts below!

---

## 3. Official Preview Release Announcement (v1.0.0-preview.1)

**Category**: `Announcements`  
**Title**: DotNet.AutoNormalize v1.0.0-preview.1 is now available!

We are pleased to announce the initial preview release of **DotNet.AutoNormalize**!

### What's included:
- **`DotNet.AutoNormalize.Abstractions`**: Core interfaces and declarative attributes.
- **`DotNet.AutoNormalize.Core`**: High-performance engine with reflection caching and cycle-safe traversal.
- **`DotNet.AutoNormalize.Json`**: System.Text.Json contract modifiers.
- **`DotNet.AutoNormalize.AspNetCore`**: Minimal APIs & MVC model binder integration.
- **`DotNet.AutoNormalize.EntityFrameworkCore`**: SaveChanges change tracking interceptor.

Full release notes and package checksums are available in [docs/release-notes-1.0.0-preview.1.md](docs/release-notes-1.0.0-preview.1.md).

Thank you to everyone who contributed to testing and verification!
