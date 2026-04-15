# P0 Benchmark Workload Realism Upgrade

- Date: 2026-04-10
- Scope: Replace placeholder benchmark nodes with executable workloads on key hot paths

## Updated Benchmark Nodes

- benches/LinalgCore/AxpyBenchmarks.cs
- benches/LinalgCore/DotBenchmarks.cs
- benches/LinalgCore/GemvBenchmarks.cs
- benches/LinalgCore/GemmBenchmarks.cs
- benches/LinalgCore/NormsBenchmarks.cs
- benches/LinalgCore/MatrixAnalysisBenchmarks.cs
- benches/OdeCore/Rk45Benchmarks.cs
- benches/NumericalCore/RootFindingBenchmarks.cs
- benches/NumericalCore/OptimizationBenchmarks.cs

## Validation Commands

- dotnet build Benchmarks/LAL.BenchmarkDotNet/LAL.BenchmarkDotNet.csproj -c Release
  - Passed
- dotnet test LAL.sln
  - Passed (72/72)
- powershell -ExecutionPolicy Bypass -File scripts/verify-performance-nodes.ps1
  - Passed (39 checked, 0 missing)
- dotnet run --project Benchmarks/LAL.BenchmarkDotNet/LAL.BenchmarkDotNet.csproj -c Release -- --filter *AxpyBenchmarks.AxpyDouble*
  - Passed, benchmark report exported

## Smoke Benchmark Evidence

- BenchmarkDotNet.Artifacts/results/LAL.Benches.AxpyBenchmarks-report-github.md
- BenchmarkDotNet.Artifacts/results/LAL.Benches.AxpyBenchmarks-report.csv
- BenchmarkDotNet.Artifacts/results/LAL.Benches.AxpyBenchmarks-report.html
