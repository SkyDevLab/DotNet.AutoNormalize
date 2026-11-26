using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using DotNet.AutoNormalize.Abstractions;

namespace DotNet.AutoNormalize.Core;

/// <summary>
/// Configuration options governing string and object normalization.
/// </summary>
public sealed class AutoNormalizeOptions
{
    private bool _emptyStringsToNull = true;
    private bool _nullToEmptyStrings = false;
    private int _maxRecursionDepth = 32;

    /// <summary>
    /// Gets or sets a value indicating whether leading and trailing whitespace should be trimmed from strings.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool TrimStrings { get; set; } = true;

    /// <summary>
    /// Gets or sets how whitespace is trimmed when <see cref="TrimStrings"/> is enabled.
    /// Defaults to <see cref="WhitespaceTrimMode.Both"/>.
    /// </summary>
    public WhitespaceTrimMode TrimMode { get; set; } = WhitespaceTrimMode.Both;

    /// <summary>
    /// Gets or sets a value indicating whether empty or whitespace-only strings should be converted to <c>null</c>.
    /// Defaults to <c>true</c>. Cannot be enabled simultaneously with <see cref="NullToEmptyStrings"/>.
    /// </summary>
    public bool EmptyStringsToNull
    {
        get => _emptyStringsToNull;
        set
        {
            if (value && _nullToEmptyStrings)
            {
                throw new InvalidOperationException(
                    $"Cannot enable '{nameof(EmptyStringsToNull)}' when '{nameof(NullToEmptyStrings)}' is also enabled. These options are mutually exclusive.");
            }
            _emptyStringsToNull = value;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether <c>null</c> strings should be converted to empty strings (<c>""</c>).
    /// Defaults to <c>false</c>. Cannot be enabled simultaneously with <see cref="EmptyStringsToNull"/>.
    /// </summary>
    public bool NullToEmptyStrings
    {
        get => _nullToEmptyStrings;
        set
        {
            if (value && _emptyStringsToNull)
            {
                throw new InvalidOperationException(
                    $"Cannot enable '{nameof(NullToEmptyStrings)}' when '{nameof(EmptyStringsToNull)}' is also enabled. These options are mutually exclusive.");
            }
            _nullToEmptyStrings = value;
        }
    }

    /// <summary>
    /// Gets or sets the target Unicode normalization form, or <c>null</c> to disable Unicode normalization.
    /// Defaults to <see cref="NormalizationForm.FormC"/>.
    /// </summary>
    public NormalizationForm? UnicodeNormalization { get; set; } = NormalizationForm.FormC;

    /// <summary>
    /// Gets or sets the casing transformation applied to strings.
    /// Defaults to <see cref="CasingTransform.None"/>.
    /// </summary>
    public CasingTransform Casing { get; set; } = CasingTransform.None;

    /// <summary>
    /// Gets or sets whether heuristic protection of common sensitive property names is enabled.
    /// Defaults to <c>true</c>.
    /// <para>
    /// Notice: This is a defense-in-depth safety net, not a security guarantee. Sensitive properties
    /// should always be explicitly decorated with <see cref="PreserveWhitespaceAttribute"/> or <see cref="DoNotNormalizeAttribute"/>.
    /// </para>
    /// </summary>
    public bool ProtectSensitiveProperties { get; set; } = true;

    /// <summary>
    /// Gets the set of property names automatically shielded from whitespace trimming and empty-to-null
    /// transformations when <see cref="ProtectSensitiveProperties"/> is enabled.
    /// Case-insensitive.
    /// </summary>
    public ISet<string> SensitivePropertyNames { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Password", "Passcode", "Passphrase", "Token", "AuthToken", "AccessToken", "RefreshToken", "IdToken",
        "Secret", "ClientSecret", "SecretKey", "ApiKey", "PrivateKey", "Signature", "Hash", "Salt"
    };

    /// <summary>
    /// Gets or sets the maximum recursion depth allowed when traversing nested object graphs.
    /// Defaults to 32. Must be greater than 0.
    /// </summary>
    public int MaxRecursionDepth
    {
        get => _maxRecursionDepth;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "MaxRecursionDepth must be greater than 0.");
            }
            _maxRecursionDepth = value;
        }
    }

    /// <summary>
    /// Gets the ordered list of custom string normalizers to execute.
    /// </summary>
    public IList<IStringNormalizer> CustomNormalizers { get; } = new List<IStringNormalizer>();

    /// <summary>
    /// Gets the set of namespace prefixes to include for normalization.
    /// When any include filter (<see cref="IncludedNamespaces"/>, <see cref="IncludedTypes"/>, or <see cref="IncludedAssemblies"/>)
    /// is non-empty, only matching types are normalized. Excluded rules always take precedence.
    /// </summary>
    public ISet<string> IncludedNamespaces { get; } = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets the set of namespace prefixes to exclude from normalization.
    /// Exclusion rules always take precedence over inclusion rules.
    /// </summary>
    public ISet<string> ExcludedNamespaces { get; } = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets the set of specific types to include for normalization.
    /// </summary>
    public ISet<Type> IncludedTypes { get; } = new HashSet<Type>();

    /// <summary>
    /// Gets the set of specific types to exclude from normalization.
    /// Exclusion rules always take precedence over inclusion rules.
    /// </summary>
    public ISet<Type> ExcludedTypes { get; } = new HashSet<Type>();

    /// <summary>
    /// Gets the set of assemblies whose types should be included for normalization.
    /// </summary>
    public ISet<Assembly> IncludedAssemblies { get; } = new HashSet<Assembly>();

    /// <summary>
    /// Gets the set of assemblies whose types should be excluded from normalization.
    /// Exclusion rules always take precedence over inclusion rules.
    /// </summary>
    public ISet<Assembly> ExcludedAssemblies { get; } = new HashSet<Assembly>();

    /// <summary>
    /// Gets or sets an optional predicate to filter which types are eligible for normalization.
    /// Executed after namespace, type, and assembly filters; returning <c>false</c> skips normalization for that type.
    /// </summary>
    public Func<Type, bool>? TypeFilter { get; set; }

    /// <summary>
    /// Determines whether the specified <paramref name="type"/> is eligible for normalization
    /// according to <see cref="DoNotNormalizeAttribute"/>, exclusion rules, inclusion rules, and <see cref="TypeFilter"/>.
    /// </summary>
    /// <param name="type">The CLR type to evaluate.</param>
    /// <returns><c>true</c> if the type should be normalized; otherwise, <c>false</c>.</returns>
    public bool ShouldNormalizeType(Type type)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        if (type.IsDefined(typeof(DoNotNormalizeAttribute), true))
        {
            return false;
        }

        if (ExcludedTypes.Contains(type) ||
            ExcludedAssemblies.Contains(type.Assembly) ||
            MatchesAnyNamespace(type.Namespace, ExcludedNamespaces))
        {
            return false;
        }

        bool hasIncludes = IncludedTypes.Count > 0 ||
                           IncludedAssemblies.Count > 0 ||
                           IncludedNamespaces.Count > 0;

        if (hasIncludes &&
            !IncludedTypes.Contains(type) &&
            !IncludedAssemblies.Contains(type.Assembly) &&
            !MatchesAnyNamespace(type.Namespace, IncludedNamespaces))
        {
            return false;
        }

        if (TypeFilter is not null && !TypeFilter(type))
        {
            return false;
        }

        return true;
    }

    private static bool MatchesAnyNamespace(string? typeNamespace, ISet<string> rules)
    {
        if (string.IsNullOrEmpty(typeNamespace) || rules.Count == 0)
        {
            return false;
        }

        foreach (string rule in rules)
        {
            if (string.IsNullOrWhiteSpace(rule))
            {
                continue;
            }

            if (typeNamespace!.Equals(rule, StringComparison.Ordinal) ||
                typeNamespace.StartsWith(rule + ".", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Adds a custom normalizer to the normalization pipeline.
    /// </summary>
    /// <param name="normalizer">The normalizer to add.</param>
    /// <returns>This options instance for fluent chaining.</returns>
    public AutoNormalizeOptions AddNormalizer(IStringNormalizer normalizer)
    {
        if (normalizer is null)
        {
            throw new ArgumentNullException(nameof(normalizer));
        }
        CustomNormalizers.Add(normalizer);
        return this;
    }

    /// <summary>
    /// Validates the configuration state of this options instance.
    /// </summary>
    public void Validate()
    {
        if (EmptyStringsToNull && NullToEmptyStrings)
        {
            throw new InvalidOperationException(
                $"Configuration conflict: '{nameof(EmptyStringsToNull)}' and '{nameof(NullToEmptyStrings)}' cannot both be true.");
        }

        if (MaxRecursionDepth <= 0)
        {
            throw new InvalidOperationException(
                $"'{nameof(MaxRecursionDepth)}' must be greater than zero.");
        }
    }
}
