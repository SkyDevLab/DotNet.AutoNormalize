using System;
using DotNet.AutoNormalize.Abstractions;
using DotNet.AutoNormalize.Core;
using DotNet.AutoNormalize.Core.Engine;
using Xunit;

namespace DotNet.AutoNormalize.Core.Tests;

public class CustomNormalizerTests
{
    public sealed class EmailCanonicalNormalizer : IStringNormalizer
    {
        public string? Normalize(string? value, NormalizationContext context)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Trim().ToLowerInvariant();
        }
    }

    public sealed class SlugNormalizer : IStringNormalizer
    {
        public string? Normalize(string? value, NormalizationContext context)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Trim().Replace(' ', '-').ToLowerInvariant();
        }
    }

    public class ArticleModel
    {
        public string? Title { get; set; }

        [NormalizeWith(typeof(SlugNormalizer))]
        public string? Slug { get; set; }

        public string? AuthorEmail { get; set; }
    }

    [Fact]
    public void Normalize_GlobalCustomNormalizer_ExecutesInPipeline()
    {
        var options = new AutoNormalizeOptions();
        options.AddNormalizer(new EmailCanonicalNormalizer());

        var normalizer = new AutoNormalizer(options);

        string? result = normalizer.Normalize("   SURYA@EXAMPLE.COM   ");
        Assert.Equal("surya@example.com", result);
    }

    [Fact]
    public void Normalize_PropertyWithNormalizeWithAttribute_ExecutesCustomNormalizer()
    {
        var normalizer = new AutoNormalizer();
        var article = new ArticleModel
        {
            Title = "   DotNet AutoNormalize Announcement   ",
            Slug = "   DotNet AutoNormalize Announcement   ",
            AuthorEmail = "  AUTHOR@DOMAIN.COM  "
        };

        normalizer.Normalize(article);

        Assert.Equal("DotNet AutoNormalize Announcement", article.Title);
        Assert.Equal("dotnet-autonormalize-announcement", article.Slug);
        Assert.Equal("AUTHOR@DOMAIN.COM", article.AuthorEmail);
    }
}
