using System;
using System.Collections;
using System.Collections.Generic;
using DotNet.AutoNormalize.Abstractions;

namespace DotNet.AutoNormalize.Core.Engine;

/// <summary>
/// Traverses and normalizes complex object graphs, nested objects, and collections with cycle protection.
/// </summary>
public sealed class ObjectGraphNormalizer
{
    private readonly AutoNormalizeOptions _options;
    private readonly NormalizationMetadataCache _metadataCache;
    private readonly StringNormalizerPipeline _pipeline;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectGraphNormalizer"/> class.
    /// </summary>
    /// <param name="options">The auto-normalize options.</param>
    /// <param name="metadataCache">The metadata cache.</param>
    /// <param name="pipeline">The string normalizer pipeline.</param>
    public ObjectGraphNormalizer(
        AutoNormalizeOptions options,
        NormalizationMetadataCache metadataCache,
        StringNormalizerPipeline pipeline)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _metadataCache = metadataCache ?? throw new ArgumentNullException(nameof(metadataCache));
        _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
    }

    internal static readonly NormalizationContext DefaultContext = new();

    /// <summary>
    /// Recursively normalizes the target object.
    /// </summary>
    /// <param name="target">The target object to normalize.</param>
    /// <param name="targetType">The declared or runtime type of the target.</param>
    /// <param name="context">Optional normalization context.</param>
    /// <returns>The normalized target object.</returns>
    public object? Normalize(object? target, Type targetType, NormalizationContext? context = null)
    {
        if (target is null)
        {
            if (targetType == typeof(string) && _options.NullToEmptyStrings)
            {
                return string.Empty;
            }
            return null;
        }

        // Fast-path: If target itself is a string scalar, bypass object graph traversal and visited tracking entirely
        if (target is string str)
        {
            return _pipeline.Normalize(str, context ?? DefaultContext);
        }

        context ??= new NormalizationContext
        {
            TargetType = targetType
        };

        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        NormalizeCore(target, target.GetType(), context, visited, depth: 0);
        return target;
    }

    private void NormalizeCore(
        object target,
        Type runtimeType,
        NormalizationContext context,
        HashSet<object> visited,
        int depth)
    {
        if (target is null)
        {
            return;
        }

        // 1. Guard against unbounded recursion
        if (depth > _options.MaxRecursionDepth)
        {
            return;
        }

        // 2. Cycle detection for reference types
        if (!runtimeType.IsValueType)
        {
            if (!visited.Add(target))
            {
                // Already visited in the current graph traversal; prevent cycles
                return;
            }
        }

        // 3. Handle collections and arrays
        if (target is IList list)
        {
            NormalizeList(list, context, visited, depth);
            return;
        }

        if (target is IDictionary dictionary)
        {
            NormalizeDictionary(dictionary, context, visited, depth);
            return;
        }

        if (target is IEnumerable enumerable && target is not string)
        {
            NormalizeEnumerable(enumerable, context, visited, depth);
            return;
        }

        // 4. Inspect type metadata
        TypeNormalizationMetadata typeMeta = _metadataCache.GetMetadata(runtimeType);
        if (typeMeta.IsDoNotNormalize || typeMeta.IsIgnoredType)
        {
            return;
        }

        // 5. Normalize direct string properties
        for (int i = 0; i < typeMeta.StringProperties.Count; i++)
        {
            PropertyNormalizationMetadata propMeta = typeMeta.StringProperties[i];
            if (!propMeta.CanRead || !propMeta.CanWrite || propMeta.DoNotNormalize)
            {
                continue;
            }

            string? currentVal = (string?)propMeta.PropertyInfo.GetValue(target);
            var propertyContext = new NormalizationContext
            {
                PropertyInfo = propMeta.PropertyInfo,
                TargetType = runtimeType,
                Services = context.Services
            };

            string? normalizedVal = _pipeline.Normalize(currentVal, propertyContext, propMeta);

            if (!ReferenceEquals(currentVal, normalizedVal) && currentVal != normalizedVal)
            {
                propMeta.PropertyInfo.SetValue(target, normalizedVal);
            }
        }

        // 6. Recursively normalize nested complex properties
        for (int i = 0; i < typeMeta.ComplexProperties.Count; i++)
        {
            PropertyNormalizationMetadata propMeta = typeMeta.ComplexProperties[i];
            if (!propMeta.CanRead || propMeta.DoNotNormalize)
            {
                continue;
            }

            object? child = propMeta.PropertyInfo.GetValue(target);
            if (child is not null)
            {
                NormalizeCore(child, child.GetType(), context, visited, depth + 1);
            }
        }

        // 7. Recursively normalize nested collection properties
        for (int i = 0; i < typeMeta.CollectionProperties.Count; i++)
        {
            PropertyNormalizationMetadata propMeta = typeMeta.CollectionProperties[i];
            if (!propMeta.CanRead || propMeta.DoNotNormalize)
            {
                continue;
            }

            object? col = propMeta.PropertyInfo.GetValue(target);
            if (col is not null)
            {
                NormalizeCore(col, col.GetType(), context, visited, depth + 1);
            }
        }
    }

    private void NormalizeList(IList list, NormalizationContext context, HashSet<object> visited, int depth)
    {
        if (list.IsReadOnly)
        {
            return;
        }

        for (int i = 0; i < list.Count; i++)
        {
            object? item = list[i];
            if (item is null)
            {
                continue;
            }

            if (item is string strItem)
            {
                string? normalized = _pipeline.Normalize(strItem, context);
                if (!ReferenceEquals(strItem, normalized) && strItem != normalized)
                {
                    list[i] = normalized;
                }
            }
            else if (!item.GetType().IsValueType && item.GetType() != typeof(string))
            {
                NormalizeCore(item, item.GetType(), context, visited, depth + 1);
            }
        }
    }

    private void NormalizeDictionary(IDictionary dict, NormalizationContext context, HashSet<object> visited, int depth)
    {
        if (dict.IsReadOnly)
        {
            return;
        }

        var keys = new ArrayList(dict.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            object? key = keys[i];
            if (key is null)
            {
                continue;
            }

            object? val = dict[key];
            if (val is null)
            {
                continue;
            }

            if (val is string strVal)
            {
                string? normalized = _pipeline.Normalize(strVal, context);
                if (!ReferenceEquals(strVal, normalized) && strVal != normalized)
                {
                    dict[key] = normalized;
                }
            }
            else if (!val.GetType().IsValueType && val.GetType() != typeof(string))
            {
                NormalizeCore(val, val.GetType(), context, visited, depth + 1);
            }
        }
    }

    private void NormalizeEnumerable(IEnumerable enumerable, NormalizationContext context, HashSet<object> visited, int depth)
    {
        foreach (object? item in enumerable)
        {
            if (item != null && !item.GetType().IsValueType && item is not string)
            {
                NormalizeCore(item, item.GetType(), context, visited, depth + 1);
            }
        }
    }
}
