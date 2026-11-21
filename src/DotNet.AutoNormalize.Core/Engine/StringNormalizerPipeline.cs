using System;
using System.Text;
using DotNet.AutoNormalize.Abstractions;

namespace DotNet.AutoNormalize.Core.Engine;

/// <summary>
/// High-performance pipeline executing string normalization rules with independent fast-paths.
/// </summary>
public sealed class StringNormalizerPipeline
{
    private readonly AutoNormalizeOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="StringNormalizerPipeline"/> class.
    /// </summary>
    /// <param name="options">The auto-normalize options governing pipeline execution.</param>
    public StringNormalizerPipeline(AutoNormalizeOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Normalizes a string according to configuration, contextual metadata, and fast paths.
    /// </summary>
    /// <param name="value">The string value to normalize.</param>
    /// <param name="context">The normalization context.</param>
    /// <param name="metadata">Optional property-level metadata.</param>
    /// <returns>The normalized string value, or original reference if unchanged.</returns>
    public string? Normalize(string? value, NormalizationContext context, PropertyNormalizationMetadata? metadata = null)
    {
        // 1. Check DoNotNormalize opt-out
        if (metadata?.DoNotNormalize == true)
        {
            return value;
        }

        // 2. Handle null inputs
        if (value is null)
        {
            if (_options.NullToEmptyStrings &&
                metadata?.PreserveWhitespace != true &&
                (metadata is null || !metadata.IsSensitive || !_options.ProtectSensitiveProperties))
            {
                return string.Empty;
            }
            return null;
        }

        // 3. Fast-path: Check if empty string
        if (value.Length == 0)
        {
            if (_options.EmptyStringsToNull &&
                metadata?.PreserveWhitespace != true &&
                (metadata is null || !metadata.IsSensitive || !_options.ProtectSensitiveProperties))
            {
                return null;
            }
            return string.Empty;
        }

        // Check if property is marked to preserve whitespace or sensitive
        bool shouldPreserveWhitespace = metadata?.PreserveWhitespace == true ||
            (metadata?.IsSensitive == true && _options.ProtectSensitiveProperties);

        string current = value;

        // 4. Trimming with fast-path detection
        if (_options.TrimStrings && !shouldPreserveWhitespace)
        {
            current = TrimWithFastPath(current, _options.TrimMode);

            // If string became empty after trim
            if (current.Length == 0)
            {
                if (_options.EmptyStringsToNull)
                {
                    return null;
                }
                return string.Empty;
            }
        }

        // 5. Unicode normalization with fast-path
        if (_options.UnicodeNormalization.HasValue &&
            (metadata is null || !metadata.IsSensitive || !_options.ProtectSensitiveProperties))
        {
            NormalizationForm form = _options.UnicodeNormalization.Value;
            if (!current.IsNormalized(form))
            {
                current = current.Normalize(form);
            }
        }

        // 6. Casing transform
        if (_options.Casing != CasingTransform.None &&
            (metadata is null || !metadata.IsSensitive || !_options.ProtectSensitiveProperties))
        {
            current = _options.Casing switch
            {
                CasingTransform.LowerCase => current.ToLowerInvariant(),
                CasingTransform.UpperCase => current.ToUpperInvariant(),
                _ => current
            };
        }

        // 7. Custom property-level normalizer from [NormalizeWith]
        if (metadata?.CustomNormalizer != null)
        {
            current = metadata.CustomNormalizer.Normalize(current, context)!;
            if (current is null)
            {
                return null;
            }
        }

        // 8. Global custom normalizers
        if (_options.CustomNormalizers.Count > 0 &&
            (metadata is null || !metadata.IsSensitive || !_options.ProtectSensitiveProperties))
        {
            for (int i = 0; i < _options.CustomNormalizers.Count; i++)
            {
                current = _options.CustomNormalizers[i].Normalize(current, context)!;
                if (current is null)
                {
                    return null;
                }
            }
        }

        return current;
    }

    private static string TrimWithFastPath(string input, WhitespaceTrimMode mode)
    {
        if (input.Length == 0)
        {
            return input;
        }

        return mode switch
        {
            WhitespaceTrimMode.Start => char.IsWhiteSpace(input[0]) ? input.TrimStart() : input,
            WhitespaceTrimMode.End => char.IsWhiteSpace(input[input.Length - 1]) ? input.TrimEnd() : input,
            _ => (!char.IsWhiteSpace(input[0]) && !char.IsWhiteSpace(input[input.Length - 1]))
                ? input
                : input.Trim()
        };
    }
}
