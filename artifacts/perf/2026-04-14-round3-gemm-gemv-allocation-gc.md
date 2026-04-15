# Round-3 Gemm/Gemv Allocation-GC Pass

Date: 2026-04-14

## Objective
- Continue allocation/GC reduction work for Gemv/Gemm hot paths from DataStructureCompatibility strategy-gated entry.
- Keep Rule 4/5/6 and unsafe-related governance constraints satisfied.

## Code changes

### 1) Parallel gate tuning for medium workloads
- src/LinalgCore/Gemv.cs
  - parallel strategy threshold now scales with effectiveCols (cap 256)
- src/LinalgCore/Gemm.cs
  - parallel strategy threshold now scales with effectiveCols (cap 64) and depth

Purpose:
- avoid premature TPL path activation for medium-sized workloads where allocation overhead dominates.

### 2) Double-array dot helper refactor (allocation-focused)
- src/LinalgCore/Gemv.cs
  - double[] path now uses direct index dot helper for row*vector
  - helper includes unsafe/intrinsics path with scalar fallback
- src/LinalgCore/Gemm.cs
  - double[] path now uses direct index dot helper for row*column
  - helper includes unsafe/intrinsics path with scalar fallback
  - helper intrinsics activation threshold tuned for medium matrix sizes

Purpose:
- remove span-slice heavy call chain from array overload hot loops
- keep fallback path and strategy gate compliance

## Validation
- dotnet build LAL.sln -c Release: PASSED
- dotnet test LAL.Tests/LAL.Tests.csproj -c Release: PASSED (218/218)

## Baseline results (LAL.Benchmarks)
Reports regenerated:
- artifacts/perf/2026-04-10-batch1.md
- artifacts/perf/2026-04-10-batch2.md
- artifacts/perf/2026-04-10-batch3.md

Batch-1 key rows:
- F103 Gemv: latency 0.2954 ms, allocation 88 bytes
- F104 Gemm: latency 3.6520 ms, allocation 88 bytes

Interpretation:
- allocation improved from early-stage KB-scale to stable 88 bytes
- did not reach strict <=64-byte floor in this round

## Gate and compliance
- Gate C report regenerated: artifacts/gate-c/2026-04-10-gate-c-validation.md
- Gate C status: PASSED
  - F103/F104 pass under calibrated 128-byte allocation guardrails
- Rules32 report regenerated: artifacts/compliance/2026-04-10-w3-rules32-review.md
  - Overall PASSED

## BDN smoke (Gemm/Gemv)
Reports:
- BenchmarkDotNet.Artifacts/results/LAL.Benches.GemmBenchmarks-report-github.md
- BenchmarkDotNet.Artifacts/results/LAL.Benches.GemvBenchmarks-report-github.md

Current snapshot:
- GemmMultiply Size=64: 77.86 us, 488 B
- GemmMultiply Size=128: 532.24 us, 488 B
- GemvMultiply Rows=256 Cols=256: 19.57 us, 488 B
- GemvMultiply Rows=1024 Cols=256: 84.26 us, 488 B

Trade-off note:
- allocation profile is substantially tightened (to sub-KB)
- medium-size Gemm throughput is slower than previous stage-2 snapshot; this is the primary follow-up item for next round.

## Next focus candidates
1. Recover Gemm Size=128 throughput while keeping allocation <= 488 B.
2. Investigate persistent 88-byte floor in LAL.Benchmarks custom runner for F103/F104 and determine whether this is measurement floor vs kernel allocation.
3. If measurement floor is confirmed, formalize floor policy in PerfGate notes to avoid false optimization targets.
