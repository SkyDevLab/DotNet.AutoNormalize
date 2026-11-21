using System.Text.Json;
using DotNet.AutoNormalize.Abstractions;
using DotNet.AutoNormalize.AspNetCore.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register AutoNormalize for Minimal APIs JSON serialization and endpoint binding
builder.Services.AddAutoNormalize(options =>
{
    options.TrimStrings = true;
    options.EmptyStringsToNull = true;
    options.UnicodeNormalization = System.Text.NormalizationForm.FormC;
});

var app = builder.Build();

app.MapGet("/", () => "DotNet.AutoNormalize Minimal API Sample is running.");

// Standard DTO endpoint
app.MapPost("/api/users", (CreateUserRequest request) =>
{
    return Results.Ok(new
    {
        Message = "User received",
        request.Username,
        request.Email,
        request.Bio,
        request.Password
    });
});

// C# Positional Record endpoint (normalized at constructor binding time)
app.MapPost("/api/customers", (CustomerRecord customer) =>
{
    return Results.Ok(new
    {
        Message = "Customer registered",
        customer.FullName,
        customer.AccountCode
    });
});

// Endpoint filter parameter normalization
app.MapGet("/api/search/{category}", (string category, string? query) =>
{
    return Results.Ok(new { Category = category, Query = query });
}).AddEndpointFilter<AutoNormalizeEndpointFilter>();

if (args.Contains("--dry-run"))
{
    Console.WriteLine("MinimalApi sample initialized and configured successfully.");
    return;
}

app.Run();

public class CreateUserRequest
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Bio { get; set; }
    public string? Password { get; set; }
}

public record CustomerRecord(string FullName, string AccountCode);
