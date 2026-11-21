using System;
using System.Linq;
using System.Text.Json;
using DotNet.AutoNormalize.Abstractions;
using DotNet.AutoNormalize.Core;
using DotNet.AutoNormalize.Json.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DotNet.AutoNormalize.PackageValidation;

/// <summary>
/// Verifies that an independent consumer project referencing ONLY the generated .nupkg packages
/// can consume and execute all core, ASP.NET Core, and EF Core normalization capabilities.
/// </summary>
public class ConsumerValidationTests
{
    public class ConsumerUser
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? ApiKey { get; set; }
    }

    public record ConsumerRecord(string FullName, string Country);

    public class ConsumerDbContext : DbContext
    {
        public ConsumerDbContext(DbContextOptions<ConsumerDbContext> options) : base(options) { }
        public DbSet<ConsumerEntity> Entities => Set<ConsumerEntity>();
    }

    public class ConsumerEntity
    {
        public int Id { get; set; }
        public string? Title { get; set; }
    }

    [Fact]
    public void PackagedAspNetCore_DependencyInjection_ResolvesAndNormalizes()
    {
        var services = new ServiceCollection();
        services.AddAutoNormalize(options =>
        {
            options.TrimStrings = true;
            options.EmptyStringsToNull = true;
        });

        var provider = services.BuildServiceProvider();
        var normalizer = provider.GetRequiredService<IAutoNormalizer>();

        var user = new ConsumerUser
        {
            Name = "   Johannes Gutenberg   ",
            Email = "   jg@press.de   ",
            ApiKey = "   Key_Secret_777   "
        };

        var result = normalizer.Normalize(user);

        Assert.NotNull(result);
        Assert.Equal("Johannes Gutenberg", result.Name);
        Assert.Equal("jg@press.de", result.Email);
        Assert.Equal("   Key_Secret_777   ", result.ApiKey); // Heuristic preservation
    }

    [Fact]
    public void PackagedJson_DeserializesAndNormalizesRecord()
    {
        var options = new JsonSerializerOptions().AddAutoNormalize();
        string json = """
        {
            "FullName": "   Alan Turing   ",
            "Country": "   United Kingdom   "
        }
        """;

        var record = JsonSerializer.Deserialize<ConsumerRecord>(json, options);

        Assert.NotNull(record);
        Assert.Equal("Alan Turing", record.FullName);
        Assert.Equal("United Kingdom", record.Country);
    }

    [Fact]
    public void PackagedEntityFrameworkCore_NormalizesOnSaveChanges()
    {
        var options = new DbContextOptionsBuilder<ConsumerDbContext>()
            .UseInMemoryDatabase("PackageValidation_" + Guid.NewGuid())
            .AddAutoNormalize()
            .Options;

        using (var db = new ConsumerDbContext(options))
        {
            db.Entities.Add(new ConsumerEntity { Title = "   Packaged EF Core Entity   " });
            db.SaveChanges();
        }

        using (var db = new ConsumerDbContext(options))
        {
            var saved = db.Entities.First();
            Assert.Equal("Packaged EF Core Entity", saved.Title);
        }
    }
}
