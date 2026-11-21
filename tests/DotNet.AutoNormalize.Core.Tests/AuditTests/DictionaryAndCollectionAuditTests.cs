using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DotNet.AutoNormalize.Core;
using DotNet.AutoNormalize.Core.Engine;
using Xunit;

namespace DotNet.AutoNormalize.Core.Tests.AuditTests;

public class DictionaryAndCollectionAuditTests
{
    public class ComplexItem
    {
        public string? Name { get; set; }
    }

    [Fact]
    public void Normalize_DictionaryKeys_AreNeverMutated()
    {
        var normalizer = new AutoNormalizer();
        var dict = new Dictionary<string, string>
        {
            ["   KeyWithSpaces   "] = "   ValueWithSpaces   "
        };

        normalizer.Normalize(dict);

        // Keys MUST NOT be mutated to prevent hashcode corruption or collisions
        Assert.True(dict.ContainsKey("   KeyWithSpaces   "));
        Assert.False(dict.ContainsKey("KeyWithSpaces"));
        // Value MUST be normalized
        Assert.Equal("ValueWithSpaces", dict["   KeyWithSpaces   "]);
    }

    [Fact]
    public void Normalize_DictionaryWithCustomComparer_PreservesComparerBehavior()
    {
        var normalizer = new AutoNormalizer();
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["item_code"] = "   Normalized Value   "
        };

        normalizer.Normalize(dict);

        Assert.Equal("Normalized Value", dict["ITEM_CODE"]);
        Assert.Equal("Normalized Value", dict["item_code"]);
    }

    [Fact]
    public void Normalize_DictionaryStringObject_NormalizesBothStringsAndComplexObjects()
    {
        var normalizer = new AutoNormalizer();
        var dict = new Dictionary<string, object>
        {
            ["str"] = "   String Value   ",
            ["complex"] = new ComplexItem { Name = "   Nested Name   " }
        };

        normalizer.Normalize(dict);

        Assert.Equal("String Value", dict["str"]);
        var complex = Assert.IsType<ComplexItem>(dict["complex"]);
        Assert.Equal("Nested Name", complex.Name);
    }

    [Fact]
    public void Normalize_ReadOnlyDictionary_SafelySkippedWithoutException()
    {
        var normalizer = new AutoNormalizer();
        var underlying = new Dictionary<string, string> { ["key"] = "   untrimmed   " };
        var readOnlyDict = new ReadOnlyDictionary<string, string>(underlying);

        var result = normalizer.Normalize(readOnlyDict);

        Assert.NotNull(result);
        Assert.Equal("   untrimmed   ", readOnlyDict["key"]);
    }

    [Fact]
    public void Normalize_CollectionWithNullValues_PreservesNullsCorrectly()
    {
        var normalizer = new AutoNormalizer(new AutoNormalizeOptions
        {
            TrimStrings = true,
            NullToEmptyStrings = false
        });

        var list = new List<string?> { "   Valid   ", null, "   Another   ", null };

        normalizer.Normalize(list);

        Assert.Equal("Valid", list[0]);
        Assert.Null(list[1]);
        Assert.Equal("Another", list[2]);
        Assert.Null(list[3]);
    }

    [Fact]
    public void Normalize_DeepNestedCollections_NormalizesAllLevels()
    {
        var normalizer = new AutoNormalizer();
        var matrix = new List<List<string>>
        {
            new() { "   Row0_Col0   ", "   Row0_Col1   " },
            new() { "   Row1_Col0   ", "   Row1_Col1   " }
        };

        normalizer.Normalize(matrix);

        Assert.Equal("Row0_Col0", matrix[0][0]);
        Assert.Equal("Row0_Col1", matrix[0][1]);
        Assert.Equal("Row1_Col0", matrix[1][0]);
        Assert.Equal("Row1_Col1", matrix[1][1]);
    }
}
