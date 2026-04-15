# OdeCore Allocation/GC Reassessment

- Date: 2026-04-11
- Scope: `src/OdeCore/*`
- Goal: reduce transient allocations and GC pressure with `stackalloc`, `ArrayPool<T>`, and workspace reuse while preserving Span-first safety constraints.

## Hotspot Review and Changes

1. Euler
- Prior pattern: large-size derivative buffer used heap array fallback.
- Change: switched to `stackalloc` for small sizes and `ArrayPool<T>` for large sizes in double/float/Complex overloads.
- File: `src/OdeCore/Euler.cs`

2. Bdf (Backward Euler)
- Prior pattern: per-call `new[]` for derivative workspace in all numeric overloads.
- Change: switched to `stackalloc` + `ArrayPool<T>` workspace policy for double/float/Complex.
- File: `src/OdeCore/Bdf.cs`

3. Radau
- Prior pattern: per-call `ToArray()` state clone and per-call derivative array in all numeric overloads.
- Change: replaced with single reusable workspace (`stage` + `dydt`) via `stackalloc`/`ArrayPool<T>` slicing for double/float/Complex.
- File: `src/OdeCore/Radau.cs`

4. Existing reusable paths kept
- `Rk4`, `Rk45`, `JacobianEstimator` already used workspace pooling/stackalloc strategies and were preserved.
- Files: `src/OdeCore/Rk4.cs`, `src/OdeCore/Rk45.cs`, `src/OdeCore/JacobianEstimator.cs`

## Indicators

- Static allocation scan for OdeCore temporary array patterns:
  - Pattern: `new <type>[...]` and `ToArray()` in `src/OdeCore/*.cs`
  - Result after refactor: no matches
- Unsafe scan for OdeCore:
  - Pattern: `unsafe` in `src/OdeCore/*.cs`
  - Result: no matches

## Rule Compliance Confirmation

Requested rules and unsafe-related checks:

- Rule 4 (shared data structures across modules): Passed
- Rule 5 (Span API boundaries): Passed
- Rule 6 (Span-first default path): Passed
- Unsafe-related rules (7/8/9/10/30): Passed

Evidence:
- `RuleComplianceChecklist.md`
- `artifacts/compliance/2026-04-10-w3-rules32-review.md` (Overall status: PASSED, 32/32 passed)

## Validation

- `dotnet test LAL.sln --configuration Release --filter "FullyQualifiedName~LAL.Tests.OdeCore"`
  - Passed: 26/26
- `dotnet test LAL.sln --configuration Release`
  - Passed: 162/162

## Safety Notes

- No public API pointer exposure introduced.
- No `unsafe` blocks added.
- Span-first boundary and managed fallback behavior preserved.
