using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using DotNet.AutoNormalize.Abstractions;
using DotNet.AutoNormalize.Core;
using DotNet.AutoNormalize.Core.Engine;
using Xunit;

namespace DotNet.AutoNormalize.Core.Tests.AuditTests;

public class ConcurrencyAuditTests
{
    public class ConcurrencyItem
    {
        public int Id { get; set; }
        public string? Payload { get; set; }
        public NestedConcurrencyItem? Nested { get; set; }
    }

    public class NestedConcurrencyItem
    {
        public string? Data { get; set; }
    }

    public class CustomUpperNormalizer : IStringNormalizer
    {
        public string? Normalize(string? value, NormalizationContext context)
        {
            return value?.Trim().ToUpperInvariant();
        }
    }

    [Fact]
    public void ConcurrentOperations_1000ParallelIterations_ExecuteSafely()
    {
        var options = new AutoNormalizeOptions
        {
            TrimStrings = true,
            EmptyStringsToNull = true
        };
        options.AddNormalizer(new CustomUpperNormalizer());

        var normalizer = new AutoNormalizer(options);
        var exceptions = new ConcurrentBag<Exception>();

        // 1,000 parallel operations scheduled across the .NET ThreadPool
        Parallel.For(0, 1000, i =>
        {
            try
            {
                var item = new ConcurrencyItem
                {
                    Id = i,
                    Payload = $"   payload_{i}   ",
                    Nested = new NestedConcurrencyItem
                    {
                        Data = $"   nested_{i}   "
                    }
                };

                normalizer.Normalize(item);

                Assert.Equal($"PAYLOAD_{i}", item.Payload);
                Assert.NotNull(item.Nested);
                Assert.Equal($"NESTED_{i}", item.Nested.Data);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        });

        Assert.Empty(exceptions);
    }

    [Fact]
    public void MetadataCache_SimultaneousCacheMisses_ThreadSafeInitialization()
    {
        var normalizer = new AutoNormalizer();
        var exceptions = new ConcurrentBag<Exception>();

        // Force multiple threads to query metadata for distinct types simultaneously
        Parallel.For(0, 1000, i =>
        {
            try
            {
                Type typeToQuery = (i % 3) switch
                {
                    0 => typeof(ConcurrencyItem),
                    1 => typeof(NestedConcurrencyItem),
                    _ => typeof(CustomUpperNormalizer)
                };

                var meta = normalizer.MetadataCache.GetMetadata(typeToQuery);
                Assert.NotNull(meta);
                Assert.Equal(typeToQuery, meta.Type);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        });

        Assert.Empty(exceptions);
    }
}
