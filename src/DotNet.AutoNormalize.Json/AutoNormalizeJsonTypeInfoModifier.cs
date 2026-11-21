using System;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using DotNet.AutoNormalize.Abstractions;
using DotNet.AutoNormalize.Core;
using DotNet.AutoNormalize.Core.Engine;
using DotNet.AutoNormalize.Json.Converters;

namespace DotNet.AutoNormalize.Json;

/// <summary>
/// Configures System.Text.Json type contract metadata to normalize string properties during deserialization.
/// </summary>
public sealed class AutoNormalizeJsonTypeInfoModifier
{
    private readonly AutoNormalizeOptions _options;
    private readonly StringNormalizerPipeline _pipeline;
    private readonly NormalizationMetadataCache _metadataCache;
    private readonly IServiceProvider? _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoNormalizeJsonTypeInfoModifier"/> class.
    /// </summary>
    /// <param name="options">The auto-normalize configuration options.</param>
    /// <param name="metadataCache">The reflection metadata cache.</param>
    /// <param name="pipeline">The string normalizer pipeline.</param>
    /// <param name="serviceProvider">Optional service provider.</param>
    public AutoNormalizeJsonTypeInfoModifier(
        AutoNormalizeOptions options,
        NormalizationMetadataCache metadataCache,
        StringNormalizerPipeline pipeline,
        IServiceProvider? serviceProvider = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _metadataCache = metadataCache ?? throw new ArgumentNullException(nameof(metadataCache));
        _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Modifies the specified <see cref="JsonTypeInfo"/> to attach property-level normalizers.
    /// </summary>
    /// <param name="typeInfo">The type information being configured.</param>
    public void Modify(JsonTypeInfo typeInfo)
    {
        if (typeInfo is null || typeInfo.Kind != JsonTypeInfoKind.Object)
        {
            return;
        }

        // Check type-level opt-out
        if (typeInfo.Type.IsDefined(typeof(DoNotNormalizeAttribute), true))
        {
            return;
        }

        TypeNormalizationMetadata typeMeta = _metadataCache.GetMetadata(typeInfo.Type);
        if (typeMeta.IsDoNotNormalize || typeMeta.IsIgnoredType)
        {
            return;
        }

        for (int i = 0; i < typeInfo.Properties.Count; i++)
        {
            JsonPropertyInfo jsonProp = typeInfo.Properties[i];

            // Only normalize string properties
            if (jsonProp.PropertyType != typeof(string))
            {
                continue;
            }

            // Check if property is marked with DoNotNormalize via AttributeProvider
            if (jsonProp.AttributeProvider != null &&
                jsonProp.AttributeProvider.IsDefined(typeof(DoNotNormalizeAttribute), true))
            {
                continue;
            }

            // Locate matching PropertyNormalizationMetadata
            PropertyNormalizationMetadata? propMeta = FindPropertyMetadata(typeMeta, jsonProp);

            if (propMeta?.DoNotNormalize == true)
            {
                continue;
            }

            // Attach property-scoped string converter
            jsonProp.CustomConverter = new AutoNormalizeStringJsonConverter(
                _pipeline,
                propMeta,
                _serviceProvider);
        }
    }

    private static PropertyNormalizationMetadata? FindPropertyMetadata(
        TypeNormalizationMetadata typeMeta,
        JsonPropertyInfo jsonProp)
    {
        for (int i = 0; i < typeMeta.Properties.Count; i++)
        {
            var p = typeMeta.Properties[i];
            // Match by PropertyInfo name or CLR property identity
            if (string.Equals(p.PropertyName, jsonProp.Name, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.PropertyName, (jsonProp.AttributeProvider as MemberInfo)?.Name, StringComparison.OrdinalIgnoreCase))
            {
                return p;
            }
        }

        return null;
    }
}
