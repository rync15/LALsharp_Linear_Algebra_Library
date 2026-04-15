# Batch-4 Algorithm Comparison Baseline

- Date: 2026-04-10
- Scope: F106, F301
- Purpose: extend Rule 11 and Rule 31 evidence for Batch-4 startup

## Selection Criteria

| Dimension | Weight | Acceptance Rule |
|---|---:|---|
| Correctness and determinism | 40% | Unit tests pass and numerical behavior is stable |
| Robustness | 25% | Handles singular/degenerate scenarios explicitly |
| Runtime profile | 25% | Baseline implementation with predictable complexity |
| Maintainability | 10% | Clear span-first API and minimal hidden behavior |

## Comparison Matrix

| Feature | Candidate Set | Selected Baseline | Evidence | Decision Rationale |
|---|---|---|---|---|
| F106 LU decomposition | partial pivoting LU, full pivoting LU, QR fallback | partial pivoting LU baseline | src/LinalgCore/Lu.cs, tests/LinalgCore/LuTests.cs | Partial pivoting provides strong numerical robustness at lower implementation complexity for baseline phase |
| F301 Secant root finding | secant, Newton-only, Brent-only | secant baseline | src/NumericalCore/RootFinding/Secant.cs, tests/NumericalCore/RootFinding/SecantTests.cs | Secant offers derivative-free iterative root finding with lower API coupling than Newton baseline |

## Conclusion

- Batch-4 startup decisions are documented with explicit alternatives and rationale.
- Evidence is sufficient for Batch-4 In Review stage prior to deeper perf tuning.
