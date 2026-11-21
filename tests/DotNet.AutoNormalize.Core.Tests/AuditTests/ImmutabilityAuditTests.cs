using System.Text.Json;
using DotNet.AutoNormalize.Core.Engine;
using DotNet.AutoNormalize.Json.Extensions;
using Xunit;

namespace DotNet.AutoNormalize.Core.Tests.AuditTests;

/// <summary>
/// Verifies the precise architectural behavior of immutable records and init-only properties.
/// In-memory standalone normalization safely skips immutable properties (since C# disallows post-construction mutation).
/// JSON deserialization normalizes them at constructor-instantiation time.
/// </summary>
public class ImmutabilityAuditTests
{
    public record PositionalUser(string FullName, string Email);

    public record NestedOrderRecord(string OrderId, PositionalUser Customer);

    public class InitOnlyUser
    {
        public string? Username { get; init; }
        public string? Bio { get; init; }
    }

    [Fact]
    public void StandaloneCore_PositionalRecord_SafelyPreservedWithoutMutationOrException()
    {
        var normalizer = new AutoNormalizer();
        var record = new PositionalUser("   Ada Lovelace   ", "   ada@example.com   ");

        // Act: Standalone in-memory normalization
        var result = normalizer.Normalize(record);

        // Assert:
        // 1. Must not throw runtime reflection exceptions.
        // 2. Must return the same reference.
        // 3. Properties cannot be mutated post-construction in memory.
        Assert.NotNull(result);
        Assert.Same(record, result);
        Assert.Equal("   Ada Lovelace   ", result.FullName);
        Assert.Equal("   ada@example.com   ", result.Email);
    }

    [Fact]
    public void StandaloneCore_InitOnlyClass_SafelySkipsInitOnlyPropertiesWithoutException()
    {
        var normalizer = new AutoNormalizer();
        var user = new InitOnlyUser
        {
            Username = "   alan_turing   ",
            Bio = "   Cryptanalyst   "
        };

        var result = normalizer.Normalize(user);

        Assert.NotNull(result);
        Assert.Same(user, result);
        Assert.Equal("   alan_turing   ", user.Username);
        Assert.Equal("   Cryptanalyst   ", user.Bio);
    }

    [Fact]
    public void StandaloneCore_NestedImmutableRecord_SafelySkipsNestedRecordProperties()
    {
        var normalizer = new AutoNormalizer();
        var order = new NestedOrderRecord("   ORD-1001   ", new PositionalUser("   Charles Babbage   ", "   cb@engine.org   "));

        var result = normalizer.Normalize(order);

        Assert.NotNull(result);
        Assert.Same(order, result);
        Assert.Equal("   ORD-1001   ", result.OrderId);
        Assert.Equal("   Charles Babbage   ", result.Customer.FullName);
    }

    [Fact]
    public void SystemTextJson_PositionalRecord_NormalizesAtConstructorTime()
    {
        var options = new JsonSerializerOptions().AddAutoNormalize();
        string json = """
        {
            "FullName": "   Ada Lovelace   ",
            "Email": "   ada@example.com   "
        }
        """;

        var record = JsonSerializer.Deserialize<PositionalUser>(json, options);

        Assert.NotNull(record);
        // During JSON deserialization, the converter normalizes parameters before constructor invocation
        Assert.Equal("Ada Lovelace", record.FullName);
        Assert.Equal("ada@example.com", record.Email);
    }

    [Fact]
    public void SystemTextJson_NestedRecord_NormalizesAllLevelsAtConstructorTime()
    {
        var options = new JsonSerializerOptions().AddAutoNormalize();
        string json = """
        {
            "OrderId": "   ORD-5544   ",
            "Customer": {
                "FullName": "   Grace Hopper   ",
                "Email": "   grace@navy.mil   "
            }
        }
        """;

        var order = JsonSerializer.Deserialize<NestedOrderRecord>(json, options);

        Assert.NotNull(order);
        Assert.Equal("ORD-5544", order.OrderId);
        Assert.NotNull(order.Customer);
        Assert.Equal("Grace Hopper", order.Customer.FullName);
        Assert.Equal("grace@navy.mil", order.Customer.Email);
    }
}
