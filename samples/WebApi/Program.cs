using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register controllers and AutoNormalize
builder.Services.AddControllers();
builder.Services.AddAutoNormalize(options =>
{
    options.TrimStrings = true;
    options.EmptyStringsToNull = true;
    options.UnicodeNormalization = System.Text.NormalizationForm.FormC;
});

var app = builder.Build();

app.MapControllers();

if (args.Contains("--dry-run"))
{
    Console.WriteLine("WebApi sample initialized and configured successfully.");
    return;
}

app.Run();
