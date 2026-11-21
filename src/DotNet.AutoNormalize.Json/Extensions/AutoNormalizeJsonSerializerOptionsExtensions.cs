using System;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using DotNet.AutoNormalize.Core;
using DotNet.AutoNormalize.Core.Engine;

namespace DotNet.AutoNormalize.Json.Extensions;

/// <summary>
/// Extension methods configuring <see cref="JsonSerializerOptions"/> for automatic normalization.
/// </summary>
public static class AutoNormalizeJsonSerializerOptionsExtensions
{
    /// <summary>
    /// Configures <see cref="JsonSerializerOptions"/> with AutoNormalize type contract modifiers using custom options.
    /// </summary>
    /// <param name="jsonOptions">The JSON serializer options to configure.</param>
    /// <param name="configure">The delegate to configure options.</param>
    /// <returns>The configured options instance.</returns>
    public static JsonSerializerOptions AddAutoNormalize(
        this JsonSerializerOptions jsonOptions,
        Action<AutoNormalizeOptions> configure)
    {
        if (jsonOptions is null) throw new ArgumentNullException(nameof(jsonOptions));
        if (configure is null) throw new ArgumentNullException(nameof(configure));

        var options = new AutoNormalizeOptions();
        configure(options);
        return jsonOptions.AddAutoNormalize(options);
    }

    /// <summary>
    /// Configures <see cref="JsonSerializerOptions"/> with AutoNormalize type contract modifiers.
    /// </summary>
    /// <param name="jsonOptions">The JSON serializer options to configure.</param>
    /// <param name="normalizeOptions">Optional AutoNormalize options.</param>
    /// <param name="serviceProvider">Optional service provider.</param>
    /// <returns>The configured options instance.</returns>
    public static JsonSerializerOptions AddAutoNormalize(
        this JsonSerializerOptions jsonOptions,
        AutoNormalizeOptions? normalizeOptions = null,
        IServiceProvider? serviceProvider = null)
    {
        if (jsonOptions is null) throw new ArgumentNullException(nameof(jsonOptions));

        normalizeOptions ??= new AutoNormalizeOptions();
        normalizeOptions.Validate();

        var metadataCache = new NormalizationMetadataCache(normalizeOptions, serviceProvider);
        var pipeline = new StringNormalizerPipeline(normalizeOptions);
        var modifier = new AutoNormalizeJsonTypeInfoModifier(normalizeOptions, metadataCache, pipeline, serviceProvider);

        var resolver = jsonOptions.TypeInfoResolver ?? new DefaultJsonTypeInfoResolver();
        jsonOptions.TypeInfoResolver = resolver.WithAddedModifier(modifier.Modify);

        return jsonOptions;
    }
}
