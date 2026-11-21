using System;
using DotNet.AutoNormalize.Abstractions;
using DotNet.AutoNormalize.Core;
using DotNet.AutoNormalize.Core.Engine;

namespace BasicUsageSample;

public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== DotNet.AutoNormalize: Basic Usage Sample ===");

        var options = new AutoNormalizeOptions
        {
            TrimStrings = true,
            EmptyStringsToNull = true,
            UnicodeNormalization = System.Text.NormalizationForm.FormC
        };

        IAutoNormalizer normalizer = new AutoNormalizer(options);

        var model = new UserProfile
        {
            DisplayName = "   Nikola Tesla   ",
            Bio = "   Electrical engineer and inventor.   ",
            WebsiteUrl = "     ",
            PreservedField = "   Must keep spaces   ",
            AccountSecret = "   SecretKey_12345   "
        };

        Console.WriteLine("\n[Before Normalization]");
        Console.WriteLine($"DisplayName    : '{model.DisplayName}'");
        Console.WriteLine($"Bio            : '{model.Bio}'");
        Console.WriteLine($"WebsiteUrl     : '{model.WebsiteUrl}'");
        Console.WriteLine($"PreservedField : '{model.PreservedField}'");
        Console.WriteLine($"AccountSecret  : '{model.AccountSecret}'");

        normalizer.Normalize(model);

        Console.WriteLine("\n[After Normalization]");
        Console.WriteLine($"DisplayName    : '{model.DisplayName}' (Trimmed)");
        Console.WriteLine($"Bio            : '{model.Bio}' (Trimmed)");
        Console.WriteLine($"WebsiteUrl     : {(model.WebsiteUrl is null ? "<null>" : $"'{model.WebsiteUrl}'")} (Converted to null)");
        Console.WriteLine($"PreservedField : '{model.PreservedField}' (Preserved via attribute)");
        Console.WriteLine($"AccountSecret  : '{model.AccountSecret}' (Preserved via sensitive name protection)");
    }
}

public class UserProfile
{
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? WebsiteUrl { get; set; }

    [PreserveWhitespace]
    public string? PreservedField { get; set; }

    public string? AccountSecret { get; set; }
}
