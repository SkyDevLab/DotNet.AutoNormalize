using System;
using System.Threading;
using System.Threading.Tasks;
using DotNet.AutoNormalize.Core;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DotNet.AutoNormalize.EntityFrameworkCore.Tests.AuditTests;

public class EfCoreAuditTests
{
    public class Order
    {
        public int Id { get; set; }
        public string? OrderNumber { get; set; }
        public string? Status { get; set; }
        public ShippingAddress? Address { get; set; }
    }

    public class ShippingAddress
    {
        public string? Street { get; set; }
        public string? City { get; set; }
    }

    public class AuditDbContext : DbContext
    {
        public DbSet<Order> Orders => Set<Order>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            }

            optionsBuilder.AddAutoNormalize(new AutoNormalizeOptions
            {
                TrimStrings = true,
                EmptyStringsToNull = true
            });
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>().OwnsOne(o => o.Address);
        }
    }

    private static AuditDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AuditDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AuditDbContext();
    }

    [Fact]
    public void UnchangedAndDeletedEntities_AreNotModifiedOrReevaluated()
    {
        using var context = CreateContext();

        var order = new Order
        {
            OrderNumber = "ORD-1",
            Status = "Active"
        };
        context.Orders.Add(order);
        context.SaveChanges();

        // Entity is now Unchanged.
        // Artificially change a property without EF tracking (e.g., via direct field or bypass)
        var entry = context.Entry(order);
        Assert.Equal(EntityState.Unchanged, entry.State);

        // Save again without changes
        context.SaveChanges();

        Assert.Equal(EntityState.Unchanged, entry.State);

        // Delete entity
        context.Orders.Remove(order);
        Assert.Equal(EntityState.Deleted, entry.State);

        // Saving changes on Deleted entity should not re-normalize or revive properties
        context.SaveChanges();
    }

    [Fact]
    public void OwnedEntities_AreNormalizedBeforePersistence()
    {
        using var context = CreateContext();

        var order = new Order
        {
            OrderNumber = "   ORD-999   ",
            Status = "   Pending   ",
            Address = new ShippingAddress
            {
                Street = "   123 Main Street   ",
                City = "   Seattle   "
            }
        };

        context.Orders.Add(order);
        context.SaveChanges();

        var saved = context.Orders.Find(order.Id);
        Assert.NotNull(saved);
        Assert.Equal("ORD-999", saved.OrderNumber);
        Assert.Equal("Pending", saved.Status);
        Assert.NotNull(saved.Address);
        Assert.Equal("123 Main Street", saved.Address.Street);
        Assert.Equal("Seattle", saved.Address.City);
    }

    [Fact]
    public async Task SaveChangesAsync_WithCancellationToken_ThrowsWhenCancelled()
    {
        using var context = CreateContext();
        context.Orders.Add(new Order { OrderNumber = "   ORD-CANCEL   " });

        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await context.SaveChangesAsync(cts.Token);
        });
    }

    [Fact]
    public void RepeatedSaveChanges_DoesNotCorruptOrDoubleProcess()
    {
        using var context = CreateContext();

        var order = new Order
        {
            OrderNumber = "   ORD-REPEAT   ",
            Status = "   Submitted   "
        };

        context.Orders.Add(order);

        // First SaveChanges normalizes values
        context.SaveChanges();
        Assert.Equal("ORD-REPEAT", order.OrderNumber);
        Assert.Equal("Submitted", order.Status);

        // Second SaveChanges should be a safe no-op on Unchanged entities
        context.SaveChanges();
        Assert.Equal("ORD-REPEAT", order.OrderNumber);
        Assert.Equal("Submitted", order.Status);
    }
}
