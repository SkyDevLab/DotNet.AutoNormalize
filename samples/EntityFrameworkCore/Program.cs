using System;
using System.Linq;
using DotNet.AutoNormalize.Abstractions;
using DotNet.AutoNormalize.Core;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCoreSample;

public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== DotNet.AutoNormalize: Entity Framework Core Sample ===");

        var options = new DbContextOptionsBuilder<SampleDbContext>()
            .UseInMemoryDatabase("SampleDb_" + Guid.NewGuid())
            .AddAutoNormalize(normalizeOptions =>
            {
                normalizeOptions.TrimStrings = true;
                normalizeOptions.EmptyStringsToNull = true;
                normalizeOptions.UnicodeNormalization = System.Text.NormalizationForm.FormC;
            })
            .Options;

        using (var db = new SampleDbContext(options))
        {
            var customer = new Customer
            {
                CompanyName = "   Contoso Ltd.   ",
                ContactEmail = "   contact@contoso.com   ",
                TaxIdentifier = "   TAX-98765   ",
                FormattedAddress = "   Line 1\n   Line 2   ",
                ApiKey = "   LiveKey_ABC12345   "
            };

            Console.WriteLine("\n[Adding Entity to DbContext]");
            Console.WriteLine($"CompanyName      : '{customer.CompanyName}'");
            Console.WriteLine($"ContactEmail     : '{customer.ContactEmail}'");
            Console.WriteLine($"TaxIdentifier    : '{customer.TaxIdentifier}'");
            Console.WriteLine($"FormattedAddress : '{customer.FormattedAddress}'");
            Console.WriteLine($"ApiKey           : '{customer.ApiKey}'");

            db.Customers.Add(customer);
            db.SaveChanges();
            Console.WriteLine("\nSaveChanges() completed.");
        }

        using (var queryDb = new SampleDbContext(options))
        {
            var saved = queryDb.Customers.First();

            Console.WriteLine("\n[Retrieved Entity from Database]");
            Console.WriteLine($"CompanyName      : '{saved.CompanyName}' (Trimmed)");
            Console.WriteLine($"ContactEmail     : '{saved.ContactEmail}' (Trimmed)");
            Console.WriteLine($"TaxIdentifier    : '{saved.TaxIdentifier}' (Trimmed)");
            Console.WriteLine($"FormattedAddress : '{saved.FormattedAddress}' (Preserved via attribute)");
            Console.WriteLine($"ApiKey           : '{saved.ApiKey}' (Preserved via sensitive heuristic)");
        }
    }
}

public class SampleDbContext : DbContext
{
    public SampleDbContext(DbContextOptions<SampleDbContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
}

public class Customer
{
    public int Id { get; set; }
    public string? CompanyName { get; set; }
    public string? ContactEmail { get; set; }
    public string? TaxIdentifier { get; set; }

    [PreserveWhitespace]
    public string? FormattedAddress { get; set; }

    public string? ApiKey { get; set; }
}
