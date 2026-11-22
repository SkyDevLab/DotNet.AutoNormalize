import React from 'react';
import { AlertTriangle, CheckCircle2, ShieldAlert, Cpu, Database } from 'lucide-react';

export const ProblemSolution: React.FC = () => {
  const traps = [
    {
      icon: <ShieldAlert className="w-5 h-5 text-amber-400" />,
      title: "1. The Password & Token Corruption Trap",
      problem: "Naive global string converters or reflectively trimming every string property in a request silently alters passwords, tokens, API keys, or Base64 signatures containing intentional leading/trailing spaces.",
      solution: "AutoNormalize applies defense-in-depth heuristic shielding for sensitive property names (Password, Token, ApiKey, Secret, Salt) and declarative attributes ([DoNotNormalize], [PreserveWhitespace]).",
      codeProblem: `// Naive global converter corrupts user password!
user.Password = "  P@ssw0rd123!  ".Trim(); 
// Result: "P@ssw0rd123!" -> Login fails on client!`,
      codeSolution: `// AutoNormalize heuristic shield protects sensitive properties
public string Password { get; set; } // Shielded automatically
[DoNotNormalize]
public string Signature { get; set; } // Opt-out preserved verbatim`,
    },
    {
      icon: <Cpu className="w-5 h-5 text-cyan-400" />,
      title: "2. The Immutable C# Positional Record Trap",
      problem: "Positional records (public record User(string Name, string Email)) have init-only properties and bind constructor parameters. Post-deserialization reflection mutation either throws or violates immutability invariants.",
      solution: "AutoNormalize hooks IJsonTypeInfoResolver.Modifiers in System.Text.Json to canonicalize tokens in-flight directly off the Utf8JsonReader stream before constructor parameters are invoked.",
      codeProblem: `// Post-deserialization reflection cannot mutate init properties!
var record = JsonSerializer.Deserialize<UserRecord>(json);
typeof(UserRecord).GetProperty("Name")?.SetValue(record, "Clean"); 
// Throws or breaks record immutability!`,
      codeSolution: `// Normalized during System.Text.Json contract resolution
builder.Services.AddAutoNormalize();

// Positional records are normalized AT BIRTH:
app.MapPost("/users", (UserRecord user) => Results.Ok(user));`,
    },
    {
      icon: <Database className="w-5 h-5 text-violet-400" />,
      title: "3. The Dictionary Key Collision Disaster",
      problem: "Normalizing dictionary keys in-place invalidates internal hashcode bucket indexing. If keys 'item ' and 'item' are normalized to the same string, Dictionary throws an unhandled duplicate key exception.",
      solution: "AutoNormalize enforces strict boundary invariants: dictionary string values are safely canonicalized, while dictionary keys are strictly preserved immutable.",
      codeProblem: `// In-place key mutation crashes dictionary collections:
dict["key "] = 1;
dict["key"] = 2;
// In-place trim triggers: ArgumentException (Key already added)`,
      codeSolution: `// AutoNormalize traverses object graphs safely:
// Dictionary values are canonicalized; keys are never mutated.
Dictionary<string, string> tags = normalizer.Normalize(inputTags);`,
    },
  ];

  return (
    <section id="problem-solution" className="py-20 px-4 sm:px-6 lg:px-8 max-w-7xl mx-auto border-t border-slate-800/60">
      <div className="text-center max-w-3xl mx-auto mb-16 space-y-3">
        <h2 className="text-xs uppercase font-semibold tracking-wider text-cyan-400">
          Why DotNet.AutoNormalize Exists
        </h2>
        <p className="text-3xl sm:text-4xl font-bold text-white tracking-tight">
          The Hidden Pitfalls of Manual Input Trimming
        </p>
        <p className="text-slate-400 text-sm sm:text-base leading-relaxed">
          Input normalization seems trivial until you encounter edge cases that crash APIs, mangle cryptographic keys, or introduce security flaws.
        </p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {traps.map((trap, idx) => (
          <div
            key={idx}
            className="rounded-xl border border-slate-800/80 bg-slate-900/60 p-6 flex flex-col justify-between hover:border-slate-700 transition-colors"
          >
            <div>
              <div className="flex items-center gap-2.5 mb-3">
                <div className="p-2 rounded-lg bg-slate-800/90 border border-slate-700/60">
                  {trap.icon}
                </div>
                <h3 className="text-base font-semibold text-white tracking-tight leading-snug">
                  {trap.title}
                </h3>
              </div>
              
              <div className="space-y-3 mb-5">
                <div className="p-3 rounded-lg bg-red-950/20 border border-red-900/30 text-xs text-red-200/90 leading-relaxed">
                  <span className="font-semibold text-red-400 block mb-1 flex items-center gap-1">
                    <AlertTriangle className="w-3.5 h-3.5" /> The Bug
                  </span>
                  {trap.problem}
                </div>

                <div className="p-3 rounded-lg bg-emerald-950/20 border border-emerald-900/30 text-xs text-emerald-200/90 leading-relaxed">
                  <span className="font-semibold text-emerald-400 block mb-1 flex items-center gap-1">
                    <CheckCircle2 className="w-3.5 h-3.5" /> How AutoNormalize Solves It
                  </span>
                  {trap.solution}
                </div>
              </div>
            </div>

            {/* Code snippets */}
            <div className="rounded-lg bg-navy-950 p-3 border border-slate-800 text-[11px] font-mono text-slate-300 overflow-x-auto">
              <pre className="text-slate-300 leading-normal">{trap.codeSolution}</pre>
            </div>
          </div>
        ))}
      </div>
    </section>
  );
};
