using System;

namespace DotNet.AutoNormalize.Abstractions;

/// <summary>
/// Completely excludes the decorated property, parameter, class, or struct from all normalization operations.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class DoNotNormalizeAttribute : Attribute
{
}
