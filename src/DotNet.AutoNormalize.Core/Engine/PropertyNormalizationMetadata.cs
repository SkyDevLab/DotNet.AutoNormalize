using System;
using System.Reflection;
using DotNet.AutoNormalize.Abstractions;

namespace DotNet.AutoNormalize.Core.Engine;

/// <summary>
/// Cached metadata describing the normalization rules and reflection properties of a specific property.
/// </summary>
public sealed class PropertyNormalizationMetadata
{
    /// <summary>
    /// Gets the underlying reflection <see cref="PropertyInfo"/>.
    /// </summary>
    public PropertyInfo PropertyInfo { get; }

    /// <summary>
    /// Gets the name of the property.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Gets the CLR type of the property.
    /// </summary>
    public Type PropertyType { get; }

    /// <summary>
    /// Gets a value indicating whether this property is of type <see cref="string"/>.
    /// </summary>
    public bool IsString { get; }

    /// <summary>
    /// Gets a value indicating whether this property is a collection (implements <see cref="System.Collections.IEnumerable"/> and is not a string).
    /// </summary>
    public bool IsCollection { get; }

    /// <summary>
    /// Gets a value indicating whether this property is a dictionary (implements <see cref="System.Collections.IDictionary"/>).
    /// </summary>
    public bool IsDictionary { get; }

    /// <summary>
    /// Gets a value indicating whether this property is a complex reference type requiring nested traversal.
    /// </summary>
    public bool IsComplexType { get; }

    /// <summary>
    /// Gets a value indicating whether the property can be read.
    /// </summary>
    public bool CanRead { get; }

    /// <summary>
    /// Gets a value indicating whether the property can be safely written to (has a public setter and is not init-only).
    /// </summary>
    public bool CanWrite { get; }

    /// <summary>
    /// Gets a value indicating whether the property has an init-only (<c>init;</c>) setter.
    /// </summary>
    public bool IsInitOnly { get; }

    /// <summary>
    /// Gets a value indicating whether the property is recognized as a sensitive credential or token.
    /// </summary>
    public bool IsSensitive { get; }

    /// <summary>
    /// Gets a value indicating whether whitespace trimming should be preserved.
    /// </summary>
    public bool PreserveWhitespace { get; }

    /// <summary>
    /// Gets a value indicating whether normalization is completely disabled for this property.
    /// </summary>
    public bool DoNotNormalize { get; }

    /// <summary>
    /// Gets the custom string normalizer configured via <see cref="NormalizeWithAttribute"/>, if any.
    /// </summary>
    public IStringNormalizer? CustomNormalizer { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyNormalizationMetadata"/> class.
    /// </summary>
    public PropertyNormalizationMetadata(
        PropertyInfo propertyInfo,
        AutoNormalizeOptions options,
        IServiceProvider? serviceProvider)
    {
        PropertyInfo = propertyInfo ?? throw new ArgumentNullException(nameof(propertyInfo));
        PropertyName = propertyInfo.Name;
        PropertyType = propertyInfo.PropertyType;

        IsString = PropertyType == typeof(string);
        CanRead = propertyInfo.CanRead && propertyInfo.GetMethod != null;

        IsInitOnly = CheckIsInitOnly(propertyInfo);
        CanWrite = propertyInfo.CanWrite && propertyInfo.SetMethod != null && propertyInfo.SetMethod.IsPublic && !IsInitOnly;

        // Check attributes
        DoNotNormalize = propertyInfo.IsDefined(typeof(DoNotNormalizeAttribute), true);
        PreserveWhitespace = propertyInfo.IsDefined(typeof(PreserveWhitespaceAttribute), true);

        // Check sensitive naming and DataType attributes
        IsSensitive = CheckIsSensitive(propertyInfo, options);

        // Check [NormalizeWith] attribute
        CustomNormalizer = ResolveCustomNormalizer(propertyInfo, serviceProvider);

        // Determine if collection / dictionary / complex
        if (!IsString)
        {
            IsDictionary = typeof(System.Collections.IDictionary).IsAssignableFrom(PropertyType) ||
                           IsGenericDictionary(PropertyType);
            IsCollection = typeof(System.Collections.IEnumerable).IsAssignableFrom(PropertyType);
            IsComplexType = !PropertyType.IsPrimitive &&
                            !PropertyType.IsEnum &&
                            !PropertyType.IsValueType &&
                            PropertyType != typeof(string) &&
                            !PropertyType.FullName!.StartsWith("System.", StringComparison.Ordinal);
        }
    }

    private static bool CheckIsInitOnly(PropertyInfo property)
    {
        MethodInfo? setMethod = property.SetMethod;
        if (setMethod is null) return false;

        Type[] modifiers = setMethod.ReturnParameter.GetRequiredCustomModifiers();
        for (int i = 0; i < modifiers.Length; i++)
        {
            if (modifiers[i].FullName == "System.Runtime.CompilerServices.IsExternalInit")
            {
                return true;
            }
        }

        return false;
    }

    private static bool CheckIsSensitive(PropertyInfo property, AutoNormalizeOptions options)
    {
        // 1. Check heuristics in options
        if (options.ProtectSensitiveProperties && options.SensitivePropertyNames.Contains(property.Name))
        {
            return true;
        }

        // 2. Check suffix convention (e.g. UserPassword, ClientSecret)
        if (options.ProtectSensitiveProperties)
        {
            foreach (string sensitiveName in options.SensitivePropertyNames)
            {
                if (property.Name.EndsWith(sensitiveName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        // 3. Check System.ComponentModel.DataAnnotations.DataTypeAttribute if present
        var customAttributes = property.GetCustomAttributes(true);
        for (int i = 0; i < customAttributes.Length; i++)
        {
            var attrType = customAttributes[i].GetType();
            if (attrType.FullName == "System.ComponentModel.DataAnnotations.DataTypeAttribute")
            {
                var dataTypeProp = attrType.GetProperty("DataType");
                if (dataTypeProp != null)
                {
                    var val = dataTypeProp.GetValue(customAttributes[i]);
                    // DataType.Password has int value 11
                    if (val is not null && val.ToString() == "Password")
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private static IStringNormalizer? ResolveCustomNormalizer(PropertyInfo property, IServiceProvider? serviceProvider)
    {
        var attr = property.GetCustomAttribute<NormalizeWithAttribute>(true);
        if (attr is null) return null;

        Type normalizerType = attr.NormalizerType;
        if (!typeof(IStringNormalizer).IsAssignableFrom(normalizerType) &&
            !typeof(INormalizer<string?>).IsAssignableFrom(normalizerType))
        {
            throw new InvalidOperationException(
                $"The type '{normalizerType.FullName}' specified on property '{property.DeclaringType?.Name}.{property.Name}' does not implement '{nameof(IStringNormalizer)}' or '{nameof(INormalizer<string?>)}'.");
        }

        if (serviceProvider != null)
        {
            var service = serviceProvider.GetService(normalizerType);
            if (service is IStringNormalizer normalizerFromDi)
            {
                return normalizerFromDi;
            }
        }

        return (IStringNormalizer)Activator.CreateInstance(normalizerType)!;
    }

    private static bool IsGenericDictionary(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Collections.Generic.IDictionary<,>))
        {
            return true;
        }

        Type[] interfaces = type.GetInterfaces();
        for (int i = 0; i < interfaces.Length; i++)
        {
            if (interfaces[i].IsGenericType && interfaces[i].GetGenericTypeDefinition() == typeof(System.Collections.Generic.IDictionary<,>))
            {
                return true;
            }
        }

        return false;
    }
}
