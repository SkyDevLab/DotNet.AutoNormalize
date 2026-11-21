using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DotNet.AutoNormalize.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace DotNet.AutoNormalize.AspNetCore.Tests;

public class MvcUserDto
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
}

[ApiController]
[Route("mvc")]
public class TestController : ControllerBase
{
    [HttpPost("body")]
    public IActionResult PostBody([FromBody] MvcUserDto dto)
    {
        return Ok(dto);
    }

    [HttpGet("query")]
    public IActionResult GetQuery([FromQuery] string search, [FromQuery] string? optional)
    {
        return Ok(new { Search = search, Optional = optional });
    }

    [HttpGet("route/{code}")]
    public IActionResult GetRoute([FromRoute] string code)
    {
        return Ok(new { Code = code });
    }

    [HttpPost("form")]
    public IActionResult PostForm([FromForm] string title, [FromForm] string? description)
    {
        return Ok(new { Title = title, Description = description });
    }
}

public class MvcIntegrationTests
{
    private sealed class MvcTestFactory : WebApplicationFactory<MvcTestFactory>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            var appBuilder = WebApplication.CreateBuilder();
            appBuilder.WebHost.UseTestServer();

            appBuilder.Services.AddControllers().AddApplicationPart(typeof(TestController).Assembly);
            appBuilder.Services.AddAutoNormalize(options =>
            {
                options.TrimStrings = true;
                options.EmptyStringsToNull = true;
            });

            var app = appBuilder.Build();
            app.MapControllers();

            app.Start();
            return app;
        }
    }

    [Fact]
    public async Task Mvc_FromBody_NormalizesDtoAndPreservesPassword()
    {
        using var factory = new MvcTestFactory();
        using var client = factory.CreateClient();

        var payload = new
        {
            Name = "   Grace Hopper   ",
            Email = "   grace@compiler.org   ",
            Password = "   P@ssphrase   "
        };

        var response = await client.PostAsJsonAsync("/mvc/body", payload);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<MvcUserDto>();
        Assert.NotNull(result);
        Assert.Equal("Grace Hopper", result.Name);
        Assert.Equal("grace@compiler.org", result.Email);
        Assert.Equal("   P@ssphrase   ", result.Password);
    }

    [Fact]
    public async Task Mvc_FromQuery_NormalizesQueryStringParameters()
    {
        using var factory = new MvcTestFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/mvc/query?search=%20%20Compiler%20Optimization%20%20&optional=%20%20%20");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<System.Text.Json.Nodes.JsonObject>();
        Assert.NotNull(json);
        Assert.Equal("Compiler Optimization", json["search"]?.ToString());
        Assert.Null(json["optional"]);
    }

    [Fact]
    public async Task Mvc_FromRoute_NormalizesRouteParameter()
    {
        using var factory = new MvcTestFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/mvc/route/%20%20ALPHA-100%20%20");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<System.Text.Json.Nodes.JsonObject>();
        Assert.NotNull(json);
        Assert.Equal("ALPHA-100", json["code"]?.ToString());
    }

    [Fact]
    public async Task Mvc_FromForm_NormalizesFormDataInputs()
    {
        using var factory = new MvcTestFactory();
        using var client = factory.CreateClient();

        var formData = new Dictionary<string, string>
        {
            ["title"] = "   Form Title   ",
            ["description"] = "     "
        };

        var response = await client.PostAsync("/mvc/form", new FormUrlEncodedContent(formData));
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<System.Text.Json.Nodes.JsonObject>();
        Assert.NotNull(json);
        Assert.Equal("Form Title", json["title"]?.ToString());
        Assert.Null(json["description"]);
    }

    [Fact]
    public void DependencyInjection_ResolvesIAutoNormalizerInstance()
    {
        using var factory = new MvcTestFactory();
        var normalizer = factory.Services.GetService<IAutoNormalizer>();

        Assert.NotNull(normalizer);
    }
}
