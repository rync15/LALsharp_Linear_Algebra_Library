# Stage-2 Micro-kernel Deep Dive (DataStructureCompatibility Strategy-Gated)

Date: 2026-04-13

## Scope
- Target modules:
  - LinalgCore: Gemm, Gemv
  - TensorCore: Convolution (1D/2D/ND)
- Policy control entry: DataStructureCompatibility performance settings
- Strategy dimensions: unsafe, SIMD, intrinsics, multithreading

## Implemented changes

### 1) GEMM deep dive
- File: src/LinalgCore/Gemm.cs
- Added transposed-B micro-kernel pipeline for real/complex GEMM:
  - Convert B (k x n) into contiguous column vectors once per call.
  - Compute each C(i,j) via contiguous dot kernels.
- Real kernels now route through shared PerformancePrimitives.Dot:
  - unsafe + AVX/FMA path
  - SIMD fallback
  - scalar fallback
- Parallel gating and worker cap now follow DataStructureCompatibility settings.

### 2) GEMV deep dive
- File: src/LinalgCore/Gemv.cs
- Row dot kernel now routes through PerformancePrimitives.Dot for real paths.
- Parallel gating and worker cap integrated with DataStructureCompatibility settings.
- Array overload routes to span overload for unified optimization path.

### 3) Convolution deep dive
- File: src/TensorCore/Convolution.cs
- 1D/2D/ND parallel gate now also honors DataStructureCompatibility global settings.
- 1D SIMD gate now follows DataStructureCompatibility EnableSimd + threshold policy.
- 1D parallel worker scheduling uses global strategy max degree.
- 2D/ND parallel options now combine Convolution-local settings and global strategy limits.

### 4) Strategy profile updates
- File: src/Core/DataStructureCompatibility.Performance.cs
- Updated module profiles to reflect stage-2 status:
  - LinalgCore/Gemm: full strategy-enabled
  - LinalgCore/Gemv: full strategy-enabled
  - TensorCore/Convolution: full strategy-enabled

### 5) Validation tests
- Files:
  - tests/LinalgCore/GemmTests.cs
  - tests/LinalgCore/GemvTests.cs
  - tests/TensorCore/ConvolutionTests.cs
- Added strategy-toggle consistency tests (scalar-only vs accelerated settings) to verify numerical equivalence.

## Build and regression
- `dotnet build LAL.sln -c Release`: PASSED
- `dotnet test LAL.Tests/LAL.Tests.csproj -c Release`: PASSED
- Test summary: 217 passed, 0 failed

## Benchmark smoke evidence

### GEMM
- Report: BenchmarkDotNet.Artifacts/results/LAL.Benches.GemmBenchmarks-report-github.md
- GemmMultiply (Size=64): 114.1 us, Alloc 824 B
- GemmMultiply (Size=128): 280.0 us, Alloc 5480 B

### GEMV
- Report: BenchmarkDotNet.Artifacts/results/LAL.Benches.GemvBenchmarks-report-github.md
- GemvMultiply (Rows=256, Cols=256): 61.96 us, Alloc 3.79 KB
- GemvMultiply (Rows=1024, Cols=256): 248.43 us, Alloc 3.66 KB

### Convolution (adaptive gate)
- Reports:
  - BenchmarkDotNet.Artifacts/results/LAL.Benches.Convolution2DThresholdCalibrationBenchmarks-report-github.md
  - BenchmarkDotNet.Artifacts/results/LAL.Benches.ConvolutionNdThresholdCalibrationBenchmarks-report-github.md
- Convolve2D_AutoAdaptiveGate:
  - 48x48, k=5: 42.65 us
  - 64x64, k=9: 230.72 us
  - 128x128, k=9: 924.92 us
- ConvolveND_AutoAdaptiveGate:
  - edge=10, k=3: 283.8 us
  - edge=12, k=5: 2222.1 us
  - edge=16, k=5: 5240.1 us

## Notes
- This stage keeps public APIs stable and introduces strategy-gated deep-kernel paths.
- Convolution’s legacy local gate (ConvolutionParallelSettings) remains active; global DataStructureCompatibility policy is now an additional controlling layer.

## Follow-up rerun (per-thread pooled scratch)

Date: 2026-04-14

### What changed
- Gemm `double[]` overload now uses thread-local pooled scratch for:
  - transposed-B columns workspace
  - per-row result scratch in parallel loop
- Gemv `double[]` overload now uses thread-local pooled scratch for parallel local-X workspace.

### Re-run commands (same BenchmarkDotNet entry/filter)
- `dotnet run -c Release --project Benchmarks/LAL.BenchmarkDotNet/LAL.BenchmarkDotNet.csproj -- --filter "*GemmBenchmarks.GemmMultiply*"`
- `dotnet run -c Release --project Benchmarks/LAL.BenchmarkDotNet/LAL.BenchmarkDotNet.csproj -- --filter "*GemvBenchmarks.GemvMultiply*"`

### Before vs After (same workload keys)

| Benchmark | Before Mean | After Mean | Mean Delta | Before Alloc | After Alloc | Alloc Delta |
|---|---:|---:|---:|---:|---:|---:|
| GemmMultiply (Size=64) | 114.1 us | 114.5 us | +0.4 us (+0.35%) | 824 B | 488 B | -336 B (-40.78%) |
| GemmMultiply (Size=128) | 280.0 us | 268.4 us | -11.6 us (-4.14%) | 5480 B | 5152 B | -328 B (-5.99%) |
| GemvMultiply (Rows=256, Cols=256) | 61.96 us | 37.28 us | -24.68 us (-39.83%) | 3.79 KB | 3.64 KB | -0.15 KB (-3.96%) |
| GemvMultiply (Rows=1024, Cols=256) | 248.43 us | 69.09 us | -179.34 us (-72.19%) | 3.66 KB | 3.84 KB | +0.18 KB (+4.92%) |

### Result summary
- Gemm: allocation reduced on both sizes; latency stayed flat on Size=64 and improved on Size=128.
- Gemv: latency improved significantly on both workloads; allocation improved on 256x256 and slightly regressed on 1024x256.
- Net: per-thread pooled scratch objective is largely achieved; remaining micro-target is reducing Gemv 1024x256 allocation while preserving current latency.
