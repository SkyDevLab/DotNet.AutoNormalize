import React, { useState } from 'react';
import { Github, Package, ExternalLink, Menu, X, Shield, Terminal, Zap, BookOpen } from 'lucide-react';

interface NavbarProps {
  currentTab: string;
  setCurrentTab: (tab: string) => void;
}

export const Navbar: React.FC<NavbarProps> = ({ currentTab, setCurrentTab }) => {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  const navItems = [
    { id: 'home', label: 'Home' },
    { id: 'features', label: 'Features' },
    { id: 'integrations', label: 'Integrations' },
    { id: 'benchmarks', label: 'Benchmarks' },
    { id: 'security', label: 'Security' },
    { id: 'docs', label: 'Docs' },
  ];

  const handleNavClick = (id: string) => {
    setCurrentTab(id);
    setMobileMenuOpen(false);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  return (
    <header className="sticky top-0 z-50 w-full border-b border-slate-800/80 bg-navy-950/80 backdrop-blur-md">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between">
        {/* Brand */}
        <div 
          onClick={() => handleNavClick('home')}
          className="flex items-center gap-3 cursor-pointer group"
        >
          <div className="w-9 h-9 rounded-lg bg-gradient-to-br from-violet-500/20 to-cyan-500/20 border border-violet-500/30 flex items-center justify-center p-1.5 shadow-[0_0_15px_rgba(139,92,246,0.2)] group-hover:border-violet-400 transition-colors">
            <img src="./favicon.png" alt="AutoNormalize" className="w-full h-full object-contain" />
          </div>
          <div className="flex items-center gap-2">
            <span className="font-bold text-lg text-white tracking-tight group-hover:text-violet-300 transition-colors">
              AutoNormalize
            </span>
            <span className="hidden sm:inline-flex items-center px-2 py-0.5 text-xs font-medium rounded-full bg-violet-500/10 text-violet-300 border border-violet-500/20">
              v1.0.0 GA
            </span>
          </div>
        </div>

        {/* Desktop Nav */}
        <nav className="hidden md:flex items-center gap-1 text-sm font-medium">
          {navItems.map((item) => (
            <button
              key={item.id}
              onClick={() => handleNavClick(item.id)}
              className={`px-3 py-1.5 rounded-md transition-colors ${
                currentTab === item.id
                  ? 'text-white bg-slate-800/80 shadow-sm'
                  : 'text-slate-400 hover:text-white hover:bg-slate-800/40'
              }`}
            >
              {item.label}
            </button>
          ))}
        </nav>

        {/* Action buttons */}
        <div className="hidden sm:flex items-center gap-3">
          <a
            href="https://www.nuget.org/packages/AutoNormalize.Core/1.0.0"
            target="_blank"
            rel="noreferrer"
            className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-mono font-medium rounded-md bg-slate-900 border border-slate-700/80 text-cyan-400 hover:border-cyan-500/40 hover:bg-slate-850 transition-all shadow-sm"
          >
            <Package className="w-3.5 h-3.5" />
            <span>NuGet 1.0.0</span>
          </a>
          <a
            href="https://github.com/SkyDevLab/DotNet.AutoNormalize"
            target="_blank"
            rel="noreferrer"
            className="flex items-center gap-2 px-3.5 py-1.5 text-xs font-medium rounded-md bg-violet-600 hover:bg-violet-500 text-white transition-all shadow-[0_0_15px_rgba(139,92,246,0.3)] hover:shadow-[0_0_20px_rgba(139,92,246,0.5)]"
          >
            <Github className="w-3.5 h-3.5" />
            <span>GitHub</span>
          </a>
        </div>

        {/* Mobile menu button */}
        <button
          onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
          className="md:hidden p-2 text-slate-400 hover:text-white focus:outline-none"
          aria-label="Toggle menu"
        >
          {mobileMenuOpen ? <X className="w-6 h-6" /> : <Menu className="w-6 h-6" />}
        </button>
      </div>

      {/* Mobile menu dropdown */}
      {mobileMenuOpen && (
        <div className="md:hidden border-b border-slate-800 bg-navy-950/95 backdrop-blur-xl px-4 pt-3 pb-6 space-y-2">
          {navItems.map((item) => (
            <button
              key={item.id}
              onClick={() => handleNavClick(item.id)}
              className={`w-full text-left px-3 py-2 rounded-md text-base font-medium transition-colors ${
                currentTab === item.id
                  ? 'text-white bg-slate-800'
                  : 'text-slate-400 hover:text-white hover:bg-slate-850'
              }`}
            >
              {item.label}
            </button>
          ))}
          <div className="pt-4 border-t border-slate-800/80 flex flex-col gap-2">
            <a
              href="https://www.nuget.org/packages/AutoNormalize.Core/1.0.0"
              target="_blank"
              rel="noreferrer"
              className="flex items-center justify-center gap-2 py-2 text-sm font-mono rounded-md bg-slate-900 border border-slate-700 text-cyan-400"
            >
              <Package className="w-4 h-4" />
              <span>NuGet 1.0.0</span>
            </a>
            <a
              href="https://github.com/SkyDevLab/DotNet.AutoNormalize"
              target="_blank"
              rel="noreferrer"
              className="flex items-center justify-center gap-2 py-2 text-sm font-medium rounded-md bg-violet-600 text-white"
            >
              <Github className="w-4 h-4" />
              <span>Star on GitHub</span>
            </a>
          </div>
        </div>
      )}
    </header>
  );
};
