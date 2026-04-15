# Batch-3 Algorithm Comparison Baseline

- Date: 2026-04-10
- Scope: F102, F105, F203
- Purpose: Support Rule 11 and Rule 31 with explicit algorithm-selection evidence

## Selection Criteria

| Dimension | Weight | Acceptance Rule |
|---|---:|---|
| Correctness | 40% | Unit tests pass with deterministic tolerances |
| Robustness | 25% | Handles edge cases (empty vectors, shape checks, finite-difference step scaling) |
| Runtime cost | 25% | Predictable baseline latency and allocation behavior |
| Maintainability | 10% | Span-first API and minimal implementation complexity |

## Comparison Matrix

| Feature | Candidates | Chosen Baseline | Data/Evidence | Decision Rationale |
|---|---|---|---|---|
| F102 Vector Norms | direct loop accumulation, pairwise reduction tree, deferred SIMD path | direct loop accumulation | tests/LinalgCore/NormsTests.cs; complexity O(n); zero extra allocation by design | Best baseline for deterministic behavior and low maintenance before SIMD tuning |
| F105 Transpose/Hermitian Adjoint | direct row-major transpose, blocked transpose, in-place transpose variants | direct row-major transpose + conjugate transpose | tests/LinalgCore/TransposeTests.cs; explicit shape checks in src/LinalgCore/Transpose.cs | Correct and clear baseline with minimal API complexity; blocked path deferred for perf phase |
| F203 Jacobian Estimator | forward difference, central difference, secant/Broyden approximation | forward difference with scaled epsilon | tests/OdeCore/JacobianEstimatorTests.cs; src/OdeCore/JacobianEstimator.cs | Requires one function evaluation per column step and stable baseline behavior for ODE integration |

## Performance Baseline Linkage

- Batch-3 runtime baseline: artifacts/perf/2026-04-10-batch3.md
- Regression harness: scripts/verify-perf-regression.ps1

## Conclusion

- Rule 11: baseline algorithm choices are documented with explicit rationale.
- Rule 31: algorithm selection is supported by structured criteria, test evidence, and benchmark linkage.
