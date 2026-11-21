using System.Text;
using DotNet.AutoNormalize.Abstractions;
using DotNet.AutoNormalize.Core;
using DotNet.AutoNormalize.Core.Engine;
using Xunit;

namespace DotNet.AutoNormalize.Core.Tests;

public class StringNormalizationTests
{
    [Fact]
    public void Normalize_NullInput_ReturnsNullByDefault()
    {
        var normalizer = new AutoNormalizer();
        string? result = normalizer.Normalize<string>(null);
        Assert.Null(result);
    }

    [Fact]
    public void Normalize_EmptyString_ReturnsNullWhenEmptyStringsToNullIsTrue()
    {
        var normalizer = new AutoNormalizer(new AutoNormalizeOptions
        {
            EmptyStringsToNull = true
        });

        string? result = normalizer.Normalize(string.Empty);
        Assert.Null(result);
    }

    [Fact]
    public void Normalize_EmptyString_ReturnsEmptyWhenEmptyStringsToNullIsFalse()
    {
        var normalizer = new AutoNormalizer(new AutoNormalizeOptions
        {
            EmptyStringsToNull = false
        });

        string? result = normalizer.Normalize(string.Empty);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Normalize_WhitespaceOnlyString_ReturnsNullWhenEmptyStringsToNullIsTrue()
    {
        var normalizer = new AutoNormalizer(new AutoNormalizeOptions
        {
            TrimStrings = true,
            EmptyStringsToNull = true
        });

        string? result = normalizer.Normalize("    \t\r\n   ");
        Assert.Null(result);
    }

    [Fact]
    public void Normalize_WhitespaceOnlyString_ReturnsEmptyWhenTrimmedAndEmptyStringsToNullIsFalse()
    {
        var normalizer = new AutoNormalizer(new AutoNormalizeOptions
        {
            TrimStrings = true,
            EmptyStringsToNull = false
        });

        string? result = normalizer.Normalize("    ");
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Normalize_NullInput_ReturnsEmptyWhenNullToEmptyStringsIsTrue()
    {
        var options = new AutoNormalizeOptions
        {
            EmptyStringsToNull = false,
            NullToEmptyStrings = true
        };
        var normalizer = new AutoNormalizer(options);

        string? result = normalizer.Normalize<string>(null);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Normalize_TrimModes_TrimsCorrectly()
    {
        var bothNormalizer = new AutoNormalizer(new AutoNormalizeOptions
        {
            TrimStrings = true,
            TrimMode = WhitespaceTrimMode.Both
        });
        Assert.Equal("Hello World", bothNormalizer.Normalize("   Hello World   "));

        var startNormalizer = new AutoNormalizer(new AutoNormalizeOptions
        {
            TrimStrings = true,
            TrimMode = WhitespaceTrimMode.Start
        });
        Assert.Equal("Hello World   ", startNormalizer.Normalize("   Hello World   "));

        var endNormalizer = new AutoNormalizer(new AutoNormalizeOptions
        {
            TrimStrings = true,
            TrimMode = WhitespaceTrimMode.End
        });
        Assert.Equal("   Hello World", endNormalizer.Normalize("   Hello World   "));
    }

    [Fact]
    public void Normalize_AlreadyTrimmedString_PreservesOriginalReference()
    {
        var normalizer = new AutoNormalizer(new AutoNormalizeOptions
        {
            TrimStrings = true,
            UnicodeNormalization = null,
            Casing = CasingTransform.None
        });

        string original = "AlreadyCleanString";
        string? result = normalizer.Normalize(original);

        Assert.True(ReferenceEquals(original, result));
    }

    [Fact]
    public void Normalize_UnicodeDecomposed_NormalizesToFormCByDefault()
    {
        var normalizer = new AutoNormalizer(new AutoNormalizeOptions
        {
            UnicodeNormalization = NormalizationForm.FormC
        });

        // "e" + combining acute accent (\u0301)
        string decomposed = "e\u0301";
        string? result = normalizer.Normalize(decomposed);

        // Precomposed "\u00e9" (é)
        Assert.Equal("\u00e9", result);
        Assert.True(result!.IsNormalized(NormalizationForm.FormC));
    }

    [Fact]
    public void Normalize_UnicodeAlreadyFormC_PreservesOriginalReference()
    {
        var normalizer = new AutoNormalizer(new AutoNormalizeOptions
        {
            TrimStrings = false,
            UnicodeNormalization = NormalizationForm.FormC,
            Casing = CasingTransform.None
        });

        string precomposed = "Caf\u00e9"; // Already FormC
        string? result = normalizer.Normalize(precomposed);

        Assert.True(ReferenceEquals(precomposed, result));
    }

    [Fact]
    public void Normalize_CasingTransforms_TransformsCorrectly()
    {
        var lowerNormalizer = new AutoNormalizer(new AutoNormalizeOptions
        {
            Casing = CasingTransform.LowerCase
        });
        Assert.Equal("test string", lowerNormalizer.Normalize("  TEST STRING  "));

        var upperNormalizer = new AutoNormalizer(new AutoNormalizeOptions
        {
            Casing = CasingTransform.UpperCase
        });
        Assert.Equal("TEST STRING", upperNormalizer.Normalize("  test string  "));
    }
}
