using System;

namespace DotNet.AutoNormalize.Abstractions;

/// <summary>
/// Specifies a custom <see cref="INormalizer{T}"/> or <see cref="IStringNormalizer"/> to normalize the decorated property or parameter.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class NormalizeWithAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NormalizeWithAttribute"/> class.
    /// </summary>
    /// <param name="normalizerType">The concrete or interface type implementing <see cref="INormalizer{T}"/> or <see cref="IStringNormalizer"/>.</param>
    public NormalizeWithAttribute(Type normalizerType)
    {
        NormalizerType = normalizerType ?? throw new ArgumentNullException(nameof(normalizerType));
    }

    /// <summary>
    /// Gets the type of the normalizer to instantiate and execute.
    /// </summary>
    public Type NormalizerType { get; }
}
