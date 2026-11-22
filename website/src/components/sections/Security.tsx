import React from 'react';
import { ShieldCheck, AlertOctagon, Lock, EyeOff, CheckCircle } from 'lucide-react';

export const Security: React.FC = () => {
  const sensitiveNames = [
    'Password', 'Passcode', 'Token', 'AuthToken', 'AccessToken', 'RefreshToken',
    'Secret', 'ClientSecret', 'SecretKey', 'ApiKey', 'PrivateKey', 'Signature',
    'Hash', 'Salt', 'Passphrase', 'Certificate'
  ];

  return (
    <section id="security" className="py-20 px-4 sm:px-6 lg:px-8 max-w-7xl mx-auto border-t border-slate-800/60">
      <div className="text-center max-w-3xl mx-auto mb-14 space-y-3">
        <h2 className="text-xs uppercase font-semibold tracking-wider text-emerald-400 flex items-center justify-center gap-1.5">
          <ShieldCheck className="w-3.5 h-3.5 text-emerald-400" /> Security Model & Boundaries
        </h2>
        <p className="text-3xl sm:text-4xl font-bold text-white tracking-tight">
          Safe by Default. Defense in Depth.
        </p>
        <p className="text-slate-400 text-sm sm:text-base leading-relaxed">
          Input normalization is strictly representation canonicalization. It does not replace input validation or HTML sanitization.
        </p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8 mb-12">
        {/* Normalization Boundary Invariants */}
        <div className="rounded-2xl border border-slate-800 bg-slate-900/60 p-6 flex flex-col justify-between">
          <div className="space-y-4">
            <h3 className="text-lg font-semibold text-white flex items-center gap-2">
              <AlertOctagon className="w-5 h-5 text-amber-400" />
              Three Core Boundary Invariants
            </h3>

            <div className="space-y-3 text-xs leading-relaxed">
              <div className="p-3.5 rounded-xl bg-navy-950 border border-slate-800 space-y-1">
                <span className="font-semibold text-white flex items-center gap-1.5">
                  <CheckCircle className="w-3.5 h-3.5 text-violet-400" />
                  1. Normalization is NOT Validation
                </span>
                <p className="text-slate-400">
                  Normalization canonicalizes representations (e.g. trimming whitespace, converting empty strings to null). It never asserts business constraints, ranges, or email formats. Always pair with <strong>FluentValidation</strong> or DataAnnotations.
                </p>
              </div>

              <div className="p-3.5 rounded-xl bg-navy-950 border border-slate-800 space-y-1">
                <span className="font-semibold text-white flex items-center gap-1.5">
                  <CheckCircle className="w-3.5 h-3.5 text-violet-400" />
                  2. Normalization is NOT Sanitization
                </span>
                <p className="text-slate-400">
                  AutoNormalize does not strip HTML tags or escape JavaScript execution vectors. For cross-site scripting (XSS) defense, utilize dedicated HTML sanitizers like HtmlSanitizer.
                </p>
              </div>

              <div className="p-3.5 rounded-xl bg-navy-950 border border-slate-800 space-y-1">
                <span className="font-semibold text-white flex items-center gap-1.5">
                  <CheckCircle className="w-3.5 h-3.5 text-violet-400" />
                  3. Normalization is NOT SQL Injection Defense
                </span>
                <p className="text-slate-400">
                  Never rely on string trimming or canonicalization to protect against SQL injection. Always utilize parameterized queries and Entity Framework Core parameter binding.
                </p>
              </div>
            </div>
          </div>

          <div className="pt-4 mt-4 border-t border-slate-800/80">
            <a
              href="https://github.com/SkyDevLab/DotNet.AutoNormalize/blob/main/docs/security-model.md"
              target="_blank"
              rel="noreferrer"
              className="text-xs font-semibold text-emerald-400 hover:text-emerald-300"
            >
              Read full Security Boundaries RFC →
            </a>
          </div>
        </div>

        {/* Heuristic Sensitive Shielding */}
        <div className="rounded-2xl border border-slate-800 bg-slate-900/60 p-6 flex flex-col justify-between">
          <div className="space-y-4">
            <h3 className="text-lg font-semibold text-white flex items-center gap-2">
              <Lock className="w-5 h-5 text-cyan-400" />
              Heuristic Sensitive Property Shielding
            </h3>

            <p className="text-xs text-slate-300 leading-relaxed">
              To prevent accidental password modification, AutoNormalize automatically shields properties matching sensitive tokens from whitespace stripping or mutation:
            </p>

            <div className="grid grid-cols-2 sm:grid-cols-4 gap-2 pt-1 font-mono text-xs">
              {sensitiveNames.map((name) => (
                <div
                  key={name}
                  className="px-2.5 py-1.5 rounded-lg bg-navy-950 border border-slate-800 text-slate-300 text-center"
                >
                  {name}
                </div>
              ))}
            </div>

            <div className="p-3.5 rounded-xl bg-navy-950 border border-slate-800 text-xs font-mono text-slate-300">
              <span className="text-slate-500">// Explicit declarative override:</span>
              <pre className="text-cyan-300 mt-1">
{`[PreserveWhitespace]
public string RawVerificationCode { get; set; }

[DoNotNormalize]
public string PgpSignature { get; set; }`}
              </pre>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
};
