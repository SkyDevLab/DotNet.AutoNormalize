# Contributing to DotNet.AutoNormalize

Thank you for contributing to DotNet.AutoNormalize!

## Prerequisites
- .NET SDK 9.0 (and .NET 8.0 runtime)
- Git

## Building and Testing Locally

1. Clone the repository:
   ```bash
   git clone https://github.com/SkyDevLab/DotNet.AutoNormalize.git
   cd DotNet.AutoNormalize
   ```
2. Restore and build:
   ```bash
   dotnet restore
   dotnet build -c Release
   ```
3. Run all tests:
   ```bash
   dotnet test -c Release
   ```
4. Verify packaging:
   ```bash
   dotnet pack -c Release -o ./artifacts/packages
   dotnet test tests/DotNet.AutoNormalize.PackageValidation/DotNet.AutoNormalize.PackageValidation.csproj
   ```

## Development Guidelines
- Maintain zero compiler warnings (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`).
- Preserve XML documentation comments on all public types and members.
- Ensure new features include unit tests and preserve public API compatibility.
- Ensure public API modifications pass `tests/DotNet.AutoNormalize.PublicApi.Tests`.
