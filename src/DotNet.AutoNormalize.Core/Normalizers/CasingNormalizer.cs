using DotNet.AutoNormalize.Abstractions;

namespace DotNet.AutoNormalize.Core.Normalizers;

/// <summary>
/// Normalizer that transforms text casing using invariant culture.
/// </summary>
public sealed class CasingNormalizer : IStringNormalizer
{
    private readonly CasingTransform _transform;

    /// <summary>
    /// Initializes a new instance of the <see cref="CasingNormalizer"/> class.
    /// </summary>
    /// <param name="transform">The casing transformation mode.</param>
    public CasingNormalizer(CasingTransform transform)
    {
        _transform = transform;
    }

    /// <inheritdoc />
    public string? Normalize(string? value, NormalizationContext context)
    {
        if (value is null || value.Length == 0 || _transform == CasingTransform.None)
        {
            return value;
        }

        return _transform switch
        {
            CasingTransform.LowerCase => value.ToLowerInvariant(),
            CasingTransform.UpperCase => value.ToUpperInvariant(),
            _ => value
        };
    }
}
