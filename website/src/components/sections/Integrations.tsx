import React, { useState } from 'react';
import { Copy, Check, Terminal, ExternalLink, Code2 } from 'lucide-react';

interface IntegrationTab {
  id: string;
  name: string;
  packageName: string;
  version: string;
  description: string;
  codeSnippet: string;
}

const TABS: IntegrationTab[] = [
  {
    id: 'aspnetcore',
    name: 'ASP.NET Core',
    packageName: 'AutoNormalize.AspNetCore',
    version: '1.0.0',
    description: 'Unified setup for Minimal APIs, MVC Controllers, Model Binders, and System.Text.Json.',
    codeSnippet: `// Program.cs
using DotNet.AutoNormalize.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. One line configures Minimal APIs, MVC, and JSON options
builder.Services.AddAutoNormalize(options =>
{
    options.TrimStrings = true;
    options.EmptyStringsToNull = true;
    options.UnicodeNormalization = NormalizationForm.FormC;
});

var app = builder.Build();

// 2. Positional record arguments normalized at birth:
app.MapPost("/api/users", (RegisterUserRequest request) =>
{
    // request.Name is trimmed ("Ada Lovelace")
    // request.Bio is null (if whitespace-only)
    // request.Password is shielded and preserved!
    return Results.Ok(request);
});

app.Run();

public record RegisterUserRequest(string Name, string? Bio, string Password);`,
  },
  {
    id: 'efcore',
    name: 'Entity Framework Core',
    packageName: 'AutoNormalize.EntityFrameworkCore',
    version: '1.0.0',
    description: 'ISaveChangesInterceptor that automatically normalizes Added and Modified entities before writing to SQL.',
    codeSnippet: `// Program.cs / DbContext setup
using DotNet.AutoNormalize.EntityFrameworkCore.Extensions;

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString)
           // Automatically normalizes entity properties on SaveChanges()
           .AddAutoNormalize(options =>
           {
               options.TrimStrings = true;
               options.EmptyStringsToNull = true;
           });
});

// In your application service:
var customer = new Customer 
{ 
    Email = "   customer@enterprise.com   ", 
    Notes = "      " 
};
dbContext.Customers.Add(customer);
await dbContext.SaveChangesAsync();

// Stored in SQL database as:
// Email: "customer@enterprise.com"
// Notes: NULL (instead of whitespace or empty string)`,
  },
  {
    id: 'json',
    name: 'System.Text.Json',
    packageName: 'AutoNormalize.Json',
    version: '1.0.0',
    description: 'IJsonTypeInfoResolver modifiers for standalone System.Text.Json serialization pipelines.',
    codeSnippet: `// Standalone System.Text.Json deserializer
using System.Text.Json;
using DotNet.AutoNormalize.Json.Extensions;

var options = new JsonSerializerOptions()
    .AddAutoNormalize(opt =>
    {
        opt.TrimStrings = true;
        opt.EmptyStringsToNull = true;
    });

string payload = """
{
    "Name": "   Grace Hopper   ",
    "Email": "   grace@navy.mil   ",
    "ApiKey": "  sk_live_123456789  "
}
""";

var model = JsonSerializer.Deserialize<ClientCredentials>(payload, options);

// model.Name == "Grace Hopper"
// model.Email == "grace@navy.mil"
// model.ApiKey == "  sk_live_123456789  " (Protected by heuristic shield!)`,
  },
  {
    id: 'core',
    name: 'Standalone Core Engine',
    packageName: 'AutoNormalize.Core',
    version: '1.0.0',
    description: 'High-speed in-memory normalizer with reflection metadata caching and circular graph protection.',
    codeSnippet: `// Standalone in-memory execution
using DotNet.AutoNormalize.Core;
using DotNet.AutoNormalize.Core.Engine;

var options = new AutoNormalizeOptions
{
    TrimStrings = true,
    EmptyStringsToNull = true,
    UnicodeNormalization = NormalizationForm.FormC
};

IAutoNormalizer normalizer = new AutoNormalizer(options);

var user = new UserProfile
{
    DisplayName = "   Nikola Tesla   ",
    Bio = "      "
};

// Normalizes in-place (supports nested complex object graphs):
normalizer.Normalize(user);

// user.DisplayName == "Nikola Tesla"
// user.Bio == null`,
  },
  {
    id: 'abstractions',
    name: 'Abstractions & Attributes',
    packageName: 'AutoNormalize.Abstractions',
    version: '1.0.0',
    description: 'Lightweight zero-dependency interfaces and declarative attributes targeting .NET Standard 2.0.',
    codeSnippet: `// Domain Model / Shared Class Library
using DotNet.AutoNormalize.Abstractions;

public class EmployeeContract
{
    public string FullName { get; set; } = string.Empty; // Normalized

    [PreserveWhitespace]
    public string FormattedMarkdown { get; set; } = string.Empty; // Whitespace preserved

    [DoNotNormalize]
    public string CryptographicHash { get; set; } = string.Empty; // Excluded completely

    [NormalizeWith(typeof(UppercasePostalCodeNormalizer))]
    public string PostalCode { get; set; } = string.Empty; // Custom rule
}`,
  },
];

export const Integrations: React.FC = () => {
  const [activeTab, setActiveTab] = useState<string>('aspnetcore');
  const [copied, setCopied] = useState<boolean>(false);

  const current = TABS.find((t) => t.id === activeTab) || TABS[0];

  const handleCopyCode = () => {
    navigator.clipboard.writeText(current.codeSnippet);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  return (
    <section id="integrations" className="py-20 px-4 sm:px-6 lg:px-8 max-w-7xl mx-auto border-t border-slate-800/60">
      <div className="text-center max-w-3xl mx-auto mb-14 space-y-3">
        <h2 className="text-xs uppercase font-semibold tracking-wider text-cyan-400">
          Architecture & Ecosystem
        </h2>
        <p className="text-3xl sm:text-4xl font-bold text-white tracking-tight">
          Seamless Integration Across Every Layer
        </p>
        <p className="text-slate-400 text-sm sm:text-base leading-relaxed">
          Choose the package that fits your architecture. Each module is independently versioned, thoroughly tested, and published to NuGet.org.
        </p>
      </div>

      {/* Tab Selectors */}
      <div className="flex flex-wrap items-center justify-center gap-2 mb-8">
        {TABS.map((tab) => (
          <button
            key={tab.id}
            onClick={() => setActiveTab(tab.id)}
            className={`px-4 py-2 rounded-lg text-xs sm:text-sm font-medium transition-all ${
              activeTab === tab.id
                ? 'bg-violet-600 text-white shadow-[0_0_15px_rgba(139,92,246,0.4)]'
                : 'bg-slate-900/80 text-slate-400 hover:text-white hover:bg-slate-800 border border-slate-800'
            }`}
          >
            {tab.name}
          </button>
        ))}
      </div>

      {/* Tab Content Box */}
      <div className="rounded-2xl border border-slate-800 bg-navy-950 overflow-hidden shadow-2xl">
        {/* Top Info Bar */}
        <div className="px-6 py-4 bg-slate-900/70 border-b border-slate-800 flex flex-col sm:flex-row sm:items-center justify-between gap-4">
          <div className="space-y-1">
            <div className="flex items-center gap-2">
              <span className="font-mono text-sm font-semibold text-white">{current.packageName}</span>
              <span className="text-[10px] font-mono px-2 py-0.5 rounded-full bg-cyan-500/10 text-cyan-300 border border-cyan-500/20">
                v{current.version}
              </span>
            </div>
            <p className="text-xs text-slate-400">{current.description}</p>
          </div>

          <div className="flex items-center gap-3">
            <a
              href={`https://www.nuget.org/packages/${current.packageName}`}
              target="_blank"
              rel="noreferrer"
              className="text-xs font-mono text-cyan-400 hover:text-cyan-300 flex items-center gap-1"
            >
              NuGet Page <ExternalLink className="w-3 h-3" />
            </a>
            <button
              onClick={handleCopyCode}
              className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-slate-800 hover:bg-slate-700 text-xs font-medium text-slate-200 transition-colors"
            >
              {copied ? <Check className="w-3.5 h-3.5 text-emerald-400" /> : <Copy className="w-3.5 h-3.5" />}
              <span>{copied ? 'Copied' : 'Copy Code'}</span>
            </button>
          </div>
        </div>

        {/* Code Snippet */}
        <div className="p-6 overflow-x-auto text-xs sm:text-sm font-mono text-slate-300 leading-relaxed bg-navy-950">
          <pre className="text-slate-300">{current.codeSnippet}</pre>
        </div>
      </div>
    </section>
  );
};
