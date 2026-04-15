# Release Plan

- Project: LAL
- Baseline Version: 0.1.0
- Date: 2026-04-10

## Versioning Policy

1. Follow Semantic Versioning: MAJOR.MINOR.PATCH.
2. MAJOR: breaking API changes.
3. MINOR: backward-compatible features/perf improvements.
4. PATCH: backward-compatible fixes and documentation corrections.

## Release Workflow

1. Freeze release scope from `TraceabilityMatrix.md` Passed rows.
2. Run full validation:
   - `dotnet test LAL.sln`
   - `powershell -ExecutionPolicy Bypass -File scripts/verify-gate-a.ps1`
   - `powershell -ExecutionPolicy Bypass -File scripts/verify-gate-b.ps1`
   - `powershell -ExecutionPolicy Bypass -File scripts/verify-w3.ps1`
   - `powershell -ExecutionPolicy Bypass -File scripts/verify-w3-and-rules32.ps1`
   - `powershell -ExecutionPolicy Bypass -File scripts/verify-traceability-dashboard.ps1`
   - `powershell -ExecutionPolicy Bypass -File scripts/verify-performance-nodes.ps1`
   - `powershell -ExecutionPolicy Bypass -File scripts/verify-gate-c.ps1`
   - `powershell -ExecutionPolicy Bypass -File scripts/verify-w5.ps1`
3. Update `CHANGELOG.md` with Added/Changed/Fixed sections.
4. Create release tag `vX.Y.Z`.
5. Publish release notes including upgrade notes and known risks.

## v0.1-draft Freeze Gate

1. `scripts/verify-w3-and-rules32.ps1` must report Rules-32 Passed = 32 and no pending/in-review/blocked.
2. `scripts/verify-traceability-dashboard.ps1` must pass and keep dashboard metrics aligned with section 3 rows.
3. CI workflow `build-test-and-gate-a` and `perf-baseline` jobs must both pass before release tag creation.
4. `TraceabilityMatrix.md` and `RuleComplianceChecklist.md` must contain same-day closeout/sign-off updates.
5. `ValidationReport.md` must be regenerated from `scripts/verify-w5.ps1` and attached to release candidate evidence.

## Gate Rerun Order (A -> D)

1. Gate A: `scripts/verify-gate-a.ps1`
2. Gate B: `scripts/verify-gate-b.ps1`
3. Gate C: `scripts/verify-performance-nodes.ps1` then `scripts/verify-gate-c.ps1`
4. Gate D: `scripts/verify-w5.ps1`

## Upgrade Guide Requirements

1. Document any public API behavior changes in changelog.
2. Document performance-policy changes (unsafe/SIMD/parallel gating).
3. Include migration snippets for changed high-level wrappers in `ApiSurface.cs`.

## Release Artifacts

1. Validation reports in `artifacts/**`.
2. Updated `TraceabilityMatrix.md` closeout row for release candidate.
3. Updated `RuleComplianceChecklist.md` sign-off state.
