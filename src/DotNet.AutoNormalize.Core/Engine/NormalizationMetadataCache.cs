using System;
using System.Collections.Concurrent;

namespace DotNet.AutoNormalize.Core.Engine;

/// <summary>
/// Thread-safe cache of reflection metadata for types evaluated during normalization.
/// </summary>
public sealed class NormalizationMetadataCache
{
    private readonly ConcurrentDictionary<Type, TypeNormalizationMetadata> _cache = new();
    private readonly AutoNormalizeOptions _options;
    private readonly IServiceProvider? _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="NormalizationMetadataCache"/> class.
    /// </summary>
    /// <param name="options">The auto-normalize options.</param>
    /// <param name="serviceProvider">Optional service provider for resolving custom normalizers.</param>
    public NormalizationMetadataCache(AutoNormalizeOptions options, IServiceProvider? serviceProvider = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Gets or creates the cached <see cref="TypeNormalizationMetadata"/> for the specified type.
    /// </summary>
    /// <param name="type">The type to look up or analyze.</param>
    /// <returns>The cached metadata.</returns>
    public TypeNormalizationMetadata GetMetadata(Type type)
    {
        if (type is null) throw new ArgumentNullException(nameof(type));

        return _cache.GetOrAdd(type, t => new TypeNormalizationMetadata(t, _options, _serviceProvider));
    }

    /// <summary>
    /// Clears all cached metadata entries.
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
    }
}
