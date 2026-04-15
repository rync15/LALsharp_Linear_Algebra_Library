# Unsafe Rationale (W4)

- Date: 2026-04-10
- Status: Gate C baseline reached with safe kernels

## 1. Current Decision

Unsafe broad enablement remains intentionally off for the current baseline.
Selective managed SIMD/parallel tuning is enabled on approved hot paths with guarded thresholds and safe fallbacks.

## 2. Why

1. Current hot paths are within defined baseline thresholds in PerfGate.
2. Managed SIMD via Vector<T> improves throughput without expanding unsafe surface area.
3. CI already enforces benchmark generation and regression checks.
4. No validated unsafe bottleneck package with before/after evidence has been approved yet.

## 3. What Is Allowed Today

- Safe kernels (`Span<T>`/`ReadOnlySpan<T>`) remain default.
- Managed SIMD (`System.Numerics.Vector<T>`) is allowed in hot kernels when guarded by hardware checks and scalar fallback.
- Parallel execution is allowed only with explicit size thresholds and deterministic fallback path.
- Any unsafe proposal must include:
  - bottleneck proof
  - before/after benchmark report
  - correctness parity tests
  - fallback implementation

## 4. Trigger To Revisit

Revisit this decision when one of the following occurs:

1. Hot path fails PerfGate under production-like size profiles.
2. Throughput target cannot be reached with safe path.
3. A contained unsafe optimization demonstrates stable gain and no correctness regression.

## 5. Re-evaluation Summary (All Modules)

1. TensorCore: SIMD introduced for elementwise arithmetic; unsafe/intrinsics deferred.
2. LinalgCore: SIMD introduced for Axpy/Dot/Gemv row dot, plus gated parallel Gemv/Gemm.
3. OdeCore: Jacobian estimator now supports opt-in parallel path for large dimensions.
4. NumericalCore: Covariance/Correlation now supports SIMD mean/sum and opt-in parallel path.

## 6. Evidence Links

- Perf gate definition: PerfGate.md
- CI perf guard: .github/workflows/ci.yml
- Perf threshold script: scripts/verify-perf-regression.ps1
- Gate A unsafe governance evidence: artifacts/gate-a/r02.md, artifacts/gate-a/r07.md, artifacts/gate-a/r10.md, artifacts/gate-a/r30.md
