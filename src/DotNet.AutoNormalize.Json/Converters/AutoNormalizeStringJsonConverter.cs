using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotNet.AutoNormalize.Abstractions;
using DotNet.AutoNormalize.Core.Engine;

namespace DotNet.AutoNormalize.Json.Converters;

/// <summary>
/// A property-scoped JSON converter for strings that applies normalization rules with full property context.
/// </summary>
public sealed class AutoNormalizeStringJsonConverter : JsonConverter<string?>
{
    private readonly StringNormalizerPipeline _pipeline;
    private readonly PropertyNormalizationMetadata? _metadata;
    private readonly IServiceProvider? _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoNormalizeStringJsonConverter"/> class.
    /// </summary>
    public AutoNormalizeStringJsonConverter(
        StringNormalizerPipeline pipeline,
        PropertyNormalizationMetadata? metadata,
        IServiceProvider? serviceProvider = null)
    {
        _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        _metadata = metadata;
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var context = new NormalizationContext
        {
            PropertyInfo = _metadata?.PropertyInfo,
            TargetType = _metadata?.PropertyInfo?.DeclaringType ?? typeof(object),
            Services = _serviceProvider
        };

        if (reader.TokenType == JsonTokenType.Null)
        {
            return _pipeline.Normalize(null, context, _metadata);
        }

        string? raw = reader.GetString();
        return _pipeline.Normalize(raw, context, _metadata);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStringValue(value);
        }
    }
}
