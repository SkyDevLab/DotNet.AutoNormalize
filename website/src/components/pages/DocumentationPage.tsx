import React, { useState } from 'react';
import { BookOpen, Code2, Shield, Zap, Database, Terminal, Check, Copy } from 'lucide-react';

interface DocSection {
  id: string;
  title: string;
  category: string;
  content: React.ReactNode;
}

export const DocumentationPage: React.FC = () => {
  const [activeSection, setActiveSection] = useState<string>('quickstart');
  const [copiedCmd, setCopiedCmd] = useState<string | null>(null);

  const handleCopy = (cmd: string) => {
    navigator.clipboard.writeText(cmd);
    setCopiedCmd(cmd);
    setTimeout(() => setCopiedCmd(null), 2000);
  };

  const sections: DocSection[] = [
    {
      id: 'quickstart',
      title: 'Quickstart',
      category: 'Getting Started',
      content: (
        <div className="space-y-6 text-sm text-slate-300">
          <div>
            <h2 className="text-2xl font-bold text-white mb-2">Getting Started with AutoNormalize</h2>
            <p className="text-slate-400">
              AutoNormalize provides representation canonicalization at application ingress and persistence boundaries. 
              In just two steps, you can eliminate manual trimming across your entire ASP.NET Core stack.
            </p>
          </div>

          <div className="space-y-3">
            <h3 className="text-base font-semibold text-white">Step 1: Install the NuGet Package</h3>
            <div 
              onClick={() => handleCopy('dotnet add package AutoNormalize.AspNetCore')}
              className="flex items-center justify-between p-3 rounded-lg bg-navy-950 border border-slate-800 font-mono text-xs cursor-pointer hover:border-slate-700 transition-colors"
            >
              <span>dotnet add package AutoNormalize.AspNetCore</span>
              {copiedCmd === 'dotnet add package AutoNormalize.AspNetCore' ? <Check className="w-4 h-4 text-emerald-400" /> : <Copy className="w-4 h-4 text-slate-400" />}
            </div>
          </div>

          <div className="space-y-3">
            <h3 className="text-base font-semibold text-white">Step 2: Register in Program.cs</h3>
            <pre className="p-4 rounded-xl bg-navy-950 border border-slate-800 font-mono text-xs text-slate-300 overflow-x-auto">
{`var builder = WebApplication.CreateBuilder(args);

// Register AutoNormalize across Minimal APIs, MVC, and System.Text.Json
builder.Services.AddAutoNormalize(options =>
{
    options.TrimStrings = true;
    options.EmptyStringsToNull = true;
    options.UnicodeNormalization = NormalizationForm.FormC;
});

var app = builder.Build();

app.MapPost("/api/users", (RegisterUserCommand command) =>
{
    // command.Name is trimmed ("Ada Lovelace")
    // command.Bio is null (if whitespace-only)
    // command.Password is shielded and preserved!
    return Results.Ok(command);
});

app.Run();

public record RegisterUserCommand(string Name, string? Bio, string Password);`}
            </pre>
          </div>
        </div>
      ),
    },
    {
      id: 'stj-modifier',
      title: 'System.Text.Json Modifier',
      category: 'Serialization',
      content: (
        <div className="space-y-6 text-sm text-slate-300">
          <div>
            <h2 className="text-2xl font-bold text-white mb-2">In-Flight JSON Type Resolver Modifiers</h2>
            <p className="text-slate-400">
              Unlike traditional libraries that inspect and mutate object graphs via reflection post-construction, 
              AutoNormalize attaches directly to <code className="text-cyan-400 font-mono">IJsonTypeInfoResolver.Modifiers</code>.
            </p>
          </div>

          <div className="p-4 rounded-xl bg-slate-900/60 border border-slate-800 space-y-2">
            <h4 className="font-semibold text-white text-sm">Why this matters for modern C#:</h4>
            <ul className="list-disc list-inside text-xs text-slate-300 space-y-1">
              <li><strong>Positional Records:</strong> Works seamlessly with immutable primary constructors (<code className="font-mono text-violet-300">public record User(string Name)</code>).</li>
              <li><strong>Native AOT Compatible:</strong> Avoids unsafe reflection emitters and dynamically generated IL.</li>
              <li><strong>Zero Allocations on Clean Strings:</strong> Reads directly from <code className="font-mono text-cyan-300">Utf8JsonReader</code> without creating intermediate strings.</li>
            </ul>
          </div>

          <div className="space-y-3">
            <h3 className="text-base font-semibold text-white">Standalone JsonSerializer Configuration</h3>
            <pre className="p-4 rounded-xl bg-navy-950 border border-slate-800 font-mono text-xs text-slate-300 overflow-x-auto">
{`using System.Text.Json;
using DotNet.AutoNormalize.Json.Extensions;

var options = new JsonSerializerOptions()
    .AddAutoNormalize(opt =>
    {
        opt.TrimStrings = true;
        opt.EmptyStringsToNull = true;
    });

var result = JsonSerializer.Deserialize<CustomerRecord>(jsonString, options);`}
            </pre>
          </div>
        </div>
      ),
    },
    {
      id: 'efcore',
      title: 'EF Core Interceptor',
      category: 'Persistence',
      content: (
        <div className="space-y-6 text-sm text-slate-300">
          <div>
            <h2 className="text-2xl font-bold text-white mb-2">EF Core SaveChangesInterceptor</h2>
            <p className="text-slate-400">
              Canonicalize database entities automatically during <code className="text-cyan-400 font-mono">DbContext.SaveChanges()</code>.
            </p>
          </div>

          <div className="space-y-3">
            <h3 className="text-base font-semibold text-white">DbContext Configuration</h3>
            <pre className="p-4 rounded-xl bg-navy-950 border border-slate-800 font-mono text-xs text-slate-300 overflow-x-auto">
{`using Microsoft.EntityFrameworkCore;
using DotNet.AutoNormalize.EntityFrameworkCore.Extensions;

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString)
           .AddAutoNormalize(opt =>
           {
               opt.TrimStrings = true;
               opt.EmptyStringsToNull = true;
           });
});`}
            </pre>
          </div>

          <div className="p-4 rounded-xl bg-slate-900/60 border border-slate-800 text-xs text-slate-300 space-y-1">
            <h4 className="font-semibold text-white">How it works under the hood:</h4>
            <p className="text-slate-400">
              The interceptor inspects the EF Core ChangeTracker and processes only entities in the <code className="font-mono text-emerald-400">Added</code> and <code className="font-mono text-cyan-400">Modified</code> states. <code className="font-mono text-slate-400">Unchanged</code> entities are completely skipped with zero query or property traversal overhead.
            </p>
          </div>
        </div>
      ),
    },
    {
      id: 'attributes',
      title: 'Attributes & Opt-Outs',
      category: 'Configuration',
      content: (
        <div className="space-y-6 text-sm text-slate-300">
          <div>
            <h2 className="text-2xl font-bold text-white mb-2">Declarative Attributes</h2>
            <p className="text-slate-400">
              Fine-tune normalization behavior on individual properties using attributes from <code className="text-cyan-400 font-mono">AutoNormalize.Abstractions</code>.
            </p>
          </div>

          <pre className="p-4 rounded-xl bg-navy-950 border border-slate-800 font-mono text-xs text-slate-300 overflow-x-auto">
{`using DotNet.AutoNormalize.Abstractions;

public class UserSubmission
{
    // Normalizes automatically (trimmed, empty to null, Form C)
    public string DisplayName { get; set; } = string.Empty;

    // Preserves whitespace (ideal for source code, markdown, formatted text)
    [PreserveWhitespace]
    public string FormattedCode { get; set; } = string.Empty;

    // Completely excluded from normalization inspection
    [DoNotNormalize]
    public string Base64Signature { get; set; } = string.Empty;

    // Custom domain normalizer
    [NormalizeWith(typeof(UppercasePostalCodeNormalizer))]
    public string PostalCode { get; set; } = string.Empty;

    // Automatically protected by heuristic sensitive shield:
    public string Password { get; set; } = string.Empty;
}`}
          </pre>
        </div>
      ),
    },
  ];

  const current = sections.find((s) => s.id === activeSection) || sections[0];

  return (
    <div className="py-12 px-4 sm:px-6 lg:px-8 max-w-7xl mx-auto">
      <div className="grid grid-cols-1 md:grid-cols-4 gap-8">
        {/* Sidebar Nav */}
        <aside className="md:col-span-1 space-y-4">
          <div className="sticky top-24 space-y-6">
            <div>
              <h3 className="text-xs uppercase font-semibold text-slate-400 tracking-wider mb-2 flex items-center gap-1.5">
                <BookOpen className="w-3.5 h-3.5 text-cyan-400" /> Documentation
              </h3>
              <div className="space-y-1">
                {sections.map((sec) => (
                  <button
                    key={sec.id}
                    onClick={() => setActiveSection(sec.id)}
                    className={`w-full text-left px-3 py-2 rounded-lg text-xs font-medium transition-colors ${
                      activeSection === sec.id
                        ? 'bg-violet-600 text-white'
                        : 'text-slate-400 hover:text-white hover:bg-slate-800/60'
                    }`}
                  >
                    {sec.title}
                  </button>
                ))}
              </div>
            </div>

            <div className="pt-4 border-t border-slate-800 text-xs text-slate-400 space-y-2">
              <span className="font-semibold text-slate-300 block">External Resources</span>
              <a 
                href="https://github.com/SkyDevLab/DotNet.AutoNormalize/tree/main/docs" 
                target="_blank" 
                rel="noreferrer"
                className="hover:text-cyan-400 block transition-colors"
              >
                GitHub Full Docs Folder →
              </a>
              <a 
                href="https://github.com/SkyDevLab/DotNet.AutoNormalize/tree/main/samples" 
                target="_blank" 
                rel="noreferrer"
                className="hover:text-cyan-400 block transition-colors"
              >
                4 Runnable Sample Apps →
              </a>
            </div>
          </div>
        </aside>

        {/* Content Viewer */}
        <main className="md:col-span-3 rounded-2xl border border-slate-800 bg-slate-900/40 p-6 sm:p-8 min-h-[600px]">
          {current.content}
        </main>
      </div>
    </div>
  );
};
