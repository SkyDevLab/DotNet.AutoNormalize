import React, { useState } from 'react';
import { Hero3DScene } from './Hero3DScene';
import { HeroDemoSandbox } from './HeroDemoSandbox';
import { Terminal, Copy, Check, ArrowRight, Shield, Zap, Sparkles } from 'lucide-react';

export const HeroSection: React.FC = () => {
  const [copied, setCopied] = useState(false);
  const cliCommand = 'dotnet add package AutoNormalize.AspNetCore';

  const handleCopy = () => {
    navigator.clipboard.writeText(cliCommand);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  return (
    <section className="relative min-h-[92vh] flex flex-col items-center justify-center pt-8 pb-16 px-4 sm:px-6 lg:px-8 overflow-hidden bg-navy-950">
      {/* 3D Cinematic Scene - STRICTLY IN HERO ONLY */}
      <Hero3DScene />

      {/* Hero Ambient Vignette and Gradients */}
      <div className="absolute inset-0 bg-gradient-to-b from-navy-950/40 via-transparent to-navy-950 pointer-events-none" />
      <div className="absolute top-1/4 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[700px] h-[400px] bg-violet-600/10 blur-[130px] rounded-full pointer-events-none" />
      <div className="absolute top-1/3 left-1/3 w-[500px] h-[350px] bg-cyan-500/10 blur-[120px] rounded-full pointer-events-none" />

      {/* Hero Content Container */}
      <div className="relative z-10 max-w-5xl mx-auto w-full text-center space-y-6 pt-4">
        {/* .NET Foundation / Compatibility Badge */}
        <div className="inline-flex items-center gap-2 px-3.5 py-1.5 rounded-full bg-slate-900/90 border border-slate-700/70 text-xs font-medium text-slate-300 shadow-lg backdrop-blur-sm">
          <span className="flex h-2 w-2 rounded-full bg-emerald-400 animate-pulse" />
          <span className="text-white font-semibold">v1.0.0 Stable GA</span>
          <span className="text-slate-500">•</span>
          <span className="text-violet-300">.NET 8.0 & 9.0 Native</span>
          <span className="text-slate-500">•</span>
          <span className="text-cyan-300">Native AOT Ready</span>
        </div>

        {/* Headline */}
        <h1 className="text-4xl sm:text-6xl lg:text-7xl font-extrabold tracking-tight text-white leading-[1.08]">
          Normalize once. <br />
          <span className="bg-clip-text text-transparent bg-gradient-to-r from-violet-400 via-purple-300 to-cyan-400">
            Everywhere.
          </span>
        </h1>

        {/* Subtitle */}
        <p className="max-w-2xl mx-auto text-base sm:text-lg text-slate-300 leading-relaxed font-normal">
          Consistent, safe-by-default input canonicalization for modern .NET applications. 
          Normalizes JSON request bodies, Minimal API endpoints, and EF Core entities in <strong className="text-white font-semibold">4.8 nanoseconds</strong> with zero heap allocations.
        </p>

        {/* CTAs and CLI copy */}
        <div className="flex flex-col sm:flex-row items-center justify-center gap-4 pt-2">
          {/* Quick Copy Command */}
          <div 
            onClick={handleCopy}
            className="flex items-center gap-3 px-4 py-2.5 rounded-xl bg-slate-900/90 border border-slate-700/80 hover:border-violet-500/50 cursor-pointer transition-all shadow-md group"
          >
            <Terminal className="w-4 h-4 text-cyan-400" />
            <span className="text-xs sm:text-sm font-mono text-slate-200">
              {cliCommand}
            </span>
            <button 
              className="p-1 rounded hover:bg-slate-800 text-slate-400 group-hover:text-white transition-colors"
              aria-label="Copy package command"
            >
              {copied ? <Check className="w-4 h-4 text-emerald-400" /> : <Copy className="w-4 h-4" />}
            </button>
          </div>

          {/* GitHub CTA */}
          <a
            href="https://github.com/SkyDevLab/DotNet.AutoNormalize"
            target="_blank"
            rel="noreferrer"
            className="flex items-center gap-2 px-5 py-2.5 rounded-xl bg-gradient-to-r from-violet-600 to-violet-700 hover:from-violet-500 hover:to-violet-600 text-white font-semibold text-sm transition-all shadow-[0_0_20px_rgba(139,92,246,0.35)] hover:shadow-[0_0_25px_rgba(139,92,246,0.55)]"
          >
            <span>Explore on GitHub</span>
            <ArrowRight className="w-4 h-4" />
          </a>
        </div>

        {/* Live Interactive Normalization Demo Box */}
        <div className="pt-8">
          <HeroDemoSandbox />
        </div>
      </div>
    </section>
  );
};
