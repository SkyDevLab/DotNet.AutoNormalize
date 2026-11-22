import React from 'react';
import { Github, Package, Shield, ExternalLink, Heart } from 'lucide-react';

export const Footer: React.FC = () => {
  return (
    <footer className="w-full border-t border-slate-800/80 bg-navy-950 py-12 text-slate-400 text-sm">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-8 mb-12">
          {/* Brand Info */}
          <div className="md:col-span-1 space-y-3">
            <div className="flex items-center gap-2.5">
              <div className="w-7 h-7 rounded-md bg-violet-500/20 border border-violet-500/30 flex items-center justify-center p-1">
                <img src="./favicon.png" alt="AutoNormalize" className="w-full h-full object-contain" />
              </div>
              <span className="font-bold text-white tracking-tight">AutoNormalize</span>
            </div>
            <p className="text-xs text-slate-400 leading-relaxed">
              Safe, fast, and consistent input canonicalization for modern .NET 8, 9, and ASP.NET Core applications.
            </p>
            <div className="flex items-center gap-2 text-xs text-slate-400 pt-1">
              <span>Released under the MIT License</span>
            </div>
          </div>

          {/* Ecosystem */}
          <div>
            <h4 className="font-semibold text-white text-xs uppercase tracking-wider mb-3">Ecosystem</h4>
            <ul className="space-y-2 text-xs">
              <li>
                <a href="https://www.nuget.org/packages/AutoNormalize.Core" target="_blank" rel="noreferrer" className="hover:text-cyan-400 transition-colors">
                  AutoNormalize.Core
                </a>
              </li>
              <li>
                <a href="https://www.nuget.org/packages/AutoNormalize.AspNetCore" target="_blank" rel="noreferrer" className="hover:text-cyan-400 transition-colors">
                  AutoNormalize.AspNetCore
                </a>
              </li>
              <li>
                <a href="https://www.nuget.org/packages/AutoNormalize.Json" target="_blank" rel="noreferrer" className="hover:text-cyan-400 transition-colors">
                  AutoNormalize.Json
                </a>
              </li>
              <li>
                <a href="https://www.nuget.org/packages/AutoNormalize.EntityFrameworkCore" target="_blank" rel="noreferrer" className="hover:text-cyan-400 transition-colors">
                  AutoNormalize.EntityFrameworkCore
                </a>
              </li>
              <li>
                <a href="https://www.nuget.org/packages/AutoNormalize.Abstractions" target="_blank" rel="noreferrer" className="hover:text-cyan-400 transition-colors">
                  AutoNormalize.Abstractions
                </a>
              </li>
            </ul>
          </div>

          {/* Documentation */}
          <div>
            <h4 className="font-semibold text-white text-xs uppercase tracking-wider mb-3">Documentation</h4>
            <ul className="space-y-2 text-xs">
              <li>
                <a href="https://github.com/SkyDevLab/DotNet.AutoNormalize/blob/main/docs/installation.md" target="_blank" rel="noreferrer" className="hover:text-violet-400 transition-colors flex items-center gap-1">
                  Installation Guide <ExternalLink className="w-3 h-3 text-slate-400" />
                </a>
              </li>
              <li>
                <a href="https://github.com/SkyDevLab/DotNet.AutoNormalize/blob/main/docs/api-design-proposal.md" target="_blank" rel="noreferrer" className="hover:text-violet-400 transition-colors flex items-center gap-1">
                  Architecture Proposal <ExternalLink className="w-3 h-3 text-slate-400" />
                </a>
              </li>
              <li>
                <a href="https://github.com/SkyDevLab/DotNet.AutoNormalize/blob/main/docs/security-model.md" target="_blank" rel="noreferrer" className="hover:text-violet-400 transition-colors flex items-center gap-1">
                  Security Model <ExternalLink className="w-3 h-3 text-slate-400" />
                </a>
              </li>
              <li>
                <a href="https://github.com/SkyDevLab/DotNet.AutoNormalize/blob/main/docs/benchmark-audit.md" target="_blank" rel="noreferrer" className="hover:text-violet-400 transition-colors flex items-center gap-1">
                  Benchmark Audit <ExternalLink className="w-3 h-3 text-slate-400" />
                </a>
              </li>
            </ul>
          </div>

          {/* Community */}
          <div>
            <h4 className="font-semibold text-white text-xs uppercase tracking-wider mb-3">Community</h4>
            <ul className="space-y-2 text-xs">
              <li>
                <a href="https://github.com/SkyDevLab/DotNet.AutoNormalize" target="_blank" rel="noreferrer" className="hover:text-white transition-colors flex items-center gap-1">
                  <Github className="w-3.5 h-3.5" /> GitHub Repository
                </a>
              </li>
              <li>
                <a href="https://github.com/SkyDevLab/DotNet.AutoNormalize/discussions" target="_blank" rel="noreferrer" className="hover:text-white transition-colors flex items-center gap-1">
                  GitHub Discussions
                </a>
              </li>
              <li>
                <a href="https://github.com/SkyDevLab/DotNet.AutoNormalize/issues" target="_blank" rel="noreferrer" className="hover:text-white transition-colors flex items-center gap-1">
                  Issue Tracker
                </a>
              </li>
              <li>
                <a href="https://github.com/SkyDevLab/DotNet.AutoNormalize/releases/tag/v1.0.0" target="_blank" rel="noreferrer" className="hover:text-white transition-colors flex items-center gap-1">
                  v1.0.0 Release Notes
                </a>
              </li>
            </ul>
          </div>
        </div>

        {/* Bottom bar */}
        <div className="pt-8 border-t border-slate-800/60 flex flex-col sm:flex-row items-center justify-between gap-4 text-xs text-slate-400">
          <p>© {new Date().getFullYear()} SkyDevLab & DotNet.AutoNormalize Contributors. All rights reserved.</p>
          <div className="flex items-center gap-1 text-slate-400">
            <span>Maintained with precision by</span>
            <a href="https://github.com/SkyDevLab" target="_blank" rel="noreferrer" className="text-slate-300 hover:text-white font-medium">
              SkyDevLab
            </a>
          </div>
        </div>
      </div>
    </footer>
  );
};
