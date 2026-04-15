# Benchmarks

This folder hosts W4 benchmark assets.

## Components

- LAL.BenchmarkDotNet: BenchmarkDotNet suite for size/type/thread dimensions.
- LAL.Benchmarks (repo root project): fast baseline report generator used by CI perf guard.
- benches/*: traceability Performance Node benchmark classes referenced from TraceabilityMatrix.

## Run BenchmarkDotNet Suite

```powershell
dotnet run --project Benchmarks/LAL.BenchmarkDotNet/LAL.BenchmarkDotNet.csproj -c Release -- --filter *
```

## Run CI Baseline Generator

```powershell
dotnet run --project LAL.Benchmarks/LAL.Benchmarks.csproj -c Release
```

## Verify Performance Node Coverage

```powershell
powershell -ExecutionPolicy Bypass -File scripts/verify-performance-nodes.ps1
```

## Execute Performance Node Benchmarks Only

```powershell
dotnet run --project Benchmarks/LAL.BenchmarkDotNet/LAL.BenchmarkDotNet.csproj -c Release -- --filter *Placeholder*
```
