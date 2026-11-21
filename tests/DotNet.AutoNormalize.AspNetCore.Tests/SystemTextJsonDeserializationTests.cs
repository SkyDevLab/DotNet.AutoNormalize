using System.Text.Json;
using DotNet.AutoNormalize.Abstractions;
using DotNet.AutoNormalize.Core;
using DotNet.AutoNormalize.Json.Extensions;
using Xunit;

namespace DotNet.AutoNormalize.AspNetCore.Tests;

public class SystemTextJsonDeserializationTests
{
    public class UserRegistrationDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }

        [PreserveWhitespace]
        public string? FormattedNotes { get; set; }

        [DoNotNormalize]
        public string? RawToken { get; set; }
    }

    public record PositionalUserRecord(string Name, string Email, string Password);

    [Fact]
    public void Deserialize_ClassDto_NormalizesStringsAndPreservesSensitiveFields()
    {
        var options = new JsonSerializerOptions().AddAutoNormalize(new AutoNormalizeOptions
        {
            TrimStrings = true,
            EmptyStringsToNull = true
        });

        string json = """
        {
            "Name": "   Surya Sharma   ",
            "Email": "   surya@example.com   ",
            "Password": "   SecretPassword123!   ",
            "FormattedNotes": "   Preserve This Leading Indent   ",
            "RawToken": "   DoNotTouch   "
        }
        """;

        var dto = JsonSerializer.Deserialize<UserRegistrationDto>(json, options);

        Assert.NotNull(dto);
        Assert.Equal("Surya Sharma", dto.Name);
        Assert.Equal("surya@example.com", dto.Email);
        // Sensitive property is shielded
        Assert.Equal("   SecretPassword123!   ", dto.Password);
        // Preserved whitespace property is untouched
        Assert.Equal("   Preserve This Leading Indent   ", dto.FormattedNotes);
        // DoNotNormalize property is untouched
        Assert.Equal("   DoNotTouch   ", dto.RawToken);
    }

    [Fact]
    public void Deserialize_PositionalRecord_NormalizesPropertiesAtBirth()
    {
        var options = new JsonSerializerOptions().AddAutoNormalize(new AutoNormalizeOptions
        {
            TrimStrings = true,
            EmptyStringsToNull = true
        });

        string json = """
        {
            "Name": "   Alice Cooper   ",
            "Email": "   alice@cooper.com   ",
            "Password": "   UnchangedPassphrase   "
        }
        """;

        var record = JsonSerializer.Deserialize<PositionalUserRecord>(json, options);

        Assert.NotNull(record);
        Assert.Equal("Alice Cooper", record.Name);
        Assert.Equal("alice@cooper.com", record.Email);
        Assert.Equal("   UnchangedPassphrase   ", record.Password);
    }

    [Fact]
    public void Deserialize_EmptyString_ConvertsToNullWhenConfigured()
    {
        var options = new JsonSerializerOptions().AddAutoNormalize(new AutoNormalizeOptions
        {
            EmptyStringsToNull = true
        });

        string json = """
        {
            "Name": "     ",
            "Email": ""
        }
        """;

        var dto = JsonSerializer.Deserialize<UserRegistrationDto>(json, options);

        Assert.NotNull(dto);
        Assert.Null(dto.Name);
        Assert.Null(dto.Email);
    }
}
