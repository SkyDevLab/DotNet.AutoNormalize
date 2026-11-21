# Pre-Launch Quality & Verification Checklist: DotNet.AutoNormalize

This checklist must be audited and verified before any public launch or social communication.

## 1. Visual Identity & Assets
- [x] **Project Logo Finalized**: Scalable SVGs (`logo.svg`, `logo-light.svg`, `logo-dark.svg`, `logo-monochrome.svg`) and high-res PNG (`logo.png`) located in `assets/branding/`.
- [x] **Project Icon Finalized**: Scalable SVGs (`icon.svg`, `icon-light.svg`, `icon-dark.svg`, `icon-monochrome.svg`) and 512x512 PNG (`icon.png`) in `assets/branding/`.
- [x] **GitHub Social Preview Ready**: 1280 × 640 high-contrast social preview image generated at `assets/branding/github-social-preview.png`.
- [x] **Branding Guide Published**: Color codes, typography rules, and usage standards documented in `docs/branding-guide.md`.
- [x] **Architecture Diagrams Generated**: Execution flow diagrams generated in `docs/images/` (`basic-normalization-flow.svg`, `json-deserialization-flow.svg`, `aspnetcore-integration-flow.svg`, `efcore-savechanges-flow.svg`, `before-and-after.svg`).

---

## 2. Documentation & Technical Content
- [x] **Hero README Reviewed**: Contains problem statement, why it exists, architecture visuals, quickstart, security model, and verified benchmark table.
- [x] **README Code Examples Verified**: All snippets tested against current API implementation (`AutoNormalizeOptions`, `AddAutoNormalize()`, `[PreserveWhitespace]`, `[DoNotNormalize]`).
- [x] **Installation Guide Completed**: All 5 packages documented separately with exact package IDs and minimal examples in `docs/installation.md`.
- [x] **Comprehensive Architecture Article**: 14-section in-depth technical article authored in `docs/articles/building-safe-input-normalization.md` (and summary in `docs/articles/building-safe-input-normalization-short.md`).
- [x] **Security Model Enforced**: Distinguishes representation canonicalization from validation and HTML sanitization in `docs/security-model.md` and `SECURITY.md`.
- [x] **Release Engineering Documentation**: Pre-flight release checklist and pipeline architecture documented in `docs/release-checklist.md` and `docs/release-engineering.md`.

---

## 3. Package Integrity & Engineering Standards
- [x] **Zero Compiler Warnings**: Full solution builds under `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`.
- [x] **Public API Compatibility**: Strict automated tests in `tests/DotNet.AutoNormalize.PublicApi.Tests` pass with zero breaking deviations.
- [x] **Automated Test Suite**: 77 passing automated tests across solution unit, integration, and package validation suites.
- [x] **Downstream Consumer Validation**: `tests/DotNet.AutoNormalize.PackageValidation` confirms that independent projects consume the generated `.nupkg` packages seamlessly.
- [x] **Package Asset Verification**: All 5 `.nupkg` and 5 `.snupkg` files verified for XML documentation, SourceLink symbols, and multi-target assets (`net8.0`, `net9.0`, `netstandard2.0`).
- [x] **Target Framework Accuracy**: No unverified claims regarding .NET 10; documented as forward-compatible via standard NuGet resolution.

---

## 4. Communication & Peer Review Drafts
- [x] **LinkedIn Drafts Prepared**: Launch announcement, technical teaser, and lessons-learned posts drafted in `docs/launch/communications/linkedin-drafts.md`.
- [x] **Discord Drafts Prepared**: Community showcase, architecture feedback, and discussion starter drafted in `docs/launch/communications/discord-drafts.md`.
- [x] **GitHub Discussions Drafts Prepared**: Introduction, early feedback, and preview announcement drafted in `docs/launch/communications/github-discussions-drafts.md`.
- [x] **Maintainer Review Request Prepared**: Respectful GitHub DM, detailed email, and follow-up drafted in `docs/launch/maintainer-review/luke-review-request.md` (no endorsement/shoutout requested).

---

## 5. Strict Publication Controls
- [x] **No Packages Published to NuGet.org**: All 10 package artifacts remain local in `./artifacts/packages/`.
- [x] **No Public GitHub Releases Created**: GitHub release workflow is staged for manual dispatch/tagging.
- [x] **No Git Tags Pushed**: Local repository only; no tags pushed to remote.
- [x] **No Communications Published**: All LinkedIn, Discord, and email messages remain strictly local drafts.
