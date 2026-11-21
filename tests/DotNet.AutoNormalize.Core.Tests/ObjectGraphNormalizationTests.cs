using System.Collections.Generic;
using DotNet.AutoNormalize.Core.Engine;
using Xunit;

namespace DotNet.AutoNormalize.Core.Tests;

public class ObjectGraphNormalizationTests
{
    public class SimpleUserDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? EmptyBio { get; set; }
    }

    public class NestedCompanyDto
    {
        public string? CompanyName { get; set; }
        public SimpleUserDto? Founder { get; set; }
        public List<SimpleUserDto>? Employees { get; set; }
    }

    public class CollectionDto
    {
        public string[]? TagsArray { get; set; }
        public List<string>? TagsList { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
    }

    public record ImmutableRecord(string Name, string Role);

    public class InitOnlyDto
    {
        public string? NormalProperty { get; set; }
        public string? InitProperty { get; init; }
    }

    [Fact]
    public void Normalize_SimpleDto_NormalizesPropertiesInPlace()
    {
        var normalizer = new AutoNormalizer();
        var dto = new SimpleUserDto
        {
            FirstName = "   John  ",
            LastName = "  Doe   ",
            EmptyBio = "     "
        };

        var result = normalizer.Normalize(dto);

        Assert.Same(dto, result);
        Assert.Equal("John", dto.FirstName);
        Assert.Equal("Doe", dto.LastName);
        Assert.Null(dto.EmptyBio);
    }

    [Fact]
    public void Normalize_NestedDto_NormalizesAllLevels()
    {
        var normalizer = new AutoNormalizer();
        var company = new NestedCompanyDto
        {
            CompanyName = "  Acme Corp  ",
            Founder = new SimpleUserDto
            {
                FirstName = "  Alice  ",
                LastName = "  Smith  ",
                EmptyBio = "  "
            },
            Employees = new List<SimpleUserDto>
            {
                new() { FirstName = "  Bob  ", LastName = "  Jones  " },
                new() { FirstName = "  Charlie  ", LastName = "  Brown  " }
            }
        };

        normalizer.Normalize(company);

        Assert.Equal("Acme Corp", company.CompanyName);
        Assert.NotNull(company.Founder);
        Assert.Equal("Alice", company.Founder.FirstName);
        Assert.Equal("Smith", company.Founder.LastName);
        Assert.Null(company.Founder.EmptyBio);

        Assert.NotNull(company.Employees);
        Assert.Equal(2, company.Employees.Count);
        Assert.Equal("Bob", company.Employees[0].FirstName);
        Assert.Equal("Jones", company.Employees[0].LastName);
        Assert.Equal("Charlie", company.Employees[1].FirstName);
        Assert.Equal("Brown", company.Employees[1].LastName);
    }

    [Fact]
    public void Normalize_CollectionsAndDictionaries_NormalizesEntriesInPlace()
    {
        var normalizer = new AutoNormalizer();
        var dto = new CollectionDto
        {
            TagsArray = new[] { "  tag1  ", "  tag2  ", "   " },
            TagsList = new List<string> { "  alpha  ", "  beta  " },
            Metadata = new Dictionary<string, string>
            {
                ["key1"] = "   value1   ",
                ["key2"] = "   value2   "
            }
        };

        normalizer.Normalize(dto);

        Assert.Equal("tag1", dto.TagsArray[0]);
        Assert.Equal("tag2", dto.TagsArray[1]);
        Assert.Null(dto.TagsArray[2]);

        Assert.Equal("alpha", dto.TagsList[0]);
        Assert.Equal("beta", dto.TagsList[1]);

        Assert.Equal("value1", dto.Metadata["key1"]);
        Assert.Equal("value2", dto.Metadata["key2"]);
    }

    [Fact]
    public void Normalize_ImmutableRecord_DoesNotThrowAndPreservesRecord()
    {
        var normalizer = new AutoNormalizer();
        var record = new ImmutableRecord("  Alice  ", "  Admin  ");

        // Records have init-only properties. In-memory normalizer must safely skip mutating init-only properties
        // without throwing exceptions.
        var result = normalizer.Normalize(record);

        Assert.NotNull(result);
        Assert.Same(record, result);
        Assert.Equal("  Alice  ", record.Name);
    }

    [Fact]
    public void Normalize_InitOnlyProperties_NormalizesMutableAndSafelySkipsInitOnly()
    {
        var normalizer = new AutoNormalizer();
        var dto = new InitOnlyDto
        {
            NormalProperty = "   Mutable Value   ",
            InitProperty = "   Init-Only Value   "
        };

        normalizer.Normalize(dto);

        Assert.Equal("Mutable Value", dto.NormalProperty);
        Assert.Equal("   Init-Only Value   ", dto.InitProperty);
    }
}
