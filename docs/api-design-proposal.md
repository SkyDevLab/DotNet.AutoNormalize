# Public API Design Proposal: DotNet.AutoNormalize

## 1. Principles of the Public API

1. **Intuitive & Discoverable**: Follows established .NET idioms (`Microsoft.Extensions.Options`, `IServiceCollection` extension methods, declarative attributes).
2. **Lean & Purpose-Driven**: No speculative interfaces or redundant abstractions. Every public type serves an active runtime purpose.
3. **Safe by Default**: Trimming and Unicode Form C enabled by default; sensitive property safety net enabled by default; explicit attributes provide unambiguous opt-outs.
4. **Zero Confusion**: Normalization is purely canonicalization, never validation or sanitization.

---

## 2. Abstractions Package (`DotNet.AutoNormalize.Abstractions`)

```csharp
namespace DotNet.AutoNormalize.Abstractions;

/// <summary>
/// Defines a contract for normalizing a value of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of value to normalize.</typeparam>
public interface INormalizer<T>
{
    /// <summary>
    /// Normalizes the specified input value.
    /// </summary>
    /// <param name="value">The input value.</param>
    /// <param name="context">The contextual metadata for the current property or operation.</param>
    /// <returns>The normalized value, or the original instance if no changes were required.</returns>
    T Normalize(T value, NormalizationContext context);
}

/// <summary>
/// Specialized contract for normalizing string values.
/// </summary>
public interface IStringNormalizer : INormalizer<string?>
{
}

/// <summary>
/// Provides execution metadata during a normalization operation.
/// </summary>
public sealed class NormalizationContext
{
    /// <summary>
    /// Gets the property information if normalizing an object property; otherwise null.
    /// </summary>
    public System.Reflection.PropertyInfo? PropertyInfo { get; init; }

    /// <summary>
    /// Gets the target type enclosing the property being normalized.
    /// </summary>
    public Type TargetType { get; init; } = typeof(object);

    /// <summary>
    /// Gets the service provider for dependency resolution, if available.
    /// </summary>
    public IServiceProvider? Services { get; init; }

    /// <summary>
    /// Gets a dictionary of custom contextual items.
    /// </summary>
    public IDictionary<string, object?> Items { get; } = new Dictionary<string, object?>(StringComparer.Ordinal);
}

/// <summary>
/// Indicates that leading and trailing whitespace should be preserved for this property or parameter.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class PreserveWhitespaceAttribute : Attribute
{
}

/// <summary>
/// Completely excludes the decorated property, parameter, class, or struct from all normalization operations.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class DoNotNormalizeAttribute : Attribute
{
}

/// <summary>
/// Specifies a custom <see cref="INormalizer{T}"/> to normalize the decorated property or parameter.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class NormalizeWithAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NormalizeWithAttribute"/> class.
    /// </summary>
    /// <param name="normalizerType">The concrete type implementing <see cref="INormalizer{T}"/> or <see cref="IStringNormalizer"/>.</param>
    public NormalizeWithAttribute(Type normalizerType)
    {
        NormalizerType = normalizerType ?? throw new ArgumentNullException(nameof(normalizerType));
    }

    /// <summary>
    /// Gets the normalizer type.
    /// </summary>
    public Type NormalizerType { get; }
}

/// <summary>
/// Entry point for normalizing in-memory object graphs.
/// </summary>
public interface IAutoNormalizer
{
    /// <summary>
    /// Recursively normalizes all eligible properties of the specified target object.
    /// </summary>
    /// <typeparam name="T">The type of the target object.</typeparam>
    /// <param name="target">The target object to normalize.</param>
    /// <returns>The normalized object (mutated in place for mutable objects).</returns>
    T? Normalize<T>(T? target);

    /// <summary>
    /// Recursively normalizes all eligible properties of the specified target object.
    /// </summary>
    /// <param name="target">The target object to normalize.</param>
    /// <param name="targetType">The runtime type of the target object.</param>
    /// <returns>The normalized object.</returns>
    object? Normalize(object? target, Type targetType);
}
```

---

## 3. Core Engine Configuration (`DotNet.AutoNormalize.Core`)

```csharp
namespace DotNet.AutoNormalize.Core;

/// <summary>
/// Specifies how whitespace is trimmed from string values.
/// </summary>
public enum WhitespaceTrimMode
{
    /// <summary>
    /// Trims both leading and trailing whitespace.
    /// </summary>
    Both = 0,

    /// <summary>
    /// Trims only leading whitespace.
    /// </summary>
    Start = 1,

    /// <summary>
    /// Trims only trailing whitespace.
    /// </summary>
    End = 2
}

/// <summary>
/// Specifies text casing transformations.
/// </summary>
public enum CasingTransform
{
    /// <summary>
    /// Retains original casing.
    /// </summary>
    None = 0,

    /// <summary>
    /// Transforms text to lowercase using invariant culture.
    /// </summary>
    LowerCase = 1,

    /// <summary>
    /// Transforms text to uppercase using invariant culture.
    /// </summary>
    UpperCase = 2
}

/// <summary>
/// Configuration options governing string and object normalization.
/// </summary>
public sealed class AutoNormalizeOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to trim whitespace from strings.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool TrimStrings { get; set; } = true;

    /// <summary>
    /// Gets or sets the whitespace trimming mode.
    /// Defaults to <see cref="WhitespaceTrimMode.Both"/>.
    /// </summary>
    public WhitespaceTrimMode TrimMode { get; set; } = WhitespaceTrimMode.Both;

    /// <summary>
    /// Gets or sets a value indicating whether empty or whitespace-only strings are converted to <c>null</c>.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool EmptyStringsToNull { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether <c>null</c> strings are converted to empty strings (<c>""</c>).
    /// Defaults to <c>false</c>. Cannot be true if <see cref="EmptyStringsToNull"/> is true.
    /// </summary>
    public bool NullToEmptyStrings { get; set; } = false;

    /// <summary>
    /// Gets or sets the target Unicode normalization form, or <c>null</c> to disable Unicode normalization.
    /// Defaults to <see cref="System.Text.NormalizationForm.FormC"/>.
    /// </summary>
    public System.Text.NormalizationForm? UnicodeNormalization { get; set; } = System.Text.NormalizationForm.FormC;

    /// <summary>
    /// Gets or sets the global casing transformation.
    /// Defaults to <see cref="CasingTransform.None"/>.
    /// </summary>
    public CasingTransform Casing { get; set; } = CasingTransform.None;

    /// <summary>
    /// Gets or sets whether heuristic protection of common sensitive property names is enabled.
    /// Defaults to <c>true</c>. Note: This is a defense-in-depth safety net, not a security guarantee.
    /// </summary>
    public bool ProtectSensitiveProperties { get; set; } = true;

    /// <summary>
    /// Gets the set of property names automatically excluded from normalization when
    /// <see cref="ProtectSensitiveProperties"/> is enabled.
    /// Comparisons are case-insensitive.
    /// </summary>
    public ISet<string> SensitivePropertyNames { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Password", "Passcode", "Token", "AuthToken", "AccessToken", "RefreshToken",
        "Secret", "ClientSecret", "ApiKey", "PrivateKey", "Signature", "Hash", "Salt"
    };

    /// <summary>
    /// Gets or sets the maximum recursion depth allowed when traversing nested object graphs.
    /// Defaults to 32.
    /// </summary>
    public int MaxRecursionDepth { get; set; } = 32;

    /// <summary>
    /// Gets the ordered list of custom string normalizers to execute.
    /// </summary>
    public IList<IStringNormalizer> CustomNormalizers { get; } = new List<IStringNormalizer>();

    /// <summary>
    /// Adds a custom normalizer to the normalization pipeline.
    /// </summary>
    public AutoNormalizeOptions AddNormalizer(IStringNormalizer normalizer)
    {
        if (normalizer is null) throw new ArgumentNullException(nameof(normalizer));
        CustomNormalizers.Add(normalizer);
        return this;
    }
}
```

---

## 4. ASP.NET Core Registration API (`DotNet.AutoNormalize.AspNetCore`)

```csharp
namespace Microsoft.Extensions.DependencyInjection;

public static class AutoNormalizeServiceCollectionExtensions
{
    /// <summary>
    /// Adds AutoNormalize services to the specified <see cref="IServiceCollection"/> and configures
    /// automatic normalization for System.Text.Json request bodies and MVC model binding.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAutoNormalize(this IServiceCollection services)
    {
        return services.AddAutoNormalize(_ => { });
    }

    /// <summary>
    /// Adds AutoNormalize services to the specified <see cref="IServiceCollection"/> with custom options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">The delegate to configure <see cref="AutoNormalizeOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAutoNormalize(
        this IServiceCollection services,
        Action<AutoNormalizeOptions> configure)
    {
        // 1. Registers AutoNormalizeOptions
        // 2. Registers IAutoNormalizer (Singleton)
        // 3. Modifies Microsoft.AspNetCore.Http.Json.JsonOptions (Minimal APIs)
        // 4. Modifies Microsoft.AspNetCore.Mvc.JsonOptions (MVC controllers)
        // 5. Appends AutoNormalizeModelBinderProvider to MvcOptions
        ...
    }
}
```

---

## 5. Usage Examples

### 5.1 Basic Web API Setup
```csharp
var builder = WebApplication.CreateBuilder(args);

// One-line setup with standard defaults
builder.Services.AddAutoNormalize();

var app = builder.Build();
```

### 5.2 Custom Options & Domain Rules
```csharp
builder.Services.AddAutoNormalize(options =>
{
    options.TrimStrings = true;
    options.EmptyStringsToNull = true;
    options.UnicodeNormalization = NormalizationForm.FormC;
    
    // Custom domain rule
    options.AddNormalizer(new EmailCanonicalNormalizer());
});
```

### 5.3 Model Opt-Outs & Custom Annotations
```csharp
public sealed class CreateUserRequest
{
    public string Name { get; set; } = string.Empty; // Normalized

    [PreserveWhitespace]
    public string FormattedBio { get; set; } = string.Empty; // Whitespace preserved

    [DoNotNormalize]
    public string SecurityToken { get; set; } = string.Empty; // Skipped entirely

    public string Password { get; set; } = string.Empty; // Automatically skipped by sensitive name heuristic
}
```
