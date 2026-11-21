using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using DotNet.AutoNormalize.Core;
using DotNet.AutoNormalize.EntityFrameworkCore.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace DotNet.AutoNormalize.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class EfCorePersistenceBenchmarks
{
    public class CustomerEntity
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? City { get; set; }
    }

    public class BenchmarkDbContext : DbContext
    {
        public DbSet<CustomerEntity> Customers => Set<CustomerEntity>();

        private readonly bool _useAutoNormalize;

        public BenchmarkDbContext(DbContextOptions<BenchmarkDbContext> options, bool useAutoNormalize)
            : base(options)
        {
            _useAutoNormalize = useAutoNormalize;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            }

            if (_useAutoNormalize)
            {
                optionsBuilder.AddInterceptors(new AutoNormalizeSaveChangesInterceptor(new AutoNormalizeOptions
                {
                    TrimStrings = true,
                    EmptyStringsToNull = true
                }));
            }
        }
    }

    private DbContextOptions<BenchmarkDbContext> _options = null!;

    [GlobalSetup]
    public void Setup()
    {
        _options = new DbContextOptionsBuilder<BenchmarkDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Benchmark(Baseline = true)]
    public int SaveChanges_DefaultEfCore()
    {
        using var context = new BenchmarkDbContext(_options, useAutoNormalize: false);
        context.Customers.Add(new CustomerEntity
        {
            FullName = "   Grace Hopper   ",
            Email = "   grace@navy.mil   ",
            City = "   Arlington   "
        });
        return context.SaveChanges();
    }

    [Benchmark]
    public int SaveChanges_WithAutoNormalize()
    {
        using var context = new BenchmarkDbContext(_options, useAutoNormalize: true);
        context.Customers.Add(new CustomerEntity
        {
            FullName = "   Grace Hopper   ",
            Email = "   grace@navy.mil   ",
            City = "   Arlington   "
        });
        return context.SaveChanges();
    }
}
