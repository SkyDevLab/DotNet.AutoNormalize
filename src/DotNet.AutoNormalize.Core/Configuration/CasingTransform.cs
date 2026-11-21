namespace DotNet.AutoNormalize.Core;

/// <summary>
/// Specifies casing transformations for string normalization.
/// </summary>
public enum CasingTransform
{
    /// <summary>
    /// Preserves original casing.
    /// </summary>
    None = 0,

    /// <summary>
    /// Transforms text to lowercase using invariant culture.
    /// </summary>
    LowerCase = 1,

    /// <summary>
    /// Transforms text to uppercase using invariant culture.
    /// </summary>
    UpperCase = 2
}
