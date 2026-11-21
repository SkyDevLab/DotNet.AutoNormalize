# Release Checklist: DotNet.AutoNormalize

This checklist governs the release process for preview and stable releases of DotNet.AutoNormalize.

## 1. Pre-Release Verification

- [ ] **Working Tree State**:
  - `git status` is clean with no uncommitted files or untracked changes.
  - All feature branches merged to `main`.
- [ ] **Automated Test Suite**:
  - Full test suite passes locally in Release mode:
    ```bash
    dotnet test DotNet.AutoNormalize.sln -c Release
    ```
  - All 77 tests (public API, core, aspnetcore, efcore, package validation) pass.
- [ ] **Public API Compatibility**:
  - `tests/DotNet.AutoNormalize.PublicApi.Tests` passes with zero breaking changes or unapproved API additions.
- [ ] **Dependency Vulnerability Audit**:
  - No vulnerable packages reported:
    ```bash
    dotnet list package --vulnerable --include-transitive
    ```
- [ ] **Sample Applications**:
  - All 4 samples (`BasicUsage`, `MinimalApi`, `WebApi`, `EntityFrameworkCore`) compile and execute successfully.

---

## 2. Versioning & Documentation

- [ ] **Semantic Versioning Compliance**:
  - Version number adheres to SemVer 2.0.0 (`1.0.0-preview.1`, `1.0.0-preview.2`, or `1.0.0`).
  - `Directory.Build.props` updated:
    * `<VersionPrefix>1.0.0</VersionPrefix>`
    * `<VersionSuffix>preview.1</VersionSuffix>` (or empty for stable).
- [ ] **Changelog Updated**:
  - `CHANGELOG.md` updated with release date, added features, fixes, and architectural notes.
  - Release notes drafted in `docs/release-notes-<version>.md`.

---

## 3. Packaging & Consumer Validation

- [ ] **Clean Package Generation**:
  - Generate packages into fresh artifacts directory:
    ```bash
    rmdir /s /q artifacts\packages
    dotnet pack -c Release -o ./artifacts/packages -p:ContinuousIntegrationBuild=true
    ```
  - Verify all 5 `.nupkg` and 5 `.snupkg` files are present.
- [ ] **Downstream Consumer Test**:
  - Execute independent package consumer tests against the local packages:
    ```bash
    dotnet test tests/DotNet.AutoNormalize.PackageValidation/DotNet.AutoNormalize.PackageValidation.csproj -c Release
    ```
- [ ] **Checksum Generation**:
  - Generate SHA-256 checksums:
    ```bash
    powershell -ExecutionPolicy Bypass -File scratch/generate_checksums.ps1
    ```

---

## 4. GitHub Release & Publishing (Protected)

- [ ] **Git Tagging**:
  - Create annotated git tag:
    ```bash
    git tag -a v1.0.0-preview.1 -m "Release v1.0.0-preview.1"
    ```
- [ ] **GitHub Release Workflow**:
  - Trigger `.github/workflows/release.yml` by pushing tag or triggering manual dispatch.
  - Inspect workflow logs for successful verification.
- [ ] **Manual Approval Gate**:
  - Approve `nuget-release` environment deployment in GitHub repository settings.
  - Verify all 5 packages are successfully published to NuGet.org.
- [ ] **Post-Publish Verification**:
  - Validate package availability on NuGet.org:
    ```bash
    dotnet add package AutoNormalize.AspNetCore --version 1.0.0-preview.1
    ```
