# Discord Community Communication Drafts: DotNet.AutoNormalize

> **Guidelines**: Post strictly in designated showcase/community-projects or architecture channels (e.g. `#dotnet`, `#architecture`, `#community-showcase`). Never tag `@everyone` or `@here`. Respect community rules against spam.

---

## 1. Pre-Launch Architecture Feedback Request

**Channel Recommendation**: `#architecture` or `#code-review`

> Hey everyone! I've been designing an open-source library for consistent input normalization in .NET (handling trimming, empty-to-null, and Unicode Form C canonicalization across Minimal APIs, MVC, and EF Core).
> 
> Specifically looking for feedback on our approach to immutable C# positional records: rather than using unsafe reflection post-construction, we hook `System.Text.Json` contract resolvers (`IJsonTypeInfoResolver`) to normalize constructor parameters *during* token reading.
> 
> If anyone interested in STJ modifiers or model binding internals has a moment to check out our API design proposal, here is the repo:
> `https://github.com/SkyDevLab/DotNet.AutoNormalize/blob/main/docs/api-design-proposal.md`
> 
> Would really appreciate honest critique on the abstraction design!

---

## 2. Post-Launch Project Announcement

**Channel Recommendation**: `#community-projects` or `#showcase`

> Hey folks! Just released the first preview of **DotNet.AutoNormalize** — an open-source library for automatic, safe input canonicalization in ASP.NET Core and EF Core.
> 
> **Why it exists**: Instead of writing repetitive `.Trim()` calls or risking global JSON converters that accidentally mangle passwords, it provides:
> - Zero-allocation fast paths on clean strings (4.8 ns, 0 B allocated)
> - Seamless support for C# positional records in System.Text.Json
> - Automatic defense for sensitive property names (`Password`, `ApiKey`, `Token`)
> - MVC model binders & EF Core `SaveChangesInterceptor`
> 
> GitHub: `https://github.com/SkyDevLab/DotNet.AutoNormalize`  
> NuGet: `DotNet.AutoNormalize.AspNetCore` (v1.0.0-preview.1)
> 
> MIT licensed. All feedback, bug reports, and discussions are very welcome!

---

## 3. Technical Discussion Starter

**Channel Recommendation**: `#dotnet` or `#performance`

> Question for the group: How do you handle string canonicalization on HTTP inputs in high-throughput services?
> 
> We recently benchmarked raw `string.Trim()` vs. a fast-path inspection pipeline in .NET 9. By checking `!char.IsWhiteSpace(s[0]) && !char.IsWhiteSpace(s[^1])` and `s.IsNormalized(NormalizationForm.FormC)`, we were able to achieve **0 managed heap allocations** and ~4.8 ns total execution on clean strings.
> 
> Curious if others have measured the trade-off of pre-checking bounds vs. letting `Trim()` allocate a new substring when strings are mostly clean in production traffic?
