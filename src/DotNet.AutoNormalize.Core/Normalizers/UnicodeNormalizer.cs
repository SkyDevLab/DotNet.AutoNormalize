using System.Text;
using DotNet.AutoNormalize.Abstractions;

namespace DotNet.AutoNormalize.Core.Normalizers;

/// <summary>
/// Normalizer that standardizes Unicode strings to a specified <see cref="NormalizationForm"/>.
/// </summary>
public sealed class UnicodeNormalizer : IStringNormalizer
{
    private readonly NormalizationForm _form;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnicodeNormalizer"/> class.
    /// </summary>
    /// <param name="form">The target Unicode normalization form.</param>
    public UnicodeNormalizer(NormalizationForm form = NormalizationForm.FormC)
    {
        _form = form;
    }

    /// <inheritdoc />
    public string? Normalize(string? value, NormalizationContext context)
    {
        if (value is null || value.Length == 0)
        {
            return value;
        }

        // Fast-path: Check if already in target normalization form to avoid heap allocation
        if (!value.IsNormalized(_form))
        {
            return value.Normalize(_form);
        }

        return value;
    }
}
