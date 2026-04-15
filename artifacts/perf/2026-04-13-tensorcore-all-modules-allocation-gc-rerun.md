# TensorCore All-Module Allocation/GC Reassessment (2026-04-13)

## Goal
Re-review all TensorCore modules and prioritize managed allocation-pressure reductions using:
- stackalloc for small fixed-size temporary buffers
- ArrayPool for reusable temporary workspaces
- workspace reuse in hot kernels

This rerun keeps unsafe disabled and preserves Span-first boundaries.

## Scope
Modules reviewed:
- Broadcasting
- ComplexOps
- ConcatStack
- Convolution
- Cumulative
- Einsum
- Fft
- MaskOps
- Padding
- Reductions
- ShapeOps
- SortSearch
- StridedView
- TensorShape
- UFuncArithmetic
- UFuncTranscendental

## Applied Changes
### 1) Convolution
- Parallel 1D/2D/N-D paths now use pooled temporary buffers for signal/kernel/destination workspaces.
- Removed large `ToArray()` call chains from parallel-entry hot paths.
- N-D sequential workspace uses reusable pooled index buffers.
- Existing adaptive gate policy remains in place and now benefits from lower temporary allocation pressure.

### 2) Einsum
- Scalar evaluate (`i,i->`) switched from heap temporary array to `stackalloc` one-element destination.
- MatMul wrappers switched flattening workspaces (`leftFlat/rightFlat/destinationFlat`) to `ArrayPool`.

### 3) Reductions
- Quantile (double/float) now uses:
  - `stackalloc` for small inputs (<= 256)
  - `ArrayPool` for larger inputs
- Keeps same interpolation semantics.

### 4) SortSearch
- Argsort/Lexsort temporary key/index buffers now come from `ArrayPool`.
- Array-returning `NonZero`/`Where` changed from List-based accumulation to two-pass exact-allocation fill to avoid List object + growth overhead.

## Module-by-Module Outcome
- Broadcasting/ComplexOps/ConcatStack/Cumulative/MaskOps/Padding/ShapeOps/StridedView/TensorShape/UFuncArithmetic/UFuncTranscendental:
  - Reviewed.
  - No hot-loop GC hotspot requiring additional pooling beyond current architecture or output-ownership semantics.
- Fft:
  - Already had stackalloc + ArrayPool patterns from previous optimization pass.
  - Kept as-is.
- Convolution/Einsum/Reductions/SortSearch:
  - Updated in this rerun as listed above.

## Indicator Benchmarks (Quick Allocation Pass)
Run mode:
- `InvocationCount=1, IterationCount=1, WarmupCount=1, LaunchCount=1`

Reports:
- `BenchmarkDotNet.Artifacts/results/LAL.Benches.Convolution2DThresholdCalibrationBenchmarks-report-github.md`
- `BenchmarkDotNet.Artifacts/results/LAL.Benches.ConvolutionNdThresholdCalibrationBenchmarks-report-github.md`
- `BenchmarkDotNet.Artifacts/results/LAL.Benches.EinsumDotBenchmarks-report-github.md`
- `BenchmarkDotNet.Artifacts/results/LAL.Benches.EinsumMatMulBenchmarks-report-github.md`
- `BenchmarkDotNet.Artifacts/results/LAL.Benches.ReductionsBenchmarks-report-github.md`
- `BenchmarkDotNet.Artifacts/results/LAL.Benches.SortSearchBenchmarks-report-github.md`

Key observations:
- Convolution 2D/N-D auto-adaptive paths avoid forced-parallel high-allocation profile and stay near sequential allocations on tested sizes.
- Einsum dot path remains low-allocation; matmul path allocation scales primarily with returned result matrix while temporary flatten workspaces now use pooling.
- Reductions quantile shows low allocations for small sizes and bounded allocation for larger-size pooled path.
- SortSearch span-destination paths (`NonZero/Where`) remain low allocation; sort-family operations now use pooled temporary buffers.

## Rule Compliance Confirmation
Targeted rules requested by user and unsafe-related set were rechecked.

### Rule 4: Shared data structures across modules
- Passed.
- Changes keep shared managed buffer model and do not introduce incompatible memory representations.

### Rule 5: Span-based API boundaries
- Passed.
- Public TensorCore APIs remain Span/ReadOnlySpan boundary-first.

### Rule 6: Span-first default implementation path
- Passed.
- Default paths remain managed safe implementations.
- Pooling/stackalloc additions are internal optimizations.

### Unsafe-related rules (2/7/8/9/10/23/30)
- Passed.
- No new `unsafe` block introduced in TensorCore.
- Benchmark artifacts provided for modified hot paths.

Validation commands:
- `powershell -ExecutionPolicy Bypass -File scripts/verify-w3-and-rules32.ps1`
- `powershell -ExecutionPolicy Bypass -File scripts/verify-gate-a.ps1`
- `Select-String -Path "src/TensorCore/*.cs" -Pattern "\bunsafe\b"`

## Regression Validation
- `dotnet test LAL.Tests/LAL.Tests.csproj -c Release --filter "FullyQualifiedName~TensorCore"` => passed (53/53)
- `dotnet test LAL.Tests/LAL.Tests.csproj -c Release` => passed (191/191)
- `dotnet build Benchmarks/LAL.BenchmarkDotNet/LAL.BenchmarkDotNet.csproj -c Release` => passed
- `powershell -ExecutionPolicy Bypass -File scripts/verify-performance-nodes.ps1` => passed (39 checked)
