# Module Kernel Strategy Reassessment (W4 Rerun)

- Date: 2026-04-10
- Scope: TensorCore, LinalgCore, OdeCore, NumericalCore
- Goal: Re-evaluate and revise unsafe/SIMD/intrinsics/parallel strategy across all modules

## Decision Matrix

| Module | Unsafe | SIMD / Intrinsics | Parallel | Applied Revisions |
|---|---|---|---|---|
| TensorCore | Keep disabled | Enabled managed SIMD via `System.Numerics.Vector<T>` for elementwise arithmetic | Deferred by default for reduction/convolution hot loops | `src/TensorCore/UFuncArithmetic.cs` |
| LinalgCore | Keep disabled | Enabled managed SIMD for Axpy, Dot, Gemv row-kernel | Enabled gated row-level parallel for Gemv and Gemm | `src/LinalgCore/Axpy.cs`, `src/LinalgCore/Dot.cs`, `src/LinalgCore/Gemv.cs`, `src/LinalgCore/Gemm.cs` |
| OdeCore | Keep disabled | No explicit SIMD introduced (limited gain for current kernel shape) | Added opt-in parallel mode for Jacobian estimation on large systems | `src/OdeCore/JacobianEstimator.cs` |
| NumericalCore | Keep disabled | Enabled managed SIMD summation in covariance pipeline | Added opt-in parallel mode for large covariance/correlation workloads | `src/NumericalCore/Statistics/Covariance.cs` |

## Governance Alignment Updates

- `PerfGate.md` updated with module-level strategy and corrected smoke benchmark command.
- `UnsafeRationale.md` updated to reflect selective managed SIMD/parallel rollout with unsafe still gated.
- `plan.md` updated with module-specific parallel policy under W4 section.

## Validation Commands

- `dotnet test LAL.sln`
- `powershell -ExecutionPolicy Bypass -File scripts/verify-w3.ps1`
- `powershell -ExecutionPolicy Bypass -File scripts/verify-performance-nodes.ps1`
- `powershell -ExecutionPolicy Bypass -File scripts/verify-gate-c.ps1`
- `dotnet build Benchmarks/LAL.BenchmarkDotNet/LAL.BenchmarkDotNet.csproj -c Release`

## Validation Results

1. Regression: Passed (74/74) via `dotnet test LAL.sln`.
2. W3 coverage: Passed via `scripts/verify-w3.ps1` (report regenerated).
3. W4 perf-node audit: Passed (39 checked, 0 missing).
4. Gate C: Passed via `scripts/verify-gate-c.ps1`.
5. Benchmark harness build: Passed for `Benchmarks/LAL.BenchmarkDotNet` Release build.
6. Unsafe expansion: None introduced in this reassessment.
