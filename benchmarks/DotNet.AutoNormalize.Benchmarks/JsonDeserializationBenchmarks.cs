using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using DotNet.AutoNormalize.Core;
using DotNet.AutoNormalize.Json.Extensions;

namespace DotNet.AutoNormalize.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class JsonDeserializationBenchmarks
{
    public class UserDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
    }

    private JsonSerializerOptions _defaultOptions = null!;
    private JsonSerializerOptions _autoNormalizeOptions = null!;
    private string _jsonPayload = null!;

    [GlobalSetup]
    public void Setup()
    {
        _defaultOptions = new JsonSerializerOptions();

        _autoNormalizeOptions = new JsonSerializerOptions().AddAutoNormalize(new AutoNormalizeOptions
        {
            TrimStrings = true,
            EmptyStringsToNull = true
        });

        _jsonPayload = """
        {
            "FirstName": "   Jane   ",
            "LastName": "   Doe   ",
            "Email": "   jane.doe@example.com   ",
            "PhoneNumber": "   +1-555-0199   ",
            "City": "   Seattle   ",
            "Country": "   United States   "
        }
        """;
    }

    [Benchmark(Baseline = true)]
    public UserDto? Deserialize_DefaultSystemTextJson()
    {
        return JsonSerializer.Deserialize<UserDto>(_jsonPayload, _defaultOptions);
    }

    [Benchmark]
    public UserDto? Deserialize_AutoNormalizeSystemTextJson()
    {
        return JsonSerializer.Deserialize<UserDto>(_jsonPayload, _autoNormalizeOptions);
    }
}
