# TensorCore Convolution Threshold Calibration (2026-04-13)

## Scope
- Added externally configurable 2D/N-D parallel gate settings in TensorCore Convolution.
- Added adaptive gate policy based on workload size and CPU core budget.
- Added BenchmarkDotNet calibration suites for 2D and N-D convolution gate profiles.

## Configuration Surface
- Public settings type: `ConvolutionParallelSettings`.
- Public APIs:
  - `Convolution.GetParallelSettings()`
  - `Convolution.SetParallelSettings(...)`
  - `Convolution.ResetParallelSettings()`
- API wrapper exposure:
  - `TensorApi.GetConvolutionParallelSettings()`
  - `TensorApi.SetConvolutionParallelSettings(...)`
  - `TensorApi.ResetConvolutionParallelSettings()`

## Adaptive Gate Policy
- 2D/N-D both support:
  - base thresholds (output and op estimate)
  - min thresholds (lower clamp when scaling down)
  - large-workload hints
  - CPU-budget-aware scaling
- Policy shape:
  - Small/medium workloads: raise thresholds by CPU-aware factor to avoid thread and allocation overhead.
  - Large workloads: reduce thresholds by CPU-aware factor to allow parallel entry.

## Calibration Benchmarks
- 2D class: `Convolution2DThresholdCalibrationBenchmarks`
- N-D class: `ConvolutionNdThresholdCalibrationBenchmarks`
- Run command used for quick gate calibration:

```powershell
dotnet run --project Benchmarks/LAL.BenchmarkDotNet/LAL.BenchmarkDotNet.csproj -c Release -- --filter "*Convolution2DThresholdCalibrationBenchmarks*" --iterationCount 1 --warmupCount 1 --launchCount 1 --runOncePerIteration
dotnet run --project Benchmarks/LAL.BenchmarkDotNet/LAL.BenchmarkDotNet.csproj -c Release -- --filter "*ConvolutionNdThresholdCalibrationBenchmarks*" --iterationCount 1 --warmupCount 1 --launchCount 1 --runOncePerIteration
```

## Result Summary
Source reports:
- `BenchmarkDotNet.Artifacts/results/LAL.Benches.Convolution2DThresholdCalibrationBenchmarks-report-github.md`
- `BenchmarkDotNet.Artifacts/results/LAL.Benches.ConvolutionNdThresholdCalibrationBenchmarks-report-github.md`

Key observations from this hardware profile (i7-11700):
- Forced parallel (`threshold=1`) shows substantially higher allocation and worse latency for tested 2D/N-D ranges.
- Auto-adaptive profile avoids forced parallel regressions and tracks sequential profile closely.
- 2D auto profile allocation remained at baseline (400 B) across tested sizes, indicating conservative gating on these ranges.
- N-D auto profile allocation remained near baseline (mostly 720 B, occasional 432 B), and latency stayed near sequential.

## Calibration Decision
- Keep default strategy conservative for small/medium workloads to avoid overhead.
- Use size hints plus CPU scaling to unlock parallel mode only for larger workloads.
- Continue to expose all gate knobs externally so application-level tuning can specialize by deployment hardware and tensor distribution.

## Validation
- `dotnet test LAL.Tests/LAL.Tests.csproj -c Release --filter "FullyQualifiedName~ConvolutionTests"` => passed
- `dotnet test LAL.Tests/LAL.Tests.csproj -c Release --filter "FullyQualifiedName~TensorCore"` => passed
- `dotnet test LAL.Tests/LAL.Tests.csproj -c Release` => passed
- `dotnet build Benchmarks/LAL.BenchmarkDotNet/LAL.BenchmarkDotNet.csproj -c Release` => passed
