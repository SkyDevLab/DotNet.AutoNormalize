import React from 'react';
import { Zap, ShieldCheck, Cpu, Database, RefreshCw, Layers, CheckSquare, Sparkles } from 'lucide-react';

export const KeyFeatures: React.FC = () => {
  const features = [
    {
      icon: <Zap className="w-5 h-5 text-amber-400" />,
      title: "Zero-Allocation Fast Path",
      description: "Clean strings that require no alteration bypass all heap allocations, completing in 4.83 ns with 0 bytes allocated on RyuJIT .NET 9.0.",
    },
    {
      icon: <Cpu className="w-5 h-5 text-cyan-400" />,
      title: "In-Flight System.Text.Json Modifiers",
      description: "Normalizes string tokens during JSON deserialization via IJsonTypeInfoResolver.Modifiers before primary constructors execute on immutable positional records.",
    },
    {
      icon: <ShieldCheck className="w-5 h-5 text-emerald-400" />,
      title: "Defense-in-Depth Shielding",
      description: "Built-in heuristic protection safeguards sensitive fields (Password, ApiKey, Token, Secret, Salt, Signature) from accidental mutation.",
    },
    {
      icon: <Layers className="w-5 h-5 text-violet-400" />,
      title: "Unified ASP.NET Core Ingress",
      description: "Single-line builder.Services.AddAutoNormalize(...) automatically wires Minimal API endpoint filters, MVC model binders, and JSON options.",
    },
    {
      icon: <Database className="w-5 h-5 text-cyan-400" />,
      title: "EF Core SaveChanges Interceptor",
      description: "Intercepts DbContext.SaveChanges() to canonicalize Added and Modified entity states prior to SQL persistence while ignoring Unchanged entities.",
    },
    {
      icon: <RefreshCw className="w-5 h-5 text-blue-400" />,
      title: "Cycle-Safe Graph Traversal",
      description: "Tracks object references and recursion depth to safely navigate complex domain hierarchies and circular graphs without stack overflows.",
    },
    {
      icon: <CheckSquare className="w-5 h-5 text-purple-400" />,
      title: "Declarative Opt-Outs",
      description: "Fine-grained control via [PreserveWhitespace], [DoNotNormalize], and [NormalizeWith(typeof(T))] attributes for custom domain rules.",
    },
    {
      icon: <Sparkles className="w-5 h-5 text-pink-400" />,
      title: "Native AOT & Trimming Compatible",
      description: "Fully compliant with modern .NET Native AOT compilation and ILLink trimming invariants across all assemblies.",
    },
  ];

  return (
    <section id="features" className="py-20 px-4 sm:px-6 lg:px-8 max-w-7xl mx-auto border-t border-slate-800/60">
      <div className="text-center max-w-3xl mx-auto mb-16 space-y-3">
        <h2 className="text-xs uppercase font-semibold tracking-wider text-violet-400">
          Core Engine Capabilities
        </h2>
        <p className="text-3xl sm:text-4xl font-bold text-white tracking-tight">
          Engineered for Extreme Performance & Safety
        </p>
        <p className="text-slate-400 text-sm sm:text-base leading-relaxed">
          Built from the ground up to eliminate boilerplate and protect your database representation integrity without compromising runtime speed.
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {features.map((feat, idx) => (
          <div
            key={idx}
            className="rounded-xl border border-slate-800/80 bg-slate-900/40 p-5 hover:border-violet-500/40 hover:bg-slate-900/70 transition-all group"
          >
            <div className="w-10 h-10 rounded-lg bg-slate-800 border border-slate-700/60 flex items-center justify-center mb-4 group-hover:scale-105 transition-transform">
              {feat.icon}
            </div>
            <h3 className="text-base font-semibold text-white mb-2 tracking-tight">
              {feat.title}
            </h3>
            <p className="text-xs text-slate-400 leading-relaxed">
              {feat.description}
            </p>
          </div>
        ))}
      </div>
    </section>
  );
};
