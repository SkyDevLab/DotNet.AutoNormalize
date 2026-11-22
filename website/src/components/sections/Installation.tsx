import React, { useState } from 'react';
import { Terminal, Copy, Check, Package, CheckCircle2 } from 'lucide-react';

interface PackageItem {
  id: string;
  name: string;
  role: string;
  command: string;
  recommendedFor: string;
}

const PACKAGES: PackageItem[] = [
  {
    id: 'aspnetcore',
    name: 'AutoNormalize.AspNetCore',
    role: 'Primary Ingress Engine',
    command: 'dotnet add package AutoNormalize.AspNetCore',
    recommendedFor: 'ASP.NET Core Web APIs, Minimal APIs, MVC controllers, and System.Text.Json',
  },
  {
    id: 'efcore',
    name: 'AutoNormalize.EntityFrameworkCore',
    role: 'Persistence Layer',
    command: 'dotnet add package AutoNormalize.EntityFrameworkCore',
    recommendedFor: 'Entity Framework Core DbContext change tracking and SaveChanges interception',
  },
  {
    id: 'json',
    name: 'AutoNormalize.Json',
    role: 'Serialization Engine',
    command: 'dotnet add package AutoNormalize.Json',
    recommendedFor: 'System.Text.Json contract resolvers, Native AOT serialization, and positional records',
  },
  {
    id: 'core',
    name: 'AutoNormalize.Core',
    role: 'Standalone Engine',
    command: 'dotnet add package AutoNormalize.Core',
    recommendedFor: 'Console apps, background workers, MediatR pipelines, and direct in-memory normalization',
  },
  {
    id: 'abstractions',
    name: 'AutoNormalize.Abstractions',
    role: 'Attributes & Contracts',
    command: 'dotnet add package AutoNormalize.Abstractions',
    recommendedFor: 'Shared domain models, class libraries, .NET Standard 2.0 assemblies',
  },
];

export const Installation: React.FC = () => {
  const [copiedPackage, setCopiedPackage] = useState<string | null>(null);

  const handleCopy = (pkgId: string, cmd: string) => {
    navigator.clipboard.writeText(cmd);
    setCopiedPackage(pkgId);
    setTimeout(() => setCopiedPackage(null), 2000);
  };

  return (
    <section id="installation" className="py-20 px-4 sm:px-6 lg:px-8 max-w-7xl mx-auto border-t border-slate-800/60">
      <div className="text-center max-w-3xl mx-auto mb-14 space-y-3">
        <h2 className="text-xs uppercase font-semibold tracking-wider text-cyan-400">
          Getting Started
        </h2>
        <p className="text-3xl sm:text-4xl font-bold text-white tracking-tight">
          Install in Seconds via NuGet
        </p>
        <p className="text-slate-400 text-sm sm:text-base leading-relaxed">
          Install the package tailored to your application boundary. All packages multi-target modern .NET.
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-12">
        {PACKAGES.map((pkg) => (
          <div
            key={pkg.id}
            className="rounded-xl border border-slate-800 bg-slate-900/50 p-5 flex flex-col justify-between hover:border-slate-700 transition-colors"
          >
            <div>
              <div className="flex items-center justify-between mb-2">
                <span className="font-semibold text-white text-sm font-mono">{pkg.name}</span>
                <span className="text-[10px] uppercase font-mono px-2 py-0.5 rounded bg-violet-500/10 text-violet-300 border border-violet-500/20">
                  {pkg.role}
                </span>
              </div>
              <p className="text-xs text-slate-400 leading-relaxed mb-4">
                {pkg.recommendedFor}
              </p>
            </div>

            <div 
              onClick={() => handleCopy(pkg.id, pkg.command)}
              className="flex items-center justify-between p-2.5 rounded-lg bg-navy-950 border border-slate-800 hover:border-slate-700 cursor-pointer text-xs font-mono text-slate-300 group transition-all"
            >
              <span className="truncate mr-2">{pkg.command}</span>
              <button className="flex-shrink-0 text-slate-400 group-hover:text-white">
                {copiedPackage === pkg.id ? (
                  <Check className="w-4 h-4 text-emerald-400" />
                ) : (
                  <Copy className="w-4 h-4" />
                )}
              </button>
            </div>
          </div>
        ))}
      </div>

      {/* Target Framework Matrix */}
      <div className="rounded-xl border border-slate-800 bg-navy-950 p-6 flex flex-col sm:flex-row items-center justify-between gap-4 text-xs text-slate-300">
        <div className="space-y-1 text-center sm:text-left">
          <h4 className="font-semibold text-white text-sm">Supported Target Frameworks</h4>
          <p className="text-slate-400">Tested and verified with automated test suites on GitHub Actions runners.</p>
        </div>
        <div className="flex flex-wrap items-center gap-3">
          <span className="px-3 py-1 rounded-full bg-slate-900 border border-slate-700 text-white font-mono flex items-center gap-1.5">
            <CheckCircle2 className="w-3.5 h-3.5 text-emerald-400" /> .NET 9.0 (STS)
          </span>
          <span className="px-3 py-1 rounded-full bg-slate-900 border border-slate-700 text-white font-mono flex items-center gap-1.5">
            <CheckCircle2 className="w-3.5 h-3.5 text-emerald-400" /> .NET 8.0 (LTS)
          </span>
          <span className="px-3 py-1 rounded-full bg-slate-900 border border-slate-700 text-white font-mono flex items-center gap-1.5">
            <CheckCircle2 className="w-3.5 h-3.5 text-emerald-400" /> .NET Standard 2.0
          </span>
        </div>
      </div>
    </section>
  );
};
