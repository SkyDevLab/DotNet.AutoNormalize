using System.Net.Http.Json;
using System.Threading.Tasks;
using DotNet.AutoNormalize.Abstractions;
using DotNet.AutoNormalize.AspNetCore.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace DotNet.AutoNormalize.AspNetCore.Tests;

public class MinimalApiIntegrationTests
{
    public class RegisterUserRequest
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }

        [PreserveWhitespace]
        public string? FormattedBio { get; set; }
    }

    public record UserRecord(string FullName, string Email, string Password);

    private sealed class MinimalApiFactory : WebApplicationFactory<MinimalApiFactory>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            var appBuilder = WebApplication.CreateBuilder();
            appBuilder.WebHost.UseTestServer();

            appBuilder.Services.AddAutoNormalize(options =>
            {
                options.TrimStrings = true;
                options.EmptyStringsToNull = true;
            });

            var app = appBuilder.Build();

            app.MapPost("/api/users", (RegisterUserRequest request) => Results.Ok(request));
            app.MapPost("/api/records", (UserRecord record) => Results.Ok(record));
            app.MapGet("/api/search", (string query) => Results.Ok(new { Query = query }))
               .AddEndpointFilter<AutoNormalizeEndpointFilter>();

            app.Start();
            return app;
        }
    }

    [Fact]
    public async Task Post_ClassDto_NormalizesBodyAndPreservesSensitivePassword()
    {
        using var factory = new MinimalApiFactory();
        using var client = factory.CreateClient();

        var payload = new
        {
            Username = "   surya_dev   ",
            Email = "   surya@developer.org   ",
            Password = "   SensitivePass123!   ",
            FormattedBio = "   Multi-line indent   "
        };

        var response = await client.PostAsJsonAsync("/api/users", payload);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<RegisterUserRequest>();
        Assert.NotNull(result);
        Assert.Equal("surya_dev", result.Username);
        Assert.Equal("surya@developer.org", result.Email);
        Assert.Equal("   SensitivePass123!   ", result.Password);
        Assert.Equal("   Multi-line indent   ", result.FormattedBio);
    }

    [Fact]
    public async Task Post_PositionalRecord_NormalizesPropertiesAtBirth()
    {
        using var factory = new MinimalApiFactory();
        using var client = factory.CreateClient();

        var payload = new
        {
            FullName = "   Ada Lovelace   ",
            Email = "   ada@computing.org   ",
            Password = "   P@ssw0rd!   "
        };

        var response = await client.PostAsJsonAsync("/api/records", payload);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<UserRecord>();
        Assert.NotNull(result);
        Assert.Equal("Ada Lovelace", result.FullName);
        Assert.Equal("ada@computing.org", result.Email);
        Assert.Equal("   P@ssw0rd!   ", result.Password);
    }

    [Fact]
    public async Task Get_ScalarParameterWithEndpointFilter_NormalizesQueryParameter()
    {
        using var factory = new MinimalApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/search?query=%20%20%20DotNet%20Library%20%20%20");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<System.Text.Json.Nodes.JsonObject>();
        Assert.NotNull(json);
        Assert.Equal("DotNet Library", json["query"]?.ToString());
    }
}
