using DotNet.AutoNormalize.Abstractions;
using DotNet.AutoNormalize.Core.Engine;
using Xunit;

namespace DotNet.AutoNormalize.Core.Tests;

public class AttributeExclusionTests
{
    public class ModelWithAttributes
    {
        public string? NormalText { get; set; }

        [PreserveWhitespace]
        public string? PreservedWhitespace { get; set; }

        [PreserveWhitespace]
        public string? PreservedEmpty { get; set; }

        [DoNotNormalize]
        public string? CompletelyIgnored { get; set; }
    }

    [DoNotNormalize]
    public class FullyIgnoredClass
    {
        public string? Value1 { get; set; }
        public string? Value2 { get; set; }
    }

    [Fact]
    public void Normalize_PreserveWhitespaceAttribute_PreservesWhitespaceAndEmptyString()
    {
        var normalizer = new AutoNormalizer();
        var model = new ModelWithAttributes
        {
            NormalText = "   Trimmed Text   ",
            PreservedWhitespace = "   Preserved Text   ",
            PreservedEmpty = "     ",
            CompletelyIgnored = "   Untouched   "
        };

        normalizer.Normalize(model);

        Assert.Equal("Trimmed Text", model.NormalText);
        Assert.Equal("   Preserved Text   ", model.PreservedWhitespace);
        Assert.Equal("     ", model.PreservedEmpty);
        Assert.Equal("   Untouched   ", model.CompletelyIgnored);
    }

    [Fact]
    public void Normalize_ClassLevelDoNotNormalize_SkipsEntireClass()
    {
        var normalizer = new AutoNormalizer();
        var model = new FullyIgnoredClass
        {
            Value1 = "   First   ",
            Value2 = "   Second   "
        };

        normalizer.Normalize(model);

        Assert.Equal("   First   ", model.Value1);
        Assert.Equal("   Second   ", model.Value2);
    }
}
