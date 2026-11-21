namespace DotNet.AutoNormalize.Abstractions;

/// <summary>
/// Defines a contract for normalizing a value of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of value to normalize.</typeparam>
public interface INormalizer<T>
{
    /// <summary>
    /// Normalizes the specified input value according to the normalizer's rules.
    /// </summary>
    /// <param name="value">The input value.</param>
    /// <param name="context">The contextual metadata for the current property or operation.</param>
    /// <returns>The normalized value, or the original instance if no changes were required.</returns>
    T Normalize(T value, NormalizationContext context);
}
