import React from 'react';
import { BarChart3, Zap, ShieldAlert, Cpu, Award } from 'lucide-react';

interface BenchmarkRow {
  workload: string;
  method: string;
  mean: string;
  allocation: string;
  zeroAlloc: boolean;
  barPercent: number;
}

const BENCHMARK_DATA: BenchmarkRow[] = [
  {
    workload: 'Clean String (Fast Path)',
    method: 'CleanString_AutoNormalize',
    mean: '4.83 ns',
    allocation: '0 B',
    zeroAlloc: true,
    barPercent: 3,
  },
  {
    workload: 'Whitespace Only to Null',
    method: 'WhitespaceOnly_AutoNormalize',
    mean: '12.49 ns',
    allocation: '0 B',
    zeroAlloc: true,
    barPercent: 8,
  },
  {
    workload: 'Dirty String Normalization',
    method: 'DirtyString_AutoNormalize',
    mean: '17.35 ns',
    allocation: '112 B',
    zeroAlloc: false,
    barPercent: 12,
  },
  {
    workload: 'Decomposed Unicode (Form C)',
    method: 'DecomposedUnicode_AutoNormalize',
    mean: '136.37 ns',
    allocation: '32 B',
    zeroAlloc: false,
    barPercent: 35,
  },
  {
    workload: 'Typical DTO (6 Properties)',
    method: 'TypicalDto_AutoNormalize',
    mean: '283.55 ns',
    allocation: '912 B',
    zeroAlloc: false,
    barPercent: 55,
  },
  {
    workload: 'JSON Deserialization + Normalize',
    method: 'Deserialize_AutoNormalize',
    mean: '601.90 ns',
    allocation: '952 B',
    zeroAlloc: false,
    barPercent: 85,
  },
  {
    workload: 'EF Core SaveChanges Interception',
    method: 'SaveChanges_WithAutoNormalize',
    mean: '22.23 µs',
    allocation: '30.95 KB',
    zeroAlloc: false,
    barPercent: 100,
  },
];

export const Benchmarks: React.FC = () => {
  return (
    <section id="benchmarks" className="py-20 px-4 sm:px-6 lg:px-8 max-w-7xl mx-auto border-t border-slate-800/60">
      <div className="text-center max-w-3xl mx-auto mb-14 space-y-3">
        <h2 className="text-xs uppercase font-semibold tracking-wider text-amber-400 flex items-center justify-center gap-1.5">
          <Zap className="w-3.5 h-3.5 text-amber-400" /> Audited Microbenchmarks
        </h2>
        <p className="text-3xl sm:text-4xl font-bold text-white tracking-tight">
          Sub-5 Nanosecond Fast Path
        </p>
        <p className="text-slate-400 text-sm sm:text-base leading-relaxed">
          Tested with BenchmarkDotNet v0.14.0 on .NET 9.0 (RyuJIT AVX2, Windows 11). All runs enforce dynamic runtime allocations to prevent RyuJIT dead-code elimination.
        </p>
      </div>

      {/* Main Benchmark Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8 mb-12">
        {/* Fast Path Spotlight Card */}
        <div className="lg:col-span-1 rounded-2xl border border-violet-500/30 bg-gradient-to-br from-violet-950/40 to-slate-900/60 p-6 flex flex-col justify-between shadow-xl">
          <div className="space-y-4">
            <div className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-emerald-500/10 border border-emerald-500/30 text-emerald-400 text-xs font-semibold">
              <Award className="w-3.5 h-3.5" /> Zero Heap Allocation Invariant
            </div>
            
            <h3 className="text-2xl font-bold text-white tracking-tight">
              4.83 Nanoseconds. <br />
              <span className="text-transparent bg-clip-text bg-gradient-to-r from-violet-400 to-cyan-400">
                0 Bytes Allocated.
              </span>
            </h3>

            <p className="text-xs text-slate-300 leading-relaxed">
              When an ingress string is already trimmed and in canonical Unicode Form C, AutoNormalize validates boundary conditions without allocating string duplicates on the managed heap.
            </p>

            <div className="p-3.5 rounded-xl bg-navy-950 border border-slate-800/80 space-y-2 text-xs">
              <div className="flex justify-between">
                <span className="text-slate-400">Target Runtime:</span>
                <span className="font-mono text-white">.NET 9.0 (STS) / .NET 8.0</span>
              </div>
              <div className="flex justify-between">
                <span className="text-slate-400">Execution Engine:</span>
                <span className="font-mono text-white">RyuJIT x64 AVX2</span>
              </div>
              <div className="flex justify-between">
                <span className="text-slate-400">Benchmark Tool:</span>
                <span className="font-mono text-white">BenchmarkDotNet v0.14.0</span>
              </div>
            </div>
          </div>

          <div className="pt-6 border-t border-slate-800/80">
            <a
              href="https://github.com/SkyDevLab/DotNet.AutoNormalize/blob/main/docs/benchmark-audit.md"
              target="_blank"
              rel="noreferrer"
              className="text-xs font-semibold text-cyan-400 hover:text-cyan-300 flex items-center gap-1"
            >
              Read full Benchmark Audit & Methodology →
            </a>
          </div>
        </div>

        {/* Benchmark Results Table */}
        <div className="lg:col-span-2 rounded-2xl border border-slate-800 bg-navy-950 overflow-hidden shadow-2xl">
          <div className="px-6 py-4 bg-slate-900/60 border-b border-slate-800 flex items-center justify-between">
            <span className="font-semibold text-sm text-white flex items-center gap-2">
              <BarChart3 className="w-4 h-4 text-cyan-400" /> BenchmarkDotNet Execution Metrics
            </span>
            <span className="text-xs font-mono text-slate-400">RyuJIT AVX2 (Windows 11)</span>
          </div>

          <div className="p-6 overflow-x-auto">
            <table className="w-full text-left text-xs font-mono">
              <thead>
                <tr className="border-b border-slate-800 text-slate-400">
                  <th className="pb-3 font-medium">Workload</th>
                  <th className="pb-3 font-medium">Method</th>
                  <th className="pb-3 font-medium text-right">Mean Latency</th>
                  <th className="pb-3 font-medium text-right">Allocated</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-800/60">
                {BENCHMARK_DATA.map((row, idx) => (
                  <tr key={idx} className="hover:bg-slate-900/40 transition-colors">
                    <td className="py-3 text-slate-200 font-sans font-medium">
                      {row.workload}
                    </td>
                    <td className="py-3 text-slate-400">
                      <code>{row.method}</code>
                    </td>
                    <td className="py-3 text-right font-bold text-white">
                      {row.mean}
                    </td>
                    <td className="py-3 text-right">
                      {row.zeroAlloc ? (
                        <span className="px-2 py-0.5 rounded bg-emerald-500/10 text-emerald-400 border border-emerald-500/30">
                          0 B (Zero Alloc)
                        </span>
                      ) : (
                        <span className="text-slate-300">{row.allocation}</span>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>

      {/* The 0.0077 ns RyuJIT Constant-Folding Lesson */}
      <div className="rounded-xl border border-slate-800/80 bg-slate-900/50 p-6 flex flex-col md:flex-row items-start gap-4">
        <div className="p-3 rounded-lg bg-violet-500/10 border border-violet-500/20 text-violet-400 flex-shrink-0">
          <Cpu className="w-6 h-6" />
        </div>
        <div className="space-y-1.5 text-xs text-slate-300">
          <h4 className="font-semibold text-white text-sm">
            Engineering Story: The 0.0077 ns RyuJIT Constant-Folding Trap
          </h4>
          <p className="leading-relaxed text-slate-400">
            During early microbenchmarks, our baseline test clocked in at 0.0077 ns per operation (~0.03 CPU cycles). The RyuJIT compiler had recognized compile-time string constants and eliminated the method invocation entirely. 
            All AutoNormalize benchmarks mandate runtime non-constant string construction (<code className="text-violet-300 font-mono">string.Concat</code>) across warm iterations to ensure 100% authentic CPU measurement.
          </p>
        </div>
      </div>
    </section>
  );
};
