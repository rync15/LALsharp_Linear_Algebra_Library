# All-Module Allocation and GC Pressure Reassessment

- Date: 2026-04-10
- Scope: TensorCore, LinalgCore, OdeCore, NumericalCore
- Objective: reduce transient allocations and GC pressure by prioritizing stackalloc, ArrayPool, and workspace reuse.

## Summary

- Overall result: PASSED
- Policy alignment:
  - Rule 4 (shared data structures): preserved Span-first data flow and shared workspace style across modules.
  - Rule 5 (Span-based API boundaries): public signatures remain Span/ReadOnlySpan.
  - Rule 6 (Span-first default path): default path remains safe managed implementation with optional pooled workspace.
  - Unsafe-related rules (7/9/10/23/30): no new unsafe blocks introduced; optimization remained managed and benchmark-gated.

## Module-by-Module Actions

| Module | Hot path reviewed | Previous pressure | Change | Expected impact |
|---|---|---|---|---|
| TensorCore | Fft.Rfft | per-call Complex[] allocation | stackalloc for small vectors, ArrayPool for larger vectors | lower Gen0 churn in repeated FFT setup |
| LinalgCore | Gemv/Gemm parallel path | ToArray allocations for A/B/X/Y/C on each call | replace with pooled buffers and explicit copy-in/copy-out | reduced transient array allocations in parallel kernels |
| OdeCore | Rk4/Rk45/JacobianEstimator | multiple stage arrays + per-column clones | single workspace slices (stackalloc/ArrayPool), pooled perturbed buffers | lower allocation rate during step-by-step integration |
| NumericalCore | Covariance parallel path | x.ToArray/y.ToArray per call | pooled parallel input buffers | reduced allocations in large-sample covariance/correlation runs |

## Source Evidence

- src/TensorCore/Fft.cs
- src/LinalgCore/Gemv.cs
- src/LinalgCore/Gemm.cs
- src/OdeCore/Rk4.cs
- src/OdeCore/Rk45.cs
- src/OdeCore/JacobianEstimator.cs
- src/NumericalCore/Statistics/Covariance.cs

## Validation

- Rules compliance confirmation: artifacts/compliance/2026-04-10-w3-rules32-review.md
- Gate sequence reports:
  - Gate A: artifacts/gate-a/*
  - Gate B: artifacts/gate-b/2026-04-10-gate-b-validation.md
  - Gate C: artifacts/gate-c/2026-04-10-gate-c-validation.md
  - Gate D: artifacts/w5/2026-04-10-w5-validation.md
