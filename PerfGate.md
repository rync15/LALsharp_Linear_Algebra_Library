# Perf Gate (W4)

- Project: LAL
- Target Runtime: .NET 8 x64 (Windows/Linux)
- Gate: C (W4 Performance Engineering and Unsafe Enablement)
- Date: 2026-04-10

## 1. Scope

This gate validates hot-path latency and allocation behavior before any unsafe rollout, and governs selective managed SIMD/parallel strategy adoption.

## 2. Input Reports

- artifacts/perf/2026-04-10-batch1.md
- artifacts/perf/2026-04-10-batch2.md
- artifacts/perf/2026-04-10-batch3.md
- artifacts/perf/perf-node-audit.md

## 3. Threshold Table

| Report | Function | Max Latency (ms) | Max Allocation Increase (bytes) |
|---|---|---:|---:|
| batch1 | F100 Axpy | 3.00 | 64 |
| batch1 | F101 Dotu | 3.50 | 64 |
| batch1 | F103 Gemv | 2.50 | 128 |
| batch1 | F104 Gemm | 35.00 | 128 |
| batch2 | F011 Reductions.Sum | 6.00 | 128 |
| batch2 | F202 RK45.Step | 0.03 | 640 |
| batch2 | F302 Brent.Solve | 0.01 | 64 |
| batch2 | F306 GradientDescent.SolveScalar | 0.01 | 64 |
| batch3 | F102 Norms.L2 | 2.00 | 64 |
| batch3 | F105 Transpose.Matrix | 1.50 | 64 |
| batch3 | F203 JacobianEstimator.EstimateForwardDifference | 0.02 | 512 |

## 4. Enforcement

1. Generate benchmark reports.
2. Compare with base-branch baseline in CI.
3. Fail when latency regression exceeds threshold percentage.
4. Fail when allocation increase exceeds threshold bytes.
5. Fail when required functions are missing from report.
6. Fail when any traceability Performance Node path is missing.

Current policy in CI:

- Latency regression threshold: 10%
- Allocation increase threshold: 64 to 128 bytes by report group
- Performance node path audit command: `powershell -ExecutionPolicy Bypass -File scripts/verify-performance-nodes.ps1`
- Performance node execution smoke test: `dotnet run --project Benchmarks/LAL.BenchmarkDotNet/LAL.BenchmarkDotNet.csproj -c Release -- --filter *AxpyBenchmarks.AxpyDouble*`

Module-level strategy update (2026-04-10 re-evaluation):

1. TensorCore: managed SIMD enabled for elementwise arithmetic; parallel path remains deferred for reduction/convolution race-risk and allocation tradeoff.
2. LinalgCore: managed SIMD enabled for Axpy/Dot/Gemv row dot; gated parallel row scheduling enabled for Gemv/Gemm on large workloads.
3. OdeCore: Jacobian estimator adds opt-in parallel mode for sufficiently large dimensions.
4. NumericalCore: covariance/correlation adds managed SIMD sum path and opt-in parallel mode for large datasets.
5. Unsafe remains disabled by default across modules until benchmark evidence plus correctness parity are approved.

## 5. Unsafe Entry Rule

Unsafe or intrinsics work can be merged only when all are true:

1. Perf regression checks are green.
2. Before/after benchmark evidence exists in artifacts/perf.
3. Correctness regression tests are green.
4. Safe fallback path remains available.
