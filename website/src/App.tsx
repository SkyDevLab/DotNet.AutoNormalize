import React, { useState } from 'react';
import { Navbar } from './components/layout/Navbar';
import { Footer } from './components/layout/Footer';
import { HeroSection } from './components/hero/HeroSection';
import { ProblemSolution } from './components/sections/ProblemSolution';
import { KeyFeatures } from './components/sections/KeyFeatures';
import { Integrations } from './components/sections/Integrations';
import { Benchmarks } from './components/sections/Benchmarks';
import { Security } from './components/sections/Security';
import { Installation } from './components/sections/Installation';
import { CommunityCta } from './components/sections/CommunityCta';
import { DocumentationPage } from './components/pages/DocumentationPage';

export const App: React.FC = () => {
  const [currentTab, setCurrentTab] = useState<string>('home');

  return (
    <div className="min-h-screen flex flex-col bg-navy-950 text-slate-100 selection:bg-violet-500/30 selection:text-violet-200">
      <Navbar currentTab={currentTab} setCurrentTab={setCurrentTab} />

      <main className="flex-1">
        {currentTab === 'home' && (
          <>
            {/* 3D Hero only in Home */}
            <HeroSection />
            <ProblemSolution />
            <KeyFeatures />
            <Integrations />
            <Benchmarks />
            <Security />
            <Installation />
            <CommunityCta />
          </>
        )}

        {currentTab === 'features' && (
          <div className="pt-8">
            <KeyFeatures />
            <ProblemSolution />
            <CommunityCta />
          </div>
        )}

        {currentTab === 'integrations' && (
          <div className="pt-8">
            <Integrations />
            <Installation />
            <CommunityCta />
          </div>
        )}

        {currentTab === 'benchmarks' && (
          <div className="pt-8">
            <Benchmarks />
            <CommunityCta />
          </div>
        )}

        {currentTab === 'security' && (
          <div className="pt-8">
            <Security />
            <CommunityCta />
          </div>
        )}

        {currentTab === 'docs' && (
          <div className="pt-8">
            <DocumentationPage />
          </div>
        )}
      </main>

      <Footer />
    </div>
  );
};

export default App;
