using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using DotNet.AutoNormalize.Core;
using DotNet.AutoNormalize.Core.Engine;

namespace DotNet.AutoNormalize.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class ObjectGraphBenchmarks
{
    public class OrderDto
    {
        public string? OrderNumber { get; set; }
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

    private static OrderDto CreateOrder() => new OrderDto
    {
        OrderNumber = "   ORD-998822   ",
        Customer = new CustomerDto
        {
            Name = "   Ada Lovelace   ",
            Email = "   ada@example.org   "
        },
        Items = new List<OrderItemDto>
        {
            new() { Sku = "   SKU-1001   ", Description = "   Item One   " },
            new() { Sku = "   SKU-1002   ", Description = "   Item Two   " },
            new() { Sku = "   SKU-1003   ", Description = "   Item Three   " }
        }
    };

    [Benchmark(Baseline = true)]
    public OrderDto ManualNormalization()
    {
        var order = CreateOrder();
        order.OrderNumber = order.OrderNumber?.Trim();
        if (order.Customer != null)
        {
            order.Customer.Name = order.Customer.Name?.Trim();
            order.Customer.Email = order.Customer.Email?.Trim();
        }
        if (order.Items != null)
        {
            for (int i = 0; i < order.Items.Count; i++)
            {
                order.Items[i].Sku = order.Items[i].Sku?.Trim();
                order.Items[i].Description = order.Items[i].Description?.Trim();
            }
        }
        return order;
    }

    [Benchmark]
    public OrderDto AutoNormalizeGraph()
    {
        var order = CreateOrder();
        _normalizer.Normalize(order);
        return order;
    }
}
