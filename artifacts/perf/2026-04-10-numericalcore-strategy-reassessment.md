# NumericalCore Strategy Reassessment

- Date: 2026-04-10
- Scope: `src/NumericalCore/Statistics/*`, `src/NumericalCore/Interpolation/*`
- Goal: evaluate unsafe/SIMD/intrinsics/parallel strategy impact after NumericalCore type-coverage expansion and hotspot optimization.

## Strategy Decisions

1. Unsafe: keep NumericalCore hot paths in managed code; no new unsafe blocks introduced.
2. SIMD/intrinsics:
   - Keep managed SIMD + FMA accumulation in covariance kernels.
   - Keep intrinsics portability under .NET JIT control (no architecture-specific hard wiring).
3. Parallelization:
   - Keep gated parallel covariance path for float/double/Complex when caller explicitly selects parallel entrypoints.
   - Keep gated parallel kernel construction for RBF interpolation and pooled in-place solve flow.
4. Allocation policy:
   - Keep `ArrayPool<T>` for RBF kernel/workspace buffers.
   - Keep stackalloc for small temporary vectors and avoid per-iteration heap churn in hot loops.

## Validation Commands

- `dotnet test LAL.sln` -> Passed (146/146).
- `dotnet run --project Benchmarks/LAL.BenchmarkDotNet/LAL.BenchmarkDotNet.csproj -c Release -- --filter "*StatisticsBenchmarks*"`
- `dotnet run --project Benchmarks/LAL.BenchmarkDotNet/LAL.BenchmarkDotNet.csproj -c Release -- --filter "*InterpolationBenchmarks*"`

## Benchmark Snapshot

Source reports:

- `BenchmarkDotNet.Artifacts/results/LAL.Benches.StatisticsBenchmarks-report-github.md`
- `BenchmarkDotNet.Artifacts/results/LAL.Benches.InterpolationBenchmarks-report-github.md`

| Area | Method | Size | Mean | Allocated |
|---|---|---:|---:|---:|
| Statistics | CovarianceDoubleSequential | 4096 | 2.986 us | 0 B |
| Statistics | CovarianceDoubleParallel | 4096 | 3.004 us | 0 B |
| Statistics | CovarianceComplexParallel | 4096 | 10.489 us | 0 B |
| Statistics | CorrelationDoubleParallel | 4096 | 8.944 us | 0 B |
| Statistics | CovarianceDoubleSequential | 65536 | 50.392 us | 0 B |
| Statistics | CovarianceDoubleParallel | 65536 | 295.973 us | 11181 B |
| Statistics | CovarianceComplexParallel | 65536 | 425.932 us | 11026 B |
| Statistics | CorrelationDoubleParallel | 65536 | 895.017 us | 32499 B |
| Interpolation | RbfWeightsSequential | 64 | 56.73 us | 50 B |
| Interpolation | RbfWeightsParallel | 64 | 57.87 us | 50 B |
| Interpolation | RbfWeightsComplexParallel | 64 | 87.95 us | 56 B |
| Interpolation | RbfWeightsSequential | 128 | 241.85 us | 80 B |
| Interpolation | RbfWeightsParallel | 128 | 218.67 us | 3231 B |
| Interpolation | RbfWeightsComplexParallel | 128 | 351.10 us | 2921 B |

## Assessment

1. Covariance parallel route is functionally correct but does not outperform sequential on this host for sampled sizes; thread/reduction overhead and allocation cost remain visible in benchmark output.
2. RBF parallel route shows positive gain at size 128 versus sequential while keeping bounded managed allocations through pooled buffers.
3. Complex paths remain fully covered and stable under the new optimization strategy.
4. Current strategy remains compliant with safety-first rules: measured optimization only, no blanket unsafe adoption, and full regression coverage maintained.