# LALsharp 線性代數函式庫 (LAL)

LAL 是一個 source-first 的 C# numerical computing 專案，聚焦在線性代數、tensor operations、ODE 求解與常見 numerical methods。

目前專案以單一 solution [LAL.sln](LAL.sln) 為核心，對外入口集中在 [ApiSurface.cs](ApiSurface.cs) 的 `LAL.Api` wrapper APIs。

## 專案特色

- 採用 Span-first 設計：大量使用 `Span<T>` 與 `ReadOnlySpan<T>`，降低不必要配置並保持可預測記憶體行為。
- 統一資料結構：透過 `NDBuffer<T>`、shape/stride 與 compatibility helpers（見 [src/Core/DataStructureCompatibility.cs](src/Core/DataStructureCompatibility.cs)）。
- 對外 API 一致：
  - `LAL.Api.LinalgApi`
  - `LAL.Api.NumericalApi`
  - `LAL.Api.OdeApi`
  - `LAL.Api.TensorApi`
- 模組覆蓋完整：4 大核心領域、57 個模組（Linalg 19、Numerical 14、Ode 8、Tensor 16）。
- 內建效能治理：可透過 [src/Core/DataStructureCompatibility.Performance.cs](src/Core/DataStructureCompatibility.Performance.cs) 的 settings/profile API 做策略調整與稽核。

## 專案結構

- [LAL.Core](LAL.Core)：主函式庫專案，編譯 [src](src) 與 [ApiSurface.cs](ApiSurface.cs)。
- [LAL.Tests](LAL.Tests)：測試專案，會編譯 [tests](tests) 內測試。
- [LAL.Benchmarks](LAL.Benchmarks)：baseline benchmark 執行器與報告產生器。
- [src](src)：依領域分層實作（`Core`、`LinalgCore`、`NumericalCore`、`OdeCore`、`TensorCore`）。
- [tests](tests)：與模組對齊的測試集合。
- [UsageSamples.md](UsageSamples.md)：完整 usage samples 與模組級範例。
- [TestPerf.md](TestPerf.md)：模組測試耗時報告。
- [UnsafeRationale.md](UnsafeRationale.md)：unsafe/safety/performance 策略說明。

## 環境需求

- .NET SDK 版本以 [global.json](global.json) 為準：`9.0.312`。
- 專案 target framework：`net8.0`。

## 建置與測試

在 repository root 執行：

```bash
dotnet restore LAL.sln
dotnet build LAL.sln -c Release
dotnet test LAL.sln -c Release
```

只跑單一領域（例如 TensorCore）：

```bash
dotnet test LAL.Tests/LAL.Tests.csproj -c Release --filter FullyQualifiedName~LAL.Tests.TensorCore
```

## 快速開始

### 1) 加入 project reference

若在同一個 solution 或本機開發環境整合，可直接引用 [LAL.Core/LAL.Core.csproj](LAL.Core/LAL.Core.csproj)：

```xml
<ItemGroup>
  <ProjectReference Include="..\LAL.Core\LAL.Core.csproj" />
</ItemGroup>
```

### 2) 使用 wrapper API 與 NDBuffer

```csharp
using LAL.Api;
using LAL.Core;

int[] shape = DataStructureCompatibility.VectorShape(3);
NDBuffer<double> x = new(shape);
NDBuffer<double> y = new(shape);

[1d, 2d, 3d].CopyTo(x.AsSpan());
[10d, 20d, 30d].CopyTo(y.AsSpan());

LinalgApi.Axpy(0.5, x.AsReadOnlySpan(), y.AsSpan());
double dot = LinalgApi.Dot(x.AsReadOnlySpan(), y.AsReadOnlySpan());

Console.WriteLine($"Dot = {dot}");
// y => [10.5, 21.0, 31.5]
```

## 效能設定 (Performance settings)

可透過 `DataStructurePerformanceSettings` 調整 runtime strategy：

```csharp
using LAL.Core;

var current = DataStructureCompatibility.GetPerformanceSettings();

DataStructureCompatibility.SetPerformanceSettings(current with
{
    EnableParallel = true,
    ParallelLengthThreshold = 32768
});
```

常用檢視 API：

- `DataStructureCompatibility.GetModulePerformanceProfiles()`
- `DataStructureCompatibility.GetModuleAllocationGcProfiles()`
- `DataStructureCompatibility.GetAllocationGcGovernanceSummary()`

## 基準測試 (Benchmarks)

執行 baseline benchmark：

```bash
dotnet run --project LAL.Benchmarks/LAL.Benchmarks.csproj -c Release
```

報告會輸出到 `artifacts/perf`。

## API 與文件入口

- Public API wrappers：[ApiSurface.cs](ApiSurface.cs)
- 完整範例與片段：[UsageSamples.md](UsageSamples.md)
- 安全與策略說明：[UnsafeRationale.md](UnsafeRationale.md)
- 模組測試效能摘要：[TestPerf.md](TestPerf.md)

## 補充說明

- 目前專案偏向 source integration 模式，而非 NuGet package 發佈流程。
- 內部實作模組刻意透過 [ApiSurface.cs](ApiSurface.cs) 的 wrapper classes 對外暴露，以維持 API 一致性。