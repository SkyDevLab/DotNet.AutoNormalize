using System;
using System.Collections.Generic;
using System.Text;
using DotNet.AutoNormalize.Core;
using DotNet.AutoNormalize.Core.Engine;
using Microsoft.Extensions.Options;
using Xunit;

namespace DotNet.AutoNormalize.Core.Tests;

public class EdgeCaseAndOptionsTests
{
    public struct SimpleStruct
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class SuffixSensitiveModel
    {
        public string? UserDbPassword { get; set; }
        public string? AuthSecretKey { get; set; }
        public string? MasterPasscode { get; set; }
        public string? PlainText { get; set; }
    }

    public class MultiLevelCollectionModel
    {
        public List<List<string>>? Matrix { get; set; }
        public Dictionary<int, string>? NumericKeyDictionary { get; set; }
    }

    [Fact]
    public void Options_MutuallyExclusiveSettings_ThrowsInvalidOperationException()
    {
        var options = new AutoNormalizeOptions();
        options.EmptyStringsToNull = true;

        Assert.Throws<InvalidOperationException>(() =>
        {
            options.NullToEmptyStrings = true;
        });
    }

    [Fact]
    public void Options_InvalidMaxRecursionDepth_ThrowsArgumentOutOfRangeException()
    {
        var options = new AutoNormalizeOptions();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            options.MaxRecursionDepth = 0;
        });

        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            options.MaxRecursionDepth = -5;
        });
    }

    [Fact]
    public void Options_AddNullNormalizer_ThrowsArgumentNullException()
    {
        var options = new AutoNormalizeOptions();

        Assert.Throws<ArgumentNullException>(() =>
        {
            options.AddNormalizer(null!);
        });
    }

    [Fact]
    public void Normalize_IOptionsWrapper_InitializesCorrectly()
    {
        var options = Options.Create(new AutoNormalizeOptions
        {
            Casing = CasingTransform.UpperCase
        });

        var normalizer = new AutoNormalizer(options);

        string? result = normalizer.Normalize("  hello  ");
        Assert.Equal("HELLO", result);
    }

    [Fact]
    public void Normalize_UnicodeForms_NormalizesToSpecifiedForm()
    {
        // Decomposed FormD: 'e' + '\u0301'
        var formDNormalizer = new AutoNormalizer(new AutoNormalizeOptions
        {
            UnicodeNormalization = NormalizationForm.FormD
        });

        string precomposed = "\u00e9"; // é
        string? resultD = formDNormalizer.Normalize(precomposed);

        Assert.NotNull(resultD);
        Assert.True(resultD.IsNormalized(NormalizationForm.FormD));
        Assert.Equal("e\u0301", resultD);
    }

    [Fact]
    public void Normalize_ValueTypeStruct_DoesNotThrow()
    {
        var normalizer = new AutoNormalizer();
        var s = new SimpleStruct { Id = 1, Name = "  Test  " };

        var result = normalizer.Normalize(s);

        // Value types passed by value are boxed
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public void Normalize_SuffixSensitiveProperties_ShieldedFromTrimming()
    {
        var normalizer = new AutoNormalizer();
        var model = new SuffixSensitiveModel
        {
            UserDbPassword = "  db_pass  ",
            AuthSecretKey = "  secret_key  ",
            MasterPasscode = "  passcode_123  ",
            PlainText = "  normal text  "
        };

        normalizer.Normalize(model);

        Assert.Equal("  db_pass  ", model.UserDbPassword);
        Assert.Equal("  secret_key  ", model.AuthSecretKey);
        Assert.Equal("  passcode_123  ", model.MasterPasscode);
        Assert.Equal("normal text", model.PlainText);
    }

    [Fact]
    public void Normalize_MultiLevelCollection_NormalizesInnerListsAndDictionaries()
    {
        var normalizer = new AutoNormalizer();
        var model = new MultiLevelCollectionModel
        {
            Matrix = new List<List<string>>
            {
                new() { "  cell_0_0  ", "  cell_0_1  " },
                new() { "  cell_1_0  ", "  " }
            },
            NumericKeyDictionary = new Dictionary<int, string>
            {
                [100] = "  hundred  ",
                [200] = "  two hundred  "
            }
        };

        normalizer.Normalize(model);

        Assert.Equal("cell_0_0", model.Matrix[0][0]);
        Assert.Equal("cell_0_1", model.Matrix[0][1]);
        Assert.Equal("cell_1_0", model.Matrix[1][0]);
        Assert.Null(model.Matrix[1][1]);

        Assert.Equal("hundred", model.NumericKeyDictionary[100]);
        Assert.Equal("two hundred", model.NumericKeyDictionary[200]);
    }
}
