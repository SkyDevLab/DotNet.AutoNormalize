using System.Text.Json;
using DotNet.AutoNormalize.AspNetCore.ModelBinding;
using DotNet.AutoNormalize.Core;
using DotNet.AutoNormalize.Json.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Xunit;

namespace DotNet.AutoNormalize.AspNetCore.Tests
{
    public class TypeFilteringIntegrationTests
    {
        [Fact]
        public void SystemTextJson_RespectsIncludedAndExcludedNamespaces()
        {
            var normalizeOptions = new AutoNormalizeOptions();
            normalizeOptions.IncludedNamespaces.Add("DotNet.AutoNormalize.AspNetCore.Tests.JsonFilterFixtures.Included");
            normalizeOptions.ExcludedNamespaces.Add("DotNet.AutoNormalize.AspNetCore.Tests.JsonFilterFixtures.Included.Legacy");

            var jsonOptions = new JsonSerializerOptions().AddAutoNormalize(normalizeOptions);

            var included = JsonSerializer.Deserialize<JsonFilterFixtures.Included.CreateUserRecord>(
                """{"Name":"  Ada  "}""", jsonOptions)!;
            var legacy = JsonSerializer.Deserialize<JsonFilterFixtures.Included.Legacy.LegacyRecord>(
                """{"Name":"  Bob  "}""", jsonOptions)!;
            var other = JsonSerializer.Deserialize<JsonFilterFixtures.Other.ExternalRecord>(
                """{"Name":"  Eve  "}""", jsonOptions)!;

            Assert.Equal("Ada", included.Name);
            Assert.Equal("  Bob  ", legacy.Name);
            Assert.Equal("  Eve  ", other.Name);
        }

        [Fact]
        public void ModelBinderProvider_ReturnsNullWhenContainerTypeIsExcluded()
        {
            var options = new AutoNormalizeOptions();
            options.ExcludedTypes.Add(typeof(JsonFilterFixtures.Other.ExternalRecord));

            var provider = new AutoNormalizeModelBinderProvider(options);
            var metadataProvider = new EmptyModelMetadataProvider();

            var excludedPropMeta = metadataProvider.GetMetadataForProperty(
                typeof(JsonFilterFixtures.Other.ExternalRecord),
                nameof(JsonFilterFixtures.Other.ExternalRecord.Name));

            var includedPropMeta = metadataProvider.GetMetadataForProperty(
                typeof(JsonFilterFixtures.Included.CreateUserRecord),
                nameof(JsonFilterFixtures.Included.CreateUserRecord.Name));

            Assert.Null(provider.GetBinder(new StubModelBinderProviderContext(excludedPropMeta)));
            Assert.NotNull(provider.GetBinder(new StubModelBinderProviderContext(includedPropMeta)));
        }

        private sealed class StubModelBinderProviderContext : ModelBinderProviderContext
        {
            public StubModelBinderProviderContext(ModelMetadata metadata)
            {
                Metadata = metadata;
            }

            public override BindingInfo BindingInfo => new();
            public override ModelMetadata Metadata { get; }
            public override IModelMetadataProvider MetadataProvider => new EmptyModelMetadataProvider();
            public override IModelBinder CreateBinder(ModelMetadata metadata) => throw new System.NotImplementedException();
        }
    }
}

namespace DotNet.AutoNormalize.AspNetCore.Tests.JsonFilterFixtures.Included
{
    public record CreateUserRecord(string Name);
}

namespace DotNet.AutoNormalize.AspNetCore.Tests.JsonFilterFixtures.Included.Legacy
{
    public record LegacyRecord(string Name);
}

namespace DotNet.AutoNormalize.AspNetCore.Tests.JsonFilterFixtures.Other
{
    public record ExternalRecord(string Name);
}
