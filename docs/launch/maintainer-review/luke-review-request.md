# Maintainer Technical Review Request: AutoNormalize

> **Status**: READY TO SEND  
> **Intended Recipient**: Fellow .NET Open-Source Maintainers (e.g., Luke / .NET Library Authors)  
> **Context**: Seeking candid peer review from experienced .NET open-source maintainers regarding architectural decisions, API contracts, security boundaries, and maintainability. No endorsement, shoutout, or promotion is requested or expected.

---

## 1. Short Direct Message / LinkedIn / Discord

**Subject**: Technical peer review request on AutoNormalize (.NET input canonicalization)

Hi [Name],

Hope you're having a great week!

I’ve been building **AutoNormalize**, an open-source library designed to solve consistent input normalization (trimming, empty-to-null, Unicode Form C) across ASP.NET Core Minimal APIs, MVC, and EF Core without repetitive boilerplate:
`https://github.com/SkyDevLab/DotNet.AutoNormalize`

Knowing your deep experience with high-quality .NET library architecture, I’d be genuinely grateful for your critical review of our core design decisions—specifically:
1. Our use of `System.Text.Json` `IJsonTypeInfoResolver` modifiers to normalize C# positional records *during* deserialization (before constructor invocation), avoiding reflection mutations.
2. Our ASP.NET Core endpoint filter and model binding integration across Minimal APIs and controllers.
3. Our security model (heuristic protection for passwords/tokens paired with explicit `[DoNotNormalize]` / `[PreserveWhitespace]` attributes).
4. Our zero-allocation fast-path design on clean strings (4.83 ns, 0 bytes allocated).

Completely understand if you're busy—no endorsement, tweet, or public shoutout is expected whatsoever; just looking for honest maintainer feedback on our API contracts and edge cases before we tag v1.0.0.

Thanks for your time and all you do for the .NET community!

Best regards,  
Surya Pratap Singh (.NET Foundation Member)
AutoNormalize Maintainers

---

## 2. Detailed Technical Email / Forum Post

**Subject**: Architectural peer review request: AutoNormalize (ASP.NET Core & EF Core Input Canonicalization)

Hi [Name],

I hope this finds you well.

I am reaching out to ask if you would be open to providing some candid technical feedback on an open-source .NET library we have developed called **AutoNormalize**:
`https://github.com/SkyDevLab/DotNet.AutoNormalize`
NuGet Packages: `https://www.nuget.org/packages/AutoNormalize.Core/1.0.0-preview.1`

### Background & Motivation
In many .NET codebases, input normalization (trimming whitespace, empty-to-null conversion, Unicode Form C canonicalization) is either manually written in every controller or handled via global `JsonConverter<string>` implementations. As you know, global string converters often introduce serious edge-case bugs: they can accidentally mangle passwords, cryptographic keys, and tokens, or crash when normalizing duplicate dictionary keys.

### Architectural Decisions We'd Love Your Opinion On:
1. **In-Flight System.Text.Json Deserialization**:
   Instead of post-processing object graphs with reflection, we hook `IJsonTypeInfoResolver.Modifiers` to attach property-scoped converters. For immutable C# positional records (`public record User(string Name, string Email)`), the converter normalizes parameters before constructor binding. We'd love your take on whether this is the cleanest approach for Native AOT and immutable types.

2. **ASP.NET Core Minimal APIs & MVC Pipeline**:
   We provide unified registration via `builder.Services.AddAutoNormalize(...)`, wiring up JSON options, model binders for route/query/form data, and endpoint filters for Minimal APIs. Is there any conflict or pipeline ordering you've observed with other middleware in complex setups?

3. **Security & Boundary Definition**:
   We explicitly document that normalization is representation canonicalization, not validation or HTML sanitization. We built a defense-in-depth heuristic name guard (`Password`, `Token`, `ApiKey`, `Signature`, `Salt`) alongside declarative attributes (`[DoNotNormalize]`, `[PreserveWhitespace]`). We'd appreciate your critique on whether this strikes the right balance between safety and developer ergonomics.

4. **Performance Invariants**:
   Using BenchmarkDotNet v0.14.0 on .NET 9.0, our clean-string fast-path completes in 4.83 ns with **0 bytes allocated** (checking `char.IsWhiteSpace` bounds and `s.IsNormalized(FormC)` before returning the original instance).

### Our Ask
We have prepared a comprehensive architecture RFC and API specification:
- Architecture Review: `https://github.com/SkyDevLab/DotNet.AutoNormalize/blob/main/docs/architecture-review.md`
- API Proposal: `https://github.com/SkyDevLab/DotNet.AutoNormalize/blob/main/docs/api-design-proposal.md`

We want to make sure this library follows the highest standards of the modern .NET ecosystem (comparable to Polly, AutoMapper, or FluentValidation). We are strictly asking for your honest architectural critique and review of our API contracts. We do not expect any promotion, endorsement, or social media mention.

If you have 15–20 minutes to skim the API and share any concerns or red flags you spot, it would be immensely valuable. If your current schedule does not permit this, we completely understand and appreciate your time regardless.

Thank you for your leadership and contributions to the .NET ecosystem.

Warm regards,  
Surya Pratap Singh (.NET Foundation Member)
AutoNormalize Maintainers

---

## 3. Polite Follow-Up Message (Send after 14 days if no response)

**Subject**: Re: Architectural peer review request: AutoNormalize

Hi [Name],

Following up briefly on my previous note regarding AutoNormalize.

I know your schedule is extremely busy, so please don't feel obligated to reply if you haven't had the time. If you do ever have a few minutes to glance over our public API contracts or EF Core interceptor design, we would still value any critique you might have.

Thanks again for all your open-source work!

Best regards,  
Surya Pratap Singh
AutoNormalize Maintainers
