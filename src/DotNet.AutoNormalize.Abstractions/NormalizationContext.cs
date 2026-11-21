using System;
using System.Collections.Generic;
using System.Reflection;

namespace DotNet.AutoNormalize.Abstractions;

/// <summary>
/// Provides execution metadata during a normalization operation.
/// </summary>
public sealed class NormalizationContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NormalizationContext"/> class.
    /// </summary>
    public NormalizationContext()
    {
    }

    /// <summary>
    /// Gets or sets the reflection <see cref="PropertyInfo"/> if normalizing an object property; otherwise <c>null</c>.
    /// </summary>
    public PropertyInfo? PropertyInfo { get; set; }

    /// <summary>
    /// Gets or sets the target type enclosing the property being normalized.
    /// </summary>
    public Type TargetType { get; set; } = typeof(object);

    /// <summary>
    /// Gets or sets the service provider used for dependency resolution, if available.
    /// </summary>
    public IServiceProvider? Services { get; set; }

    private IDictionary<string, object?>? _items;

    /// <summary>
    /// Gets a dictionary of custom contextual items. Lazily initialized to avoid allocations in hot paths.
    /// </summary>
    public IDictionary<string, object?> Items => _items ??= new Dictionary<string, object?>(StringComparer.Ordinal);
}
