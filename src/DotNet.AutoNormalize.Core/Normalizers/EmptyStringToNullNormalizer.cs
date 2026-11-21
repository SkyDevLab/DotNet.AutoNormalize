using DotNet.AutoNormalize.Abstractions;

namespace DotNet.AutoNormalize.Core.Normalizers;

/// <summary>
/// Normalizer that converts empty or whitespace-only strings to <c>null</c>.
/// </summary>
public sealed class EmptyStringToNullNormalizer : IStringNormalizer
{
    /// <inheritdoc />
    public string? Normalize(string? value, NormalizationContext context)
    {
        if (value is null)
        {
            return null;
        }

        if (value.Length == 0)
        {
            return null;
        }

        // Fast-path for non-whitespace strings
        if (!char.IsWhiteSpace(value[0]) && !char.IsWhiteSpace(value[value.Length - 1]))
        {
            return value;
        }

        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
