# NumericalCore Allocation and GC Reassessment

- Date: 2026-04-10
- Scope: src/NumericalCore/Statistics/Covariance.cs, src/NumericalCore/Interpolation/Rbf.cs, src/NumericalCore/Interpolation/Spline.cs
- Objective: reduce allocation and GC pressure with stackalloc, ArrayPool, and workspace reuse while preserving safe defaults.

## Code Changes

1. Covariance correlation path refactor:
   - Correlation for double/float/Complex no longer calls Compute three times.
   - Added one-pass centered-sum accumulation for covariance and variances.
   - Parallel correlation now rents/copies work buffers once per call and reuses them.
2. RBF float path refactor:
   - Replaced per-call new double[] conversion buffers with stackalloc (small) or ArrayPool (large).
   - Added explicit return of rented buffers in finally blocks.
3. Spline temporary workspace refactor:
   - Replaced new u arrays with stackalloc or ArrayPool workspace.

## Rule Compliance Checks

- Rules matrix check: powershell -ExecutionPolicy Bypass -File scripts/verify-w3-and-rules32.ps1
  - Overall status: PASSED
  - Rules parsed: 32, Passed: 32, In Review: 0, Pending: 0, Blocked: 0
- Gate A safety and evidence check: powershell -ExecutionPolicy Bypass -File scripts/verify-gate-a.ps1
  - Result: passed (r01-r32 evidence files and checklist columns validated)

Targeted confirmation for requested rule groups:

1. Rule 4 (shared data structures): no incompatible model introduced; changes remain in existing span and pooled-buffer model.
2. Rule 5 (span-based boundaries): public APIs remain span-based, no array-only boundary regression.
3. Rule 6 (span-first default): default code path is still safe managed code; unsafe is not required for default execution.
4. Unsafe-related rules (2/7/9/10/23/30):
   - No new unsafe blocks or pointer-based public APIs.
   - Optimizations remain managed SIMD/FMA plus gated parallelism.
   - Correctness and regression checks remain green.

## Validation

- dotnet test LAL.sln
  - Passed: 147, Failed: 0

## Performance Indicators (Allocation and GC)

Baseline source used for comparison:
- artifacts/perf/2026-04-10-numericalcore-strategy-reassessment.md

Current benchmark reports:
- BenchmarkDotNet.Artifacts/results/LAL.Benches.StatisticsBenchmarks-report-github.md
- BenchmarkDotNet.Artifacts/results/LAL.Benches.InterpolationBenchmarks-report-github.md

### Key before/after deltas

| Metric | Baseline | Current | Delta |
|---|---:|---:|---:|
| CorrelationDoubleParallel (Size=65536) Mean | 895.017 us | 286.459 us | -68.00% |
| CorrelationDoubleParallel (Size=65536) Allocated | 32499 B | 10786 B | -66.81% |
| CorrelationDoubleParallel (Size=65536) Gen0 | 2.9297 | 1.2207 | -58.33% |
| RbfWeightsParallel (Size=128) Mean | 218.67 us | 208.01 us | -4.87% |
| RbfWeightsParallel (Size=128) Allocated | 3231 B | 3193 B | -1.18% |
| RbfWeightsComplexParallel (Size=128) Mean | 351.10 us | 341.57 us | -2.71% |
| RbfWeightsComplexParallel (Size=128) Allocated | 2921 B | 2811 B | -3.77% |

### New float-path indicators (no prior baseline in report)

| Metric | Current |
|---|---:|
| RbfWeightsFloatSequential (Size=128) Mean | 154.54 us |
| RbfWeightsFloatSequential (Size=128) Allocated | 80 B |
| RbfWeightsFloatParallel (Size=128) Mean | 119.46 us |
| RbfWeightsFloatParallel (Size=128) Allocated | 3285 B |

## Conclusion

The reassessment reduced allocation and GC pressure in the main NumericalCore hot paths requested by this task while preserving rule compliance and safe defaults.
