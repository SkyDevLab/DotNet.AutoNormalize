using DotNet.AutoNormalize.Abstractions;

namespace DotNet.AutoNormalize.Core.Normalizers;

/// <summary>
/// Normalizer that converts <c>null</c> strings to empty strings (<c>""</c>).
/// </summary>
public sealed class NullToEmptyStringNormalizer : IStringNormalizer
{
    /// <inheritdoc />
    public string? Normalize(string? value, NormalizationContext context)
    {
        return value ?? string.Empty;
    }
}
