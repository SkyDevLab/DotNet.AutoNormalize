using System;
using System.Collections.Generic;
using System.Reflection;
using DotNet.AutoNormalize.Abstractions;

namespace DotNet.AutoNormalize.Core.Engine;

/// <summary>
/// Cached reflection metadata for a normalized type.
/// </summary>
public sealed class TypeNormalizationMetadata
{
    /// <summary>
    /// Gets the CLR type described by this metadata.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Gets a value indicating whether the type has been marked with <see cref="DoNotNormalizeAttribute"/>.
    /// </summary>
    public bool IsDoNotNormalize { get; }

    /// <summary>
    /// Gets a value indicating whether this type is a primitive, scalar, or system type that requires no object traversal.
    /// </summary>
    public bool IsIgnoredType { get; }

    /// <summary>
    /// Gets all public instance properties eligible for normalization inspection.
    /// </summary>
    public IReadOnlyList<PropertyNormalizationMetadata> Properties { get; }

    /// <summary>
    /// Gets all string properties eligible for normalization.
    /// </summary>
    public IReadOnlyList<PropertyNormalizationMetadata> StringProperties { get; }

    /// <summary>
    /// Gets all complex reference properties eligible for nested normalization.
    /// </summary>
    public IReadOnlyList<PropertyNormalizationMetadata> ComplexProperties { get; }

    /// <summary>
    /// Gets all collection or dictionary properties eligible for element normalization.
    /// </summary>
    public IReadOnlyList<PropertyNormalizationMetadata> CollectionProperties { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeNormalizationMetadata"/> class.
    /// </summary>
    public TypeNormalizationMetadata(
        Type type,
        AutoNormalizeOptions options,
        IServiceProvider? serviceProvider)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));

        IsDoNotNormalize = type.IsDefined(typeof(DoNotNormalizeAttribute), true);
        IsIgnoredType = DetermineIfIgnored(type);

        if (IsDoNotNormalize || IsIgnoredType)
        {
            Properties = Array.Empty<PropertyNormalizationMetadata>();
            StringProperties = Array.Empty<PropertyNormalizationMetadata>();
            ComplexProperties = Array.Empty<PropertyNormalizationMetadata>();
            CollectionProperties = Array.Empty<PropertyNormalizationMetadata>();
            return;
        }

        PropertyInfo[] publicProps = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var allProps = new List<PropertyNormalizationMetadata>(publicProps.Length);
        var stringProps = new List<PropertyNormalizationMetadata>();
        var complexProps = new List<PropertyNormalizationMetadata>();
        var collectionProps = new List<PropertyNormalizationMetadata>();

        for (int i = 0; i < publicProps.Length; i++)
        {
            PropertyInfo prop = publicProps[i];

            // Ignore indexer properties (e.g. this[int index])
            if (prop.GetIndexParameters().Length > 0)
            {
                continue;
            }

            var meta = new PropertyNormalizationMetadata(prop, options, serviceProvider);
            allProps.Add(meta);

            if (meta.IsString)
            {
                stringProps.Add(meta);
            }
            else if (meta.IsCollection || meta.IsDictionary)
            {
                collectionProps.Add(meta);
            }
            else if (meta.IsComplexType)
            {
                complexProps.Add(meta);
            }
        }

        Properties = allProps;
        StringProperties = stringProps;
        ComplexProperties = complexProps;
        CollectionProperties = collectionProps;
    }

    private static bool DetermineIfIgnored(Type type)
    {
        if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal))
        {
            return true;
        }

        if (type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(TimeSpan) ||
            type == typeof(Guid) || type == typeof(Uri))
        {
            return true;
        }

        // Exclude system namespace types that are not collections
        if (type.Namespace != null &&
            (type.Namespace.StartsWith("System.Reflection", StringComparison.Ordinal) ||
             type.Namespace.StartsWith("System.IO", StringComparison.Ordinal) ||
             type.Namespace.StartsWith("System.Threading", StringComparison.Ordinal)))
        {
            return true;
        }

        return false;
    }
}
