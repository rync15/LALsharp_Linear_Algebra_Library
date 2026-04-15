# Rule 21 Algorithm Comparison Baseline

- Date: 2026-04-10
- Rule: 21 (Implement highest-performance suitable algorithm per feature)
- Scope: Batch-1 and Batch-2 implemented P0/P1 kernels
- Runtime: .NET 8 (x64)

## Comparison Criteria

| Dimension | Weight | Pass Threshold |
|---|---:|---|
| Correctness and convergence | 40% | Deterministic tests pass with documented tolerance |
| Latency and allocation | 35% | No avoidable allocation regressions on hot paths |
| Robustness and fallback behavior | 15% | Convergence preserved for edge scenarios |
| API and implementation suitability | 10% | Span-first public API and maintainable baseline |

## Evidence Matrix

| Feature | Candidate Set | Selected Baseline | Evidence | Conclusion |
|---|---|---|---|---|
| F100/F101/F103/F104 (Linalg hot paths) | Safe loop baseline, future SIMD/unsafe path | Safe loop baseline | `artifacts/perf/2026-04-10-batch1.md`: Axpy 1.4618 ms, Dotu 1.1782 ms, Gemv 2.7582 ms, Gemm 17.1856 ms; allocation = 0 | Baseline is suitable and measurable; future optimization must beat this gate |
| F202 (ODE single-step) | Euler, RK4, RK45 | RK45 for adaptive scenarios | `tests/OdeCore/EulerTests.cs`, `tests/OdeCore/Rk4Tests.cs`, `tests/OdeCore/Rk45Tests.cs` all pass; RK45 exposes embedded error estimate (`Rk45StepResult.EstimatedError`) for step control | RK45 selected as highest-suitable default for adaptive accuracy/robustness tradeoff |
| F302 (Root finding with bracket) | Newton, Brent | Brent for bracketed root search | `tests/NumericalCore/RootFinding/BrentTests.cs` and `tests/NumericalCore/RootFinding/NewtonTests.cs` pass; Brent returns converged bracket-safe result with iteration record | Brent selected for bracketed mode due to stronger convergence safety than derivative-dependent Newton |
| F011 (Reductions) | Naive sum, compensated sum | Kahan compensated summation | `src/TensorCore/Reductions.cs` uses compensated update and `tests/TensorCore/ReductionsTests.cs` pass | Kahan baseline selected to balance precision stability and throughput |
| F306 (Optimization baseline) | Fixed-step GD variants, momentum family (backlog) | Vanilla gradient descent baseline | `tests/NumericalCore/Optimization/GradientDescentTests.cs` pass; selected as minimal stable baseline before advanced optimizers (L-BFGS backlog) | Baseline accepted; advanced methods remain planned with future perf comparison |

## Decision

- Rule 21 status recommendation: Passed
- Rationale: each implemented feature has explicit candidate comparison dimensions, measurable baseline evidence, and a documented reason for the selected algorithm.
