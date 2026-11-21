using System;
using DotNet.AutoNormalize.Abstractions;

namespace DotNet.AutoNormalize.Core.Normalizers;

/// <summary>
/// Normalizer that trims leading and/or trailing whitespace from strings with fast-path detection.
/// </summary>
public sealed class TrimNormalizer : IStringNormalizer
{
    private readonly WhitespaceTrimMode _mode;

    /// <summary>
    /// Initializes a new instance of the <see cref="TrimNormalizer"/> class.
    /// </summary>
    /// <param name="mode">The whitespace trim mode.</param>
    public TrimNormalizer(WhitespaceTrimMode mode = WhitespaceTrimMode.Both)
    {
        _mode = mode;
    }

    /// <inheritdoc />
    public string? Normalize(string? value, NormalizationContext context)
    {
        if (value is null || value.Length == 0)
        {
            return value;
        }

        return _mode switch
        {
            WhitespaceTrimMode.Start => FastTrimStart(value),
            WhitespaceTrimMode.End => FastTrimEnd(value),
            _ => FastTrimBoth(value)
        };
    }

    private static string FastTrimBoth(string value)
    {
        // Fast-path: check first and last characters before calling string.Trim()
        if (!char.IsWhiteSpace(value[0]) && !char.IsWhiteSpace(value[value.Length - 1]))
        {
            return value;
        }

        return value.Trim();
    }

    private static string FastTrimStart(string value)
    {
        // Fast-path: check first character
        if (!char.IsWhiteSpace(value[0]))
        {
            return value;
        }

        return value.TrimStart();
    }

    private static string FastTrimEnd(string value)
    {
        // Fast-path: check last character
        if (!char.IsWhiteSpace(value[value.Length - 1]))
        {
            return value;
        }

        return value.TrimEnd();
    }
}
