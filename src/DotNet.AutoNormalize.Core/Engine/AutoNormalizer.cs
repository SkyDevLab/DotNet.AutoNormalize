using System;
using DotNet.AutoNormalize.Abstractions;
using Microsoft.Extensions.Options;

namespace DotNet.AutoNormalize.Core.Engine;

/// <summary>
/// Default implementation of <see cref="IAutoNormalizer"/> that applies normalization rules across object graphs.
/// </summary>
public sealed class AutoNormalizer : IAutoNormalizer
{
    private readonly AutoNormalizeOptions _options;
    private readonly NormalizationMetadataCache _metadataCache;
    private readonly StringNormalizerPipeline _pipeline;
    private readonly ObjectGraphNormalizer _graphNormalizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoNormalizer"/> class with optional options and services.
    /// </summary>
    /// <param name="options">Configuration options; defaults to standard options if null.</param>
    /// <param name="services">Optional service provider for resolving custom normalizers.</param>
    public AutoNormalizer(AutoNormalizeOptions? options = null, IServiceProvider? services = null)
    {
        _options = options ?? new AutoNormalizeOptions();
        _options.Validate();

        _metadataCache = new NormalizationMetadataCache(_options, services);
        _pipeline = new StringNormalizerPipeline(_options);
        _graphNormalizer = new ObjectGraphNormalizer(_options, _metadataCache, _pipeline);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoNormalizer"/> class using <see cref="IOptions{TOptions}"/>.
    /// </summary>
    /// <param name="options">The options wrapper.</param>
    /// <param name="services">Optional service provider for resolving custom normalizers.</param>
    public AutoNormalizer(IOptions<AutoNormalizeOptions> options, IServiceProvider? services = null)
        : this(options?.Value, services)
    {
    }

    /// <summary>
    /// Gets the active configuration options.
    /// </summary>
    public AutoNormalizeOptions Options => _options;

    /// <summary>
    /// Gets the underlying string normalizer pipeline.
    /// </summary>
    public StringNormalizerPipeline Pipeline => _pipeline;

    /// <summary>
    /// Gets the metadata cache used by this normalizer.
    /// </summary>
    public NormalizationMetadataCache MetadataCache => _metadataCache;

    /// <inheritdoc />
    public T? Normalize<T>(T? target, NormalizationContext? context = null)
    {
        if (target is null)
        {
            if (typeof(T) == typeof(string) && _options.NullToEmptyStrings)
            {
                return (T?)(object)string.Empty;
            }
            return default;
        }

        // Fast-path: scalar string bypasses object graph traversal and allocations entirely
        if (typeof(T) == typeof(string) && target is string str)
        {
            string? normalized = _pipeline.Normalize(str, context ?? ObjectGraphNormalizer.DefaultContext);
            return (T?)(object?)normalized;
        }

        object? result = _graphNormalizer.Normalize(target, typeof(T), context);
        return (T?)result;
    }

    /// <inheritdoc />
    public object? Normalize(object? target, Type targetType, NormalizationContext? context = null)
    {
        if (targetType is null)
        {
            throw new ArgumentNullException(nameof(targetType));
        }

        if (target is null)
        {
            if (targetType == typeof(string) && _options.NullToEmptyStrings)
            {
                return string.Empty;
            }
            return null;
        }

        return _graphNormalizer.Normalize(target, targetType, context);
    }
}
