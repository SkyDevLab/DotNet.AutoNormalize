# Security Policy: DotNet.AutoNormalize

## Supported Versions

| Version | Supported |
| :--- | :--- |
| 1.0.x | Yes |
| < 1.0 | No |

## Reporting a Security Vulnerability

If you discover a potential security vulnerability in DotNet.AutoNormalize, please report it privately.

**Do not open a public GitHub issue for security vulnerabilities.**

### Contact
Please send vulnerability reports to:
`security@dotnet-autonormalize.org` (or via GitHub Private Vulnerability Reporting on the repository).

### Maintainer Response Expectations & SLA
As an independent open-source project maintained by community engineers:
- **Initial Acknowledgment**: Within **7 business days** of receiving your report.
- **Triage & Assessment**: Within **14 business days** of acknowledgment.
- **Fix & Coordinated Release**: Fixes are targeted for release within **90 days** from initial report, adhering to coordinated vulnerability disclosure best practices.

## Security Posture & Intentional Boundaries

1. **Representation Canonicalization Only**: DotNet.AutoNormalize canonicalizes representations (trimming, Unicode normalization, empty-to-null). It does not perform input sanitization or business rule validation.
2. **Sensitive Fields**: Passwords, API tokens, cryptographic secrets, and hashes are shielded via heuristic exclusions and explicit attributes (`[DoNotNormalize]`, `[PreserveWhitespace]`).
