using System;

namespace DotNet.AutoNormalize.Abstractions;

/// <summary>
/// Indicates that leading and trailing whitespace should be preserved for this property or parameter.
/// Trimming and empty-to-null transformations are skipped.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class PreserveWhitespaceAttribute : Attribute
{
}
