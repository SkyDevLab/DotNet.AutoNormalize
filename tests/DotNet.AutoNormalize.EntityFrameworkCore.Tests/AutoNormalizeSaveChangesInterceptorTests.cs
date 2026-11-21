using System;
using System.Threading.Tasks;
using DotNet.AutoNormalize.Abstractions;
using DotNet.AutoNormalize.Core;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DotNet.AutoNormalize.EntityFrameworkCore.Tests;

public class AutoNormalizeSaveChangesInterceptorTests
{
    public class CustomerEntity
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }

        [PreserveWhitespace]
        public string? FormattedAddress { get; set; }
    }

    [DoNotNormalize]
    public class AuditLogEntity
    {
        public int Id { get; set; }
        public string? Message { get; set; }
    }

    public class TestDbContext : DbContext
    {
        public DbSet<CustomerEntity> Customers => Set<CustomerEntity>();
        public DbSet<AuditLogEntity> AuditLogs => Set<AuditLogEntity>();

        private readonly AutoNormalizeOptions? _normalizeOptions;

        public TestDbContext(DbContextOptions<TestDbContext> options, AutoNormalizeOptions? normalizeOptions = null)
            : base(options)
        {
            _normalizeOptions = normalizeOptions;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            }

            optionsBuilder.AddAutoNormalize(_normalizeOptions ?? new AutoNormalizeOptions
            {
                TrimStrings = true,
                EmptyStringsToNull = true
            });
        }
    }

    private static TestDbContext CreateContext(AutoNormalizeOptions? options = null)
    {
        var dbOptions = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestDbContext(dbOptions, options);
    }

    [Fact]
    public void SaveChanges_NewEntity_NormalizesStringsAndProtectsSensitiveFields()
    {
        using var context = CreateContext();

        var customer = new CustomerEntity
        {
            FullName = "   Ada Lovelace   ",
            Email = "   ada@analytical.engine   ",
            PasswordHash = "   $2a$12$e8uq4f.secret.hash   ",
            FormattedAddress = "   Line 1\n   Line 2   "
        };

        context.Customers.Add(customer);
        context.SaveChanges();

        var saved = context.Customers.Find(customer.Id);
        Assert.NotNull(saved);
        Assert.Equal("Ada Lovelace", saved.FullName);
        Assert.Equal("ada@analytical.engine", saved.Email);
        // Sensitive property protected
        Assert.Equal("   $2a$12$e8uq4f.secret.hash   ", saved.PasswordHash);
        // Preserved whitespace property protected
        Assert.Equal("   Line 1\n   Line 2   ", saved.FormattedAddress);
    }

    [Fact]
    public async Task SaveChangesAsync_NewEntity_NormalizesStringsCorrectly()
    {
        using var context = CreateContext();

        var customer = new CustomerEntity
        {
            FullName = "   Charles Babbage   ",
            Email = "   charles@difference.engine   ",
            PasswordHash = "   secret   "
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var saved = await context.Customers.FindAsync(customer.Id);
        Assert.NotNull(saved);
        Assert.Equal("Charles Babbage", saved.FullName);
        Assert.Equal("charles@difference.engine", saved.Email);
        Assert.Equal("   secret   ", saved.PasswordHash);
    }

    [Fact]
    public void SaveChanges_EmptyString_ConvertsToNullWhenConfigured()
    {
        using var context = CreateContext(new AutoNormalizeOptions
        {
            TrimStrings = true,
            EmptyStringsToNull = true
        });

        var customer = new CustomerEntity
        {
            FullName = "   Alan Turing   ",
            Email = "     " // Whitespace only
        };

        context.Customers.Add(customer);
        context.SaveChanges();

        var saved = context.Customers.Find(customer.Id);
        Assert.NotNull(saved);
        Assert.Equal("Alan Turing", saved.FullName);
        Assert.Null(saved.Email);
    }

    [Fact]
    public void SaveChanges_ModifiedEntity_NormalizesUpdatedProperties()
    {
        using var context = CreateContext();

        var customer = new CustomerEntity
        {
            FullName = "Grace Hopper",
            Email = "grace@navy.mil"
        };

        context.Customers.Add(customer);
        context.SaveChanges();

        // Modify property with untrimmed string
        customer.FullName = "   Rear Admiral Grace Hopper   ";
        context.SaveChanges();

        var updated = context.Customers.Find(customer.Id);
        Assert.NotNull(updated);
        Assert.Equal("Rear Admiral Grace Hopper", updated.FullName);
    }

    [Fact]
    public void SaveChanges_DoNotNormalizeClass_LeavesAllPropertiesUntouched()
    {
        using var context = CreateContext();

        var log = new AuditLogEntity
        {
            Message = "   CRITICAL AUDIT EVENT: LEADING SPACES INTENTIONAL   "
        };

        context.AuditLogs.Add(log);
        context.SaveChanges();

        var saved = context.AuditLogs.Find(log.Id);
        Assert.NotNull(saved);
        Assert.Equal("   CRITICAL AUDIT EVENT: LEADING SPACES INTENTIONAL   ", saved.Message);
    }
}
