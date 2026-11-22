import React from 'react';
import { Github, Package, MessageSquare, ArrowRight, Star, Heart } from 'lucide-react';

export const CommunityCta: React.FC = () => {
  return (
    <section className="py-20 px-4 sm:px-6 lg:px-8 max-w-7xl mx-auto border-t border-slate-800/60 text-center">
      <div className="relative rounded-3xl border border-violet-500/30 bg-gradient-to-b from-slate-900/80 via-navy-950 to-navy-950 p-8 sm:p-14 overflow-hidden shadow-2xl">
        {/* Glow */}
        <div className="absolute top-0 left-1/2 -translate-x-1/2 w-96 h-48 bg-violet-600/20 blur-[100px] pointer-events-none rounded-full" />

        <div className="relative z-10 max-w-3xl mx-auto space-y-6">
          <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-violet-500/10 border border-violet-500/20 text-xs font-semibold text-violet-300">
            <Star className="w-3.5 h-3.5 text-amber-400 fill-amber-400" /> Free & Open Source under MIT
          </div>

          <h2 className="text-3xl sm:text-5xl font-extrabold text-white tracking-tight leading-tight">
            Ready to standardize your API inputs?
          </h2>

          <p className="text-slate-300 text-sm sm:text-base leading-relaxed max-w-xl mx-auto">
            Say goodbye to redundant <code className="text-cyan-300 font-mono">.Trim()</code> boilerplate and corrupted passwords. Start canonicalizing data at ingress and persistence boundaries today.
          </p>

          <div className="flex flex-col sm:flex-row items-center justify-center gap-4 pt-3">
            <a
              href="https://github.com/SkyDevLab/DotNet.AutoNormalize"
              target="_blank"
              rel="noreferrer"
              className="flex items-center gap-2 px-6 py-3 rounded-xl bg-violet-600 hover:bg-violet-500 text-white font-semibold text-sm transition-all shadow-[0_0_25px_rgba(139,92,246,0.4)]"
            >
              <Github className="w-4 h-4" />
              <span>Star on GitHub</span>
            </a>

            <a
              href="https://github.com/SkyDevLab/DotNet.AutoNormalize/discussions"
              target="_blank"
              rel="noreferrer"
              className="flex items-center gap-2 px-6 py-3 rounded-xl bg-slate-900 hover:bg-slate-800 border border-slate-700 text-slate-200 font-semibold text-sm transition-all"
            >
              <MessageSquare className="w-4 h-4 text-cyan-400" />
              <span>Join Architecture Discussion</span>
            </a>
          </div>
        </div>
      </div>
    </section>
  );
};
