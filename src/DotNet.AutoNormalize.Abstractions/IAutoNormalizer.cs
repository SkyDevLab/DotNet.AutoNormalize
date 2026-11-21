using System;

namespace DotNet.AutoNormalize.Abstractions;

/// <summary>
/// Entry point for normalizing objects and object graphs.
/// </summary>
public interface IAutoNormalizer
{
    /// <summary>
    /// Recursively normalizes all eligible properties of the specified target object.
    /// </summary>
    /// <typeparam name="T">The type of the target object.</typeparam>
    /// <param name="target">The target object to normalize.</param>
    /// <param name="context">Optional normalization context.</param>
    /// <returns>The normalized object (mutated in place for mutable objects).</returns>
    T? Normalize<T>(T? target, NormalizationContext? context = null);

    /// <summary>
    /// Recursively normalizes all eligible properties of the specified target object with explicit runtime type.
    /// </summary>
    /// <param name="target">The target object to normalize.</param>
    /// <param name="targetType">The runtime type of the target object.</param>
    /// <param name="context">Optional normalization context.</param>
    /// <returns>The normalized object.</returns>
    object? Normalize(object? target, Type targetType, NormalizationContext? context = null);
}
