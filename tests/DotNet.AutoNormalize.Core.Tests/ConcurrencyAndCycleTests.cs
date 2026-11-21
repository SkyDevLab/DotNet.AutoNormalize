using System.Collections.Generic;
using System.Threading.Tasks;
using DotNet.AutoNormalize.Core;
using DotNet.AutoNormalize.Core.Engine;
using Xunit;

namespace DotNet.AutoNormalize.Core.Tests;

public class ConcurrencyAndCycleTests
{
    public class CyclicNode
    {
        public string? Name { get; set; }
        public CyclicNode? Next { get; set; }
    }

    public class DeepTreeNode
    {
        public string? Value { get; set; }
        public DeepTreeNode? Child { get; set; }
    }

    [Fact]
    public void Normalize_CyclicObjectGraph_DoesNotStackOverflowAndNormalizesBoth()
    {
        var normalizer = new AutoNormalizer();

        var nodeA = new CyclicNode { Name = "   Node A   " };
        var nodeB = new CyclicNode { Name = "   Node B   " };

        // Create cycle: A -> B -> A
        nodeA.Next = nodeB;
        nodeB.Next = nodeA;

        normalizer.Normalize(nodeA);

        Assert.Equal("Node A", nodeA.Name);
        Assert.Equal("Node B", nodeB.Name);
    }

    [Fact]
    public void Normalize_SelfReferencingNode_DoesNotStackOverflow()
    {
        var normalizer = new AutoNormalizer();

        var node = new CyclicNode { Name = "   Self Referencing   " };
        node.Next = node;

        normalizer.Normalize(node);

        Assert.Equal("Self Referencing", node.Name);
    }

    [Fact]
    public void Normalize_DeepGraph_RespectsMaxRecursionDepth()
    {
        var normalizer = new AutoNormalizer(new AutoNormalizeOptions
        {
            MaxRecursionDepth = 5
        });

        // Build a 10-level deep chain
        var root = new DeepTreeNode { Value = "   Level 0   " };
        var current = root;
        for (int i = 1; i <= 10; i++)
        {
            var next = new DeepTreeNode { Value = $"   Level {i}   " };
            current.Child = next;
            current = next;
        }

        normalizer.Normalize(root);

        // Levels 0 to 5 should be normalized
        var inspect = root;
        int level = 0;
        while (inspect != null)
        {
            if (level <= 5)
            {
                Assert.Equal($"Level {level}", inspect.Value);
            }
            else
            {
                // Deeper than MaxRecursionDepth=5 should remain unnormalized
                Assert.Equal($"   Level {level}   ", inspect.Value);
            }

            inspect = inspect.Child;
            level++;
        }
    }

    [Fact]
    public void Normalize_ConcurrentThreads_ExecutesSafelyWithoutRaceConditions()
    {
        var normalizer = new AutoNormalizer();

        Parallel.For(0, 1000, i =>
        {
            var node = new CyclicNode
            {
                Name = $"   Thread Item {i}   ",
                Next = new CyclicNode
                {
                    Name = $"   Child Item {i}   "
                }
            };

            normalizer.Normalize(node);

            Assert.Equal($"Thread Item {i}", node.Name);
            Assert.Equal($"Child Item {i}", node.Next.Name);
        });
    }

    [Fact]
    public void Normalize_LargeCollection_NormalizesAllElements()
    {
        var normalizer = new AutoNormalizer();
        var list = new List<string>(1000);
        for (int i = 0; i < 1000; i++)
        {
            list.Add($"   Item {i}   ");
        }

        normalizer.Normalize(list);

        for (int i = 0; i < 1000; i++)
        {
            Assert.Equal($"Item {i}", list[i]);
        }
    }
}
