using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using DotNet.AutoNormalize.Core;
using DotNet.AutoNormalize.Core.Engine;

namespace DotNet.AutoNormalize.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class DtoAndObjectGraphBenchmarks
{
    public class SinglePropertyDto
    {
        public string? Value { get; set; }
    }

    public class TypicalUserDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
    }

    public class NestedOrderDto
    {
        public string? OrderId { get; set; }
        public CustomerDto? Customer { get; set; }
        public List<OrderItemDto>? Items { get; set; }
    }

    public class CustomerDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
    }

    public class OrderItemDto
    {
        public string? Sku { get; set; }
        public string? Description { get; set; }
    }

    public class CollectionDto
    {
        public List<string> Items { get; set; } = new();
    }

    private AutoNormalizer _normalizer = null!;

    [GlobalSetup]
    public void Setup()
    {
        _normalizer = new AutoNormalizer(new AutoNormalizeOptions
        {
            TrimStrings = true,
            EmptyStringsToNull = true
        });
    }

    [Benchmark]
    public SinglePropertyDto SingleProperty_AutoNormalize()
    {
        var dto = new SinglePropertyDto { Value = "   Sample Value   " };
        _normalizer.Normalize(dto);
        return dto;
    }

    [Benchmark]
    public TypicalUserDto TypicalDto_AutoNormalize()
    {
        var dto = new TypicalUserDto
        {
            FirstName = "   Ada   ",
            LastName = "   Lovelace   ",
            Email = "   ada@computing.org   ",
            PhoneNumber = "   +1-555-0100   ",
            City = "   London   ",
            Country = "   United Kingdom   "
        };
        _normalizer.Normalize(dto);
        return dto;
    }

    [Benchmark]
    public NestedOrderDto NestedObject_AutoNormalize()
    {
        var dto = new NestedOrderDto
        {
            OrderId = "   ORD-9988   ",
            Customer = new CustomerDto
            {
                Name = "   Charles Babbage   ",
                Email = "   charles@difference.engine   "
            },
            Items = new List<OrderItemDto>
            {
                new() { Sku = "   SKU-1   ", Description = "   Item 1   " },
                new() { Sku = "   SKU-2   ", Description = "   Item 2   " }
            }
        };
        _normalizer.Normalize(dto);
        return dto;
    }

    [Benchmark]
    public CollectionDto Collection_AutoNormalize()
    {
        var dto = new CollectionDto();
        for (int i = 0; i < 50; i++)
        {
            dto.Items.Add($"   Collection Item {i}   ");
        }
        _normalizer.Normalize(dto);
        return dto;
    }
}
