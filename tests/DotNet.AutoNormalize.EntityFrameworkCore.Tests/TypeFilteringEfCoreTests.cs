using System.Threading.Tasks;
using DotNet.AutoNormalize.Core;
using DotNet.AutoNormalize.EntityFrameworkCore.Interceptors;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DotNet.AutoNormalize.EntityFrameworkCore.Tests
{
    public class TypeFilteringEfCoreTests
    {
        public class FilterTestDbContext : DbContext
        {
            public DbSet<EfFilterFixtures.Included.NormalizedEntity> NormalizedEntities => Set<EfFilterFixtures.Included.NormalizedEntity>();
            public DbSet<EfFilterFixtures.Excluded.RawEntity> RawEntities => Set<EfFilterFixtures.Excluded.RawEntity>();

            public FilterTestDbContext(DbContextOptions<FilterTestDbContext> options) : base(options)
            {
            }
        }

        [Fact]
        public async Task SaveChangesAsync_RespectsNamespaceFilters()
        {
            var normalizeOptions = new AutoNormalizeOptions();
            normalizeOptions.IncludedNamespaces.Add("DotNet.AutoNormalize.EntityFrameworkCore.Tests.EfFilterFixtures.Included");

            var dbOptions = new DbContextOptionsBuilder<FilterTestDbContext>()
                .UseInMemoryDatabase("TypeFilteringEfCoreTestDb")
                .AddInterceptors(new AutoNormalizeSaveChangesInterceptor(normalizeOptions))
                .Options;

            await using var context = new FilterTestDbContext(dbOptions);

            var included = new EfFilterFixtures.Included.NormalizedEntity { Title = "  Clean Me  " };
            var excluded = new EfFilterFixtures.Excluded.RawEntity { Payload = "  Leave Untouched  " };

            context.NormalizedEntities.Add(included);
            context.RawEntities.Add(excluded);
            await context.SaveChangesAsync();

            Assert.Equal("Clean Me", included.Title);
            Assert.Equal("  Leave Untouched  ", excluded.Payload);
        }
    }
}

namespace DotNet.AutoNormalize.EntityFrameworkCore.Tests.EfFilterFixtures.Included
{
    public class NormalizedEntity
    {
        public int Id { get; set; }
        public string? Title { get; set; }
    }
}

namespace DotNet.AutoNormalize.EntityFrameworkCore.Tests.EfFilterFixtures.Excluded
{
    public class RawEntity
    {
        public int Id { get; set; }
        public string? Payload { get; set; }
    }
}
