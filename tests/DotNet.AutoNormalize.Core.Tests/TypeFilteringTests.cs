using System;
using DotNet.AutoNormalize.Abstractions;
using DotNet.AutoNormalize.Core;
using DotNet.AutoNormalize.Core.Engine;
using Xunit;

namespace DotNet.AutoNormalize.Core.Tests
{
    public class TypeFilteringTests
    {
        [Fact]
        public void GlobalMode_WhenNoIncludesConfigured_NormalizesAllExceptExcludedNamespaces()
        {
            var options = new AutoNormalizeOptions();
            options.ExcludedNamespaces.Add("DotNet.AutoNormalize.Core.Tests.FilterFixtures.Requests.Legacy");

            var normalizer = new AutoNormalizer(options);

            var active = normalizer.Normalize(new FilterFixtures.Requests.ActiveRequest { Name = "  Ada  " })!;
            var nested = normalizer.Normalize(new FilterFixtures.Requests.V2.V2Request { Name = "  Grace  " })!;
            var legacy = normalizer.Normalize(new FilterFixtures.Requests.Legacy.LegacyRequest { Name = "  Bob  " })!;
            var legacyChild = normalizer.Normalize(new FilterFixtures.Requests.Legacy.Internal.DeepLegacyRequest { Name = "  Eve  " })!;
            var domain = normalizer.Normalize(new FilterFixtures.Domain.CustomerEntity { Name = "  Linus  " })!;

            Assert.Equal("Ada", active.Name);
            Assert.Equal("Grace", nested.Name);
            Assert.Equal("  Bob  ", legacy.Name);
            Assert.Equal("  Eve  ", legacyChild.Name);
            Assert.Equal("Linus", domain.Name);
        }

        [Fact]
        public void IncludedAndExcludedNamespaces_ExcludeWinsOverInclude_AndUsesDotBoundary()
        {
            var options = new AutoNormalizeOptions();
            options.IncludedNamespaces.Add("DotNet.AutoNormalize.Core.Tests.FilterFixtures.Requests");
            options.ExcludedNamespaces.Add("DotNet.AutoNormalize.Core.Tests.FilterFixtures.Requests.Legacy");

            var normalizer = new AutoNormalizer(options);

            var active = normalizer.Normalize(new FilterFixtures.Requests.ActiveRequest { Name = "  Ada  " })!;
            var v2 = normalizer.Normalize(new FilterFixtures.Requests.V2.V2Request { Name = "  Grace  " })!;
            var legacy = normalizer.Normalize(new FilterFixtures.Requests.Legacy.LegacyRequest { Name = "  Bob  " })!;
            var similarPrefix = normalizer.Normalize(new FilterFixtures.RequestsV2.SimilarPrefixRequest { Name = "  Alan  " })!;
            var domain = normalizer.Normalize(new FilterFixtures.Domain.CustomerEntity { Name = "  Linus  " })!;

            Assert.Equal("Ada", active.Name);
            Assert.Equal("Grace", v2.Name);
            Assert.Equal("  Bob  ", legacy.Name);
            Assert.Equal("  Alan  ", similarPrefix.Name); // Must NOT match "Requests" prefix without dot
            Assert.Equal("  Linus  ", domain.Name);
        }

        [Fact]
        public void IncludedAndExcludedTypes_ExcludeWins_AndOrSemanticsWithNamespaces()
        {
            var options = new AutoNormalizeOptions();
            options.IncludedNamespaces.Add("DotNet.AutoNormalize.Core.Tests.FilterFixtures.Requests");
            options.IncludedTypes.Add(typeof(FilterFixtures.Domain.CustomerEntity));
            options.ExcludedTypes.Add(typeof(FilterFixtures.Requests.ActiveRequest));

            var normalizer = new AutoNormalizer(options);

            var excludedRequest = normalizer.Normalize(new FilterFixtures.Requests.ActiveRequest { Name = "  Ada  " })!;
            var v2Request = normalizer.Normalize(new FilterFixtures.Requests.V2.V2Request { Name = "  Grace  " })!;
            var includedDomain = normalizer.Normalize(new FilterFixtures.Domain.CustomerEntity { Name = "  Linus  " })!;
            var otherDomain = normalizer.Normalize(new FilterFixtures.Domain.OrderEntity { Code = "  ORD-1  " })!;

            Assert.Equal("  Ada  ", excludedRequest.Name);
            Assert.Equal("Grace", v2Request.Name);
            Assert.Equal("Linus", includedDomain.Name);
            Assert.Equal("  ORD-1  ", otherDomain.Code);
        }

        [Fact]
        public void IncludedAndExcludedAssemblies_RespectsPrecedence()
        {
            var testAssembly = typeof(TypeFilteringTests).Assembly;
            var options = new AutoNormalizeOptions();
            options.IncludedAssemblies.Add(testAssembly);
            options.ExcludedAssemblies.Add(testAssembly);

            Assert.False(options.ShouldNormalizeType(typeof(FilterFixtures.Requests.ActiveRequest)));

            options.ExcludedAssemblies.Clear();
            Assert.True(options.ShouldNormalizeType(typeof(FilterFixtures.Requests.ActiveRequest)));
        }

        [Fact]
        public void TypeFilterPredicate_AppliedAfterIncludeExcludeRules()
        {
            var options = new AutoNormalizeOptions
            {
                TypeFilter = t => !t.Name.EndsWith("RawDto", StringComparison.Ordinal)
            };

            var normalizer = new AutoNormalizer(options);

            var regular = normalizer.Normalize(new FilterFixtures.Requests.ActiveRequest { Name = "  Ada  " })!;
            var raw = normalizer.Normalize(new FilterFixtures.Domain.WebhookRawDto { Payload = "  raw-body  " })!;

            Assert.Equal("Ada", regular.Name);
            Assert.Equal("  raw-body  ", raw.Payload);
        }

        [Fact]
        public void DoNotNormalizeAttribute_StillWinsOverExplicitIncludedTypes()
        {
            var options = new AutoNormalizeOptions();
            options.IncludedTypes.Add(typeof(FilterFixtures.Domain.OptedOutModel));

            Assert.False(options.ShouldNormalizeType(typeof(FilterFixtures.Domain.OptedOutModel)));
        }

        [Fact]
        public void NestedObjectGraph_SkipsExcludedNestedChildTypes()
        {
            var options = new AutoNormalizeOptions();
            options.ExcludedTypes.Add(typeof(FilterFixtures.Domain.WebhookRawDto));

            var normalizer = new AutoNormalizer(options);
            var parent = new FilterFixtures.Requests.WrapperRequest
            {
                Title = "  Hello  ",
                RawChild = new FilterFixtures.Domain.WebhookRawDto { Payload = "  keep-spaces  " }
            };

            normalizer.Normalize(parent);

            Assert.Equal("Hello", parent.Title);
            Assert.Equal("  keep-spaces  ", parent.RawChild.Payload);
        }

        [Fact]
        public void ShouldNormalizeType_NullType_ThrowsArgumentNullException()
        {
            var options = new AutoNormalizeOptions();
            Assert.Throws<ArgumentNullException>(() => options.ShouldNormalizeType(null!));
        }
    }
}

namespace DotNet.AutoNormalize.Core.Tests.FilterFixtures.Requests
{
    public class ActiveRequest { public string? Name { get; set; } }
    public class WrapperRequest
    {
        public string? Title { get; set; }
        public Domain.WebhookRawDto? RawChild { get; set; }
    }
}

namespace DotNet.AutoNormalize.Core.Tests.FilterFixtures.Requests.V2
{
    public class V2Request { public string? Name { get; set; } }
}

namespace DotNet.AutoNormalize.Core.Tests.FilterFixtures.Requests.Legacy
{
    public class LegacyRequest { public string? Name { get; set; } }
}

namespace DotNet.AutoNormalize.Core.Tests.FilterFixtures.Requests.Legacy.Internal
{
    public class DeepLegacyRequest { public string? Name { get; set; } }
}

namespace DotNet.AutoNormalize.Core.Tests.FilterFixtures.RequestsV2
{
    public class SimilarPrefixRequest { public string? Name { get; set; } }
}

namespace DotNet.AutoNormalize.Core.Tests.FilterFixtures.Domain
{
    public class CustomerEntity { public string? Name { get; set; } }
    public class OrderEntity { public string? Code { get; set; } }
    public class WebhookRawDto { public string? Payload { get; set; } }

    [DoNotNormalize]
    public class OptedOutModel { public string? Value { get; set; } }
}
