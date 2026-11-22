import React, { useState, useMemo } from 'react';
import { Play, Check, ShieldCheck, Zap, RotateCcw, Sparkles } from 'lucide-react';

interface Preset {
  name: string;
  field: string;
  value: string;
  description: string;
}

const PRESETS: Preset[] = [
  {
    name: 'Dirty User Input',
    field: 'FullName',
    value: '   Ada Lovelace   ',
    description: 'Surrounded by irregular whitespace tokens',
  },
  {
    name: 'Whitespace Only',
    field: 'UserBio',
    value: '      ',
    description: 'Tabs and spaces without content, converts to null',
  },
  {
    name: 'Decomposed Unicode',
    field: 'CityName',
    value: 'cafe\u0301', // Decomposed e + acute accent
    description: 'Decomposed e + acute accent -> Canonical Form C (é)',
  },
  {
    name: 'Sensitive Password',
    field: 'UserPassword',
    value: '  secret-p@ssw0rd!  ',
    description: 'Heuristic sensitive shield protects from modification',
  },
  {
    name: 'Clean Fast Path',
    field: 'Username',
    value: 'grace_hopper',
    description: 'Already canonical, takes 4.83 ns zero-alloc fast path',
  },
];

export const HeroDemoSandbox: React.FC = () => {
  const [selectedPreset, setSelectedPreset] = useState<number>(0);
  const [propertyName, setPropertyName] = useState(PRESETS[0].field);
  const [inputValue, setInputValue] = useState(PRESETS[0].value);

  // Normalization options
  const [trimStrings, setTrimStrings] = useState(true);
  const [emptyToNull, setEmptyToNull] = useState(true);
  const [unicodeFormC, setUnicodeFormC] = useState(true);
  const [shieldSensitive, setShieldSensitive] = useState(true);
  const [casing, setCasing] = useState<'none' | 'upper' | 'lower'>('none');

  const applyPreset = (index: number) => {
    setSelectedPreset(index);
    setPropertyName(PRESETS[index].field);
    setInputValue(PRESETS[index].value);
  };

  // Execute canonical normalization logic matching AutoNormalize.Core
  const result = useMemo(() => {
    const sensitiveNames = ['password', 'passcode', 'token', 'secret', 'apikey', 'signature', 'salt'];
    const isSensitive = sensitiveNames.some((s) => propertyName.toLowerCase().includes(s));

    // Shielding check
    if (shieldSensitive && isSensitive) {
      return {
        output: inputValue,
        isNull: false,
        allocatedBytes: 0,
        latencyEstimate: '0.9 ns',
        isShielded: true,
        isCleanFastPath: false,
        explanation: `Shielded by heuristic guard for sensitive field '${propertyName}'. Preserved verbatim to protect credentials.`,
      };
    }

    let val: string | null = inputValue;
    const explanations: string[] = [];

    // Check clean fast-path invariant
    const hasLeadingTrailingWhitespace = val.length > 0 && (val.startsWith(' ') || val.endsWith(' ') || val.startsWith('\t') || val.endsWith('\t'));
    const isAlreadyFormC = val.normalize('NFC') === val;

    if (!hasLeadingTrailingWhitespace && isAlreadyFormC && casing === 'none') {
      return {
        output: val,
        isNull: false,
        allocatedBytes: 0,
        latencyEstimate: '4.83 ns',
        isShielded: false,
        isCleanFastPath: true,
        explanation: 'Clean String Fast Path: Zero allocations on managed heap (0 Bytes allocated). Returns original reference in 4.83 ns.',
      };
    }

    // 1. Trim
    if (trimStrings && val !== null) {
      const trimmed = val.trim();
      if (trimmed !== val) {
        explanations.push('Trimmed leading/trailing whitespace');
        val = trimmed;
      }
    }

    // 2. Empty to null
    if (emptyToNull && val !== null && val.length === 0) {
      explanations.push('Empty string converted to NULL');
      val = null;
    }

    // 3. Unicode Form C
    if (unicodeFormC && val !== null) {
      const normalized = val.normalize('NFC');
      if (normalized !== val) {
        explanations.push('Canonicalized Unicode into Normalization Form C');
        val = normalized;
      }
    }

    // 4. Casing
    if (casing === 'upper' && val !== null) {
      val = val.toUpperCase();
      explanations.push('Applied UpperCaseInvariant');
    } else if (casing === 'lower' && val !== null) {
      val = val.toLowerCase();
      explanations.push('Applied LowerCaseInvariant');
    }

    const wasModified = val !== inputValue;
    const allocated = val === null ? 0 : wasModified ? val.length * 2 + 24 : 0;

    return {
      output: val,
      isNull: val === null,
      allocatedBytes: allocated,
      latencyEstimate: val === null ? '12.49 ns' : '17.35 ns',
      isShielded: false,
      isCleanFastPath: false,
      explanation: explanations.length > 0 ? explanations.join(' • ') : 'No transformations needed',
    };
  }, [inputValue, propertyName, trimStrings, emptyToNull, unicodeFormC, shieldSensitive, casing]);

  return (
    <div className="w-full glass-panel rounded-2xl p-5 sm:p-6 shadow-[0_20px_50px_rgba(0,0,0,0.6)] border border-slate-800 relative z-10 backdrop-blur-xl">
      {/* Header & Presets */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 pb-4 border-b border-slate-800">
        <div className="flex items-center gap-2">
          <div className="w-2.5 h-2.5 rounded-full bg-cyan-400 animate-ping" />
          <h3 className="text-sm font-semibold text-white tracking-wide uppercase flex items-center gap-1.5">
            <Sparkles className="w-3.5 h-3.5 text-cyan-400" /> Live Normalization Sandbox
          </h3>
        </div>
        
        {/* Presets Bar */}
        <div className="flex flex-wrap items-center gap-1.5">
          {PRESETS.map((preset, idx) => (
            <button
              key={preset.name}
              onClick={() => applyPreset(idx)}
              className={`text-xs px-2.5 py-1 rounded-md transition-all font-medium ${
                selectedPreset === idx
                  ? 'bg-violet-600 text-white shadow-sm'
                  : 'bg-slate-900/90 text-slate-400 hover:text-white hover:bg-slate-800 border border-slate-800'
              }`}
            >
              {preset.name}
            </button>
          ))}
        </div>
      </div>

      {/* Interactive Controls & Inputs */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-5 pt-4">
        {/* Input Column */}
        <div className="space-y-3.5">
          <div className="flex items-center gap-3">
            <div className="w-1/3">
              <label className="block text-xs font-mono text-slate-400 mb-1">Property Name</label>
              <input
                type="text"
                value={propertyName}
                onChange={(e) => setPropertyName(e.target.value)}
                className="w-full bg-slate-900 border border-slate-700/80 rounded-lg px-2.5 py-1.5 text-xs font-mono text-white focus:outline-none focus:border-violet-500"
                placeholder="PropertyName"
              />
            </div>
            <div className="w-2/3">
              <label className="block text-xs font-mono text-slate-400 mb-1">Raw Ingress String</label>
              <input
                type="text"
                value={inputValue}
                onChange={(e) => setInputValue(e.target.value)}
                className="w-full bg-slate-900 border border-slate-700/80 rounded-lg px-3 py-1.5 text-xs font-mono text-white focus:outline-none focus:border-cyan-500"
                placeholder="Enter string to normalize..."
              />
            </div>
          </div>

          {/* Rule Toggles */}
          <div className="grid grid-cols-2 gap-2 pt-1 text-xs text-slate-300">
            <label className="flex items-center gap-2 cursor-pointer select-none bg-slate-900/50 p-2 rounded-md border border-slate-800/80 hover:border-slate-700">
              <input
                type="checkbox"
                checked={trimStrings}
                onChange={(e) => setTrimStrings(e.target.checked)}
                className="rounded border-slate-700 text-violet-600 focus:ring-0"
              />
              <span>Trim Whitespace</span>
            </label>
            <label className="flex items-center gap-2 cursor-pointer select-none bg-slate-900/50 p-2 rounded-md border border-slate-800/80 hover:border-slate-700">
              <input
                type="checkbox"
                checked={emptyToNull}
                onChange={(e) => setEmptyToNull(e.target.checked)}
                className="rounded border-slate-700 text-violet-600 focus:ring-0"
              />
              <span>Empty to NULL</span>
            </label>
            <label className="flex items-center gap-2 cursor-pointer select-none bg-slate-900/50 p-2 rounded-md border border-slate-800/80 hover:border-slate-700">
              <input
                type="checkbox"
                checked={unicodeFormC}
                onChange={(e) => setUnicodeFormC(e.target.checked)}
                className="rounded border-slate-700 text-cyan-600 focus:ring-0"
              />
              <span>Unicode Form C</span>
            </label>
            <label className="flex items-center gap-2 cursor-pointer select-none bg-slate-900/50 p-2 rounded-md border border-slate-800/80 hover:border-slate-700">
              <input
                type="checkbox"
                checked={shieldSensitive}
                onChange={(e) => setShieldSensitive(e.target.checked)}
                className="rounded border-slate-700 text-cyan-600 focus:ring-0"
              />
              <span className="flex items-center gap-1">Shield Sensitive <ShieldCheck className="w-3 h-3 text-cyan-400" /></span>
            </label>
          </div>
        </div>

        {/* Output Column */}
        <div className="flex flex-col justify-between bg-navy-950/90 rounded-xl p-4 border border-slate-800/90 relative overflow-hidden">
          <div className="space-y-2.5">
            <div className="flex items-center justify-between text-xs text-slate-400 pb-1.5 border-b border-slate-800/80">
              <span className="font-mono">Canonical Output</span>
              <div className="flex items-center gap-2">
                <span className="text-[10px] uppercase font-mono px-1.5 py-0.5 rounded bg-slate-800 text-slate-300">
                  Latency ~{result.latencyEstimate}
                </span>
                <span className={`text-[10px] uppercase font-mono px-1.5 py-0.5 rounded ${
                  result.allocatedBytes === 0 ? 'bg-emerald-500/10 text-emerald-400 border border-emerald-500/30' : 'bg-slate-800 text-slate-300'
                }`}>
                  {result.allocatedBytes === 0 ? '0 B (Zero-Alloc)' : `${result.allocatedBytes} Bytes`}
                </span>
              </div>
            </div>

            {/* Display Output */}
            <div className="font-mono text-sm py-2">
              {result.isNull ? (
                <div className="flex items-center gap-2">
                  <span className="text-slate-500">Value:</span>
                  <span className="px-2 py-0.5 rounded bg-violet-950/80 border border-violet-500/30 text-violet-300 font-bold">
                    null
                  </span>
                  <span className="text-xs text-slate-500">(Clean SQL NULL)</span>
                </div>
              ) : (
                <div className="flex items-center gap-2">
                  <span className="text-slate-500">Value:</span>
                  <span className="px-2 py-0.5 rounded bg-slate-900 border border-cyan-500/40 text-cyan-300 font-bold">
                    "{result.output}"
                  </span>
                </div>
              )}
            </div>

            {/* Invariant status badge */}
            <div className="text-xs text-slate-400 leading-relaxed bg-slate-900/40 p-2.5 rounded-lg border border-slate-800/50">
              <p className="flex items-center gap-1.5 text-xs font-semibold text-slate-200 mb-0.5">
                {result.isShielded ? (
                  <span className="text-amber-400 flex items-center gap-1">
                    <ShieldCheck className="w-3.5 h-3.5" /> Heuristic Shield Active
                  </span>
                ) : result.isCleanFastPath ? (
                  <span className="text-emerald-400 flex items-center gap-1">
                    <Zap className="w-3.5 h-3.5" /> Fast Path Triggered
                  </span>
                ) : (
                  <span className="text-cyan-400 flex items-center gap-1">
                    <Check className="w-3.5 h-3.5" /> Transformed
                  </span>
                )}
              </p>
              <p className="text-[11px] text-slate-400">{result.explanation}</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
