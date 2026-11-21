using System;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using DotNet.AutoNormalize.Core;
using DotNet.AutoNormalize.Core.Engine;

namespace DotNet.AutoNormalize.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class ScalarStringBenchmarks
{
    private AutoNormalizer _normalizer = null!;
    private string _cleanString = null!;
    private string _dirtyString = null!;
    private string _whitespaceOnlyString = null!;
    private string _decomposedUnicode = null!;

    [GlobalSetup]
    public void Setup()
    {
        _normalizer = new AutoNormalizer(new AutoNormalizeOptions
        {
            TrimStrings = true,
            EmptyStringsToNull = true,
            UnicodeNormalization = NormalizationForm.FormC
        });

        // Construct runtime strings to prevent JIT constant-folding or dead-code elimination
        _cleanString = string.Concat("CleanStringWithoutWhitespace_", "RuntimeAllocated");
        _dirtyString = string.Concat("   ", "Dirty string with leading and trailing spaces", "   ");
        _whitespaceOnlyString = string.Concat("     ", "\t\r\n     ", "    ");
        _decomposedUnicode = string.Concat("Caf", "\u0065\u0301"); // Café decomposed
    }

    [Benchmark(Baseline = true)]
    public string? CleanString_ManualTrim()
    {
        return _cleanString.Trim();
    }

    [Benchmark]
    public string? CleanString_AutoNormalize()
    {
        return _normalizer.Normalize(_cleanString);
    }

    [Benchmark]
    public string? DirtyString_ManualTrim()
    {
        string trimmed = _dirtyString.Trim();
        return trimmed.Length == 0 ? null : trimmed;
    }

    [Benchmark]
    public string? DirtyString_AutoNormalize()
    {
        return _normalizer.Normalize(_dirtyString);
    }

    [Benchmark]
    public string? WhitespaceOnly_Manual()
    {
        string trimmed = _whitespaceOnlyString.Trim();
        return trimmed.Length == 0 ? null : trimmed;
    }

    [Benchmark]
    public string? WhitespaceOnly_AutoNormalize()
    {
        return _normalizer.Normalize(_whitespaceOnlyString);
    }

    [Benchmark]
    public string? DecomposedUnicode_Manual()
    {
        return _decomposedUnicode.IsNormalized(NormalizationForm.FormC)
            ? _decomposedUnicode
            : _decomposedUnicode.Normalize(NormalizationForm.FormC);
    }

    [Benchmark]
    public string? DecomposedUnicode_AutoNormalize()
    {
        return _normalizer.Normalize(_decomposedUnicode);
    }
}
