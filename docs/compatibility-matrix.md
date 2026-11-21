# Compatibility & Target Framework Matrix: DotNet.AutoNormalize

## 1. Supported .NET Runtimes and Lifecycle Alignment

`DotNet.AutoNormalize` strictly aligns with Microsoft's official .NET Support Lifecycle.

| Runtime Version | Support Status | Release Type | Support End Date | Supported in AutoNormalize |
| :--- | :--- | :--- | :--- | :--- |
| **.NET 8.0** | Supported | LTS (Long Term Support) | November 10, 2026 | **Yes (Primary Target)** |
| **.NET 9.0** | Supported | STS (Standard Term Support) | May 2026 | **Yes (Primary Target)** |
| **.NET 10.0** | Upcoming (Preview) | LTS | November 2028 | **Planned / Forward-Compatible** |
| **.NET 6.0 & 7.0**| End of Life (EOL) | EOL | Expired | **No** (Security & Lifecycle policy) |
| **.NET Framework 4.8**| Legacy Windows | Windows Component | Extended | **Abstractions Only** via `netstandard2.0` |

---

## 2. Package Target Framework Multi-Targeting Strategy

| Package Name | Target Frameworks | Direct Dependencies | Rationale |
| :--- | :--- | :--- | :--- |
| **`DotNet.AutoNormalize.Abstractions`** | `netstandard2.0`<br/>`net8.0`<br/>`net9.0` | None | Enables shared model/DTO libraries that still multi-target `.NET Standard 2.0` to reference attributes (`[PreserveWhitespace]`, `[DoNotNormalize]`) without dragging in .NET 8/9 runtimes. |
| **`DotNet.AutoNormalize.Core`** | `net8.0`<br/>`net9.0` | `DotNet.AutoNormalize.Abstractions`<br/>`Microsoft.Extensions.Options` | Core normalization engine, cached metadata, and built-in rules. Uses modern C# span optimizations and memory primitives. |
| **`DotNet.AutoNormalize.Json`** | `net8.0`<br/>`net9.0` | `DotNet.AutoNormalize.Abstractions`<br/>`System.Text.Json` (>= 8.0.0) | Leverages `IJsonTypeInfoResolver` modifiers introduced in .NET 7+ for property-level interceptors. |
| **`DotNet.AutoNormalize.AspNetCore`** | `net8.0`<br/>`net9.0` | `DotNet.AutoNormalize.Core`<br/>`DotNet.AutoNormalize.Json`<br/>`<FrameworkReference Include="Microsoft.AspNetCore.App" />` | Seamless integration with Minimal APIs, MVC Model Binding, and Dependency Injection. |
| **`DotNet.AutoNormalize.EntityFrameworkCore`** | `net8.0`<br/>`net9.0` | `DotNet.AutoNormalize.Core`<br/>`Microsoft.EntityFrameworkCore` (8.0 on net8.0, 9.0 on net9.0) | Pre-save entity normalization via `ISaveChangesInterceptor`. Strictly version-matched to avoid EF Core runtime conflicts. |

---

## 3. Framework-Specific Feature Compatibility

### 3.1 ASP.NET Core Web Integration
* **Minimal APIs**: Fully supported on .NET 8 and .NET 9. `System.Text.Json` modifiers normalize request bodies automatically during JSON deserialization.
* **MVC Controllers**: Supported on .NET 8 and .NET 9. Request bodies normalized via `System.Text.Json`; query, route, header, and form inputs normalized via `AutoNormalizeModelBinderProvider`.
* **Zero ASP.NET Core Version Pollution**: The web package utilizes `<FrameworkReference Include="Microsoft.AspNetCore.App" />`, ensuring it binds directly to the hosting application's ambient runtime without downloading redundant NuGet DLLs.

### 3.2 Entity Framework Core Multi-Targeting
EF Core packages do not maintain cross-major-version binary compatibility. An application running EF Core 9 cannot safely load an interceptor compiled against EF Core 8.
To prevent version mismatches, `DotNet.AutoNormalize.EntityFrameworkCore` targets:
* `net8.0`: References `Microsoft.EntityFrameworkCore` `8.0.*`
* `net9.0`: References `Microsoft.EntityFrameworkCore` `9.0.*`

Conditioned in MSBuild:
```xml
<ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.11" />
</ItemGroup>
<ItemGroup Condition="'$(TargetFramework)' == 'net9.0'">
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
</ItemGroup>
```

---

## 4. Platform & Architecture Portability

`DotNet.AutoNormalize` consists entirely of pure, safe managed code:
* **Operating Systems**: 100% portable across Linux (Ubuntu, Debian, Alpine, RHEL), macOS (ARM64, x64), and Windows (x64, ARM64).
* **Container Environments**: Verified for lightweight Alpine and distroless container images.
* **Native AOT Compatibility**: Core engine and JSON modifiers avoid dynamic code emission (`Emit` / unmanaged pointers), allowing clean compilation with `<PublishAot>true</PublishAot>` under .NET 8 and .NET 9.
