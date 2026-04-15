# Usage Samples (W3, Test-Ready Snippets)

本版重點：

1. Core 內公開資料結構與函數模組完整列出。
2. LinalgCore/NumericalCore/OdeCore/TensorCore 的 57 個模組，每一個都改成可直接貼到測試專案執行的完整程式片段。
3. 每個模組條目都包含：說明文檔、完整程式片段、數學計算流程。
4. 每個範例都透過 `NDBuffer<T>` 與 `DataStructureCompatibility` 進行共通資料結構配合。

模組來源：DataStructureCompatibility.CreateModuleProfiles()
總覆蓋：57（LinalgCore 19、NumericalCore 14、OdeCore 8、TensorCore 16）

## 1. Core：所有公開資料結構與函數模組

### 1.1 公開資料結構（完整）

| 類型 | 說明文檔 |
|---|---|
| NDBuffer<T> | 跨 Core 共通 n 維資料容器（Shape/Strides/Span）。 |
| PerformanceStrategyFlags | 執行策略旗標（Scalar/Simd/Intrinsics/Unsafe/Parallel）。 |
| DataStructurePerformanceSettings | 全域策略設定（SIMD/Intrinsics/Parallel 閾值）。 |
| ModulePerformanceProfile | 模組效能策略檔。 |
| AllocationOptimizationFlags | 配置優化旗標（StackAlloc/ArrayPool/Reuse/ThreadLocalScratch）。 |
| AllocationGcThresholds | 配置與 GC 閾值設定。 |
| ModuleAllocationGcProfile | 模組配置治理檔（Rule4/5/6/Unsafe）。 |
| AllocationGcGovernanceSummary | 全域治理摘要。 |

### 1.2 DataStructureCompatibility 基礎函數模組（完整）

| 模組 | 公開函數 |
|---|---|
| Shape 建構 | VectorShape, MatrixShape, NormalizeShape, GetElementCount |
| Addressing | RowMajorStrides, Offset |
| Complex Buffer | CreateComplexVector, CreateComplexMatrix, WrapComplex(2 overloads) |
| Compatibility | TryGetVectorLength, TryGetMatrixDimensions, EnsureBufferMatchesShape, EnsureVectorCompatible, EnsureMatrixCompatible, EnsureComplexCompatible, EnsureComplexVectorCompatible, EnsureComplexMatrixCompatible, IsRowMajorContiguous |

### 1.3 NDBuffer<T> 公開成員（完整）

| 類型 | 公開成員 |
|---|---|
| 屬性 | Shape, Strides, Length, IsRowMajorContiguous |
| 建構子 | NDBuffer(shape), NDBuffer(storage, shape), NDBuffer(storage, shape, strides) |
| 存取 | AsSpan, AsReadOnlySpan, GetOffset, At, Get, Set |
| 形狀推斷 | TryGetVectorLength, TryGetMatrixDimensions |

### 1.4 DataStructureCompatibility.Performance 函數模組（完整）

| 模組 | 公開函數 |
|---|---|
| Settings | GetPerformanceSettings, SetPerformanceSettings, ResetPerformanceSettings, GetRuntimeHardwareCapabilities |
| Performance Profiles | GetModulePerformanceProfiles, TryGetModulePerformanceProfile |
| Allocation Profiles | GetModuleAllocationGcProfiles, TryGetModuleAllocationGcProfile, GetAllocationGcGovernanceSummary |
| Compatibility Entry - Linalg | LinalgAxpy(double/float), LinalgDot(double/float) |
| Compatibility Entry - Tensor | TensorAdd/Subtract/Multiply/Divide (double/float/complex) |
| Compatibility Entry - Ode | OdeEulerStep(double/float/complex) |
| Compatibility Entry - Numerical | NumericalCorrelation(double/float/complex) |

### 1.5 Core 完整片段（可直接貼 tests）

#### Core/ShapeAddressing
說明文檔：示範 shape、stride、offset 與 NDBuffer 索引。

```csharp
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class CoreShapeAddressingUsageSampleTests
{
    [Fact]
    public void ShapeStrideOffset_WithNDBuffer_Sample()
    {
        int[] shape = DataStructureCompatibility.MatrixShape(2, 3);
        int[] strides = DataStructureCompatibility.RowMajorStrides(shape);

        NDBuffer<double> buf = new(shape);
        [1d, 2d, 3d, 4d, 5d, 6d].CopyTo(buf.AsSpan());

        int off = DataStructureCompatibility.Offset([1, 2], shape, strides);
        Assert.Equal(5, off);
        Assert.Equal(6d, buf.AsReadOnlySpan()[off]);
    }
}
```

數學計算流程：
1. $N=\prod_d shape[d]$
2. $stride[d]=\prod_{j>d}shape[j]$
3. $offset=\sum_d i_d\cdot stride[d]$

#### Core/CompatibilityValidation
說明文檔：示範向量與矩陣相容性檢查。

```csharp
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class CoreCompatibilityValidationUsageSampleTests
{
    [Fact]
    public void EnsureVectorAndMatrixCompatible_Sample()
    {
        int[] vShape = DataStructureCompatibility.VectorShape(4);
        int[] mShape = DataStructureCompatibility.MatrixShape(2, 2);

        double[] v = [1, 2, 3, 4];
        double[] m = [1, 2, 3, 4];

        DataStructureCompatibility.EnsureVectorCompatible(v, vShape);
        DataStructureCompatibility.EnsureMatrixCompatible(m, mShape);

        Assert.True(DataStructureCompatibility.IsRowMajorContiguous(mShape, DataStructureCompatibility.RowMajorStrides(mShape)));
    }
}
```

數學計算流程：
1. 向量：rank=1，長度一致。
2. 矩陣：rank=2，長度 $rows\times cols$。
3. stride 需符合 row-major 連續性。

#### Core/PerformanceProfiles
說明文檔：示範效能 profile 與治理摘要查詢。

```csharp
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class CorePerformanceProfilesUsageSampleTests
{
    [Fact]
    public void QueryProfilesAndSummary_Sample()
    {
        ReadOnlySpan<ModulePerformanceProfile> perf = DataStructureCompatibility.GetModulePerformanceProfiles();
        ReadOnlySpan<ModuleAllocationGcProfile> alloc = DataStructureCompatibility.GetModuleAllocationGcProfiles();
        AllocationGcGovernanceSummary summary = DataStructureCompatibility.GetAllocationGcGovernanceSummary();

        Assert.True(perf.Length > 0);
        Assert.Equal(perf.Length, alloc.Length);
        Assert.True(summary.ModuleCount > 0);
    }
}
```

數學計算流程：
1. 對每模組建立策略與配置向量。
2. 對向量做統計聚合（計數/違規數）。
3. 輸出治理摘要。

#### Core/CompatibilityEntry
說明文檔：示範 DataStructureCompatibility 相容入口直接計算。

```csharp
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class CoreCompatibilityEntryUsageSampleTests
{
    [Fact]
    public void LinalgDotTensorAddNumericalCorrelation_Sample()
    {
        NDBuffer<double> x = new(DataStructureCompatibility.VectorShape(3));
        NDBuffer<double> y = new(DataStructureCompatibility.VectorShape(3));
        [1d, 2d, 3d].CopyTo(x.AsSpan());
        [4d, 5d, 6d].CopyTo(y.AsSpan());

        double dot = DataStructureCompatibility.LinalgDot(x, y);

        NDBuffer<double> z = new(DataStructureCompatibility.VectorShape(3));
        DataStructureCompatibility.TensorAdd(x, y, z);
        double corr = DataStructureCompatibility.NumericalCorrelation(x, y);

        Assert.Equal(32d, dot, 10);
        Assert.Equal([5d, 7d, 9d], z.AsReadOnlySpan().ToArray());
        Assert.True(corr > 0.9);
    }
}
```

數學計算流程：
1. Dot：$x^Ty$。
2. TensorAdd：$z_i=x_i+y_i$。
3. Correlation：$r=\frac{cov(x,y)}{\sigma_x\sigma_y}$。

## 2. LinalgCore（19）完整可執行片段

#### LinalgCore/Axpy
說明文檔：向量線性更新。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class LinalgAxpyUsageSampleTests
{
    [Fact]
    public void Axpy_WithNDBuffer_Sample()
    {
        NDBuffer<double> x = new(DataStructureCompatibility.VectorShape(3));
        NDBuffer<double> y = new(DataStructureCompatibility.VectorShape(3));
        [1d, 2d, 3d].CopyTo(x.AsSpan());
        [10d, 20d, 30d].CopyTo(y.AsSpan());

        LinalgApi.Axpy(0.5, x.AsReadOnlySpan(), y.AsSpan());

        Assert.Equal([10.5, 21.0, 31.5], y.AsReadOnlySpan().ToArray());
    }
}
```

數學計算流程：$y_i \leftarrow \alpha x_i + y_i$。

#### LinalgCore/Cholesky
說明文檔：對稱正定矩陣分解。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class LinalgCholeskyUsageSampleTests
{
    [Fact]
    public void Cholesky_WithNDBuffer_Sample()
    {
        NDBuffer<double> a = new(DataStructureCompatibility.MatrixShape(2, 2));
        NDBuffer<double> l = new(DataStructureCompatibility.MatrixShape(2, 2));
        [4d, 2d, 2d, 3d].CopyTo(a.AsSpan());

        bool ok = LinalgApi.CholeskyDecomposeLower(a.AsReadOnlySpan(), 2, l.AsSpan());

        Assert.True(ok);
        Assert.True(l.AsReadOnlySpan()[0] > 0d);
    }
}
```

數學計算流程：$A=LL^\top$。

#### LinalgCore/DenseSolver
說明文檔：解稠密線性系統。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class LinalgDenseSolverUsageSampleTests
{
    [Fact]
    public void DenseSolve_WithNDBuffer_Sample()
    {
        NDBuffer<double> a = new(DataStructureCompatibility.MatrixShape(2, 2));
        NDBuffer<double> b = new(DataStructureCompatibility.VectorShape(2));
        NDBuffer<double> x = new(DataStructureCompatibility.VectorShape(2));
        [3d, 1d, 1d, 2d].CopyTo(a.AsSpan());
        [9d, 8d].CopyTo(b.AsSpan());

        bool ok = LinalgApi.DenseSolve(a.AsReadOnlySpan(), 2, b.AsReadOnlySpan(), x.AsSpan());

        Assert.True(ok);
        Assert.Equal(2d, x.AsReadOnlySpan()[0], 8);
        Assert.Equal(3d, x.AsReadOnlySpan()[1], 8);
    }
}
```

數學計算流程：$Ax=b$。

#### LinalgCore/DistanceMetrics
說明文檔：點集 pairwise 距離。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class LinalgDistanceMetricsUsageSampleTests
{
    [Fact]
    public void PairwiseEuclidean_WithNDBuffer_Sample()
    {
        int pointCount = 2;
        int dim = 2;
        NDBuffer<double> points = new(DataStructureCompatibility.MatrixShape(pointCount, dim));
        NDBuffer<double> d = new(DataStructureCompatibility.MatrixShape(pointCount, pointCount));
        [0d, 0d, 3d, 4d].CopyTo(points.AsSpan());

        LinalgApi.PairwiseEuclidean(points.AsReadOnlySpan(), pointCount, dim, d.AsSpan());

        Assert.Equal(5d, d.AsReadOnlySpan()[1], 10);
        Assert.Equal(5d, d.AsReadOnlySpan()[2], 10);
    }
}
```

數學計算流程：$d_{ij}=\|p_i-p_j\|_2$。

#### LinalgCore/Dot
說明文檔：向量內積。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class LinalgDotUsageSampleTests
{
    [Fact]
    public void Dot_WithNDBuffer_Sample()
    {
        NDBuffer<double> x = new(DataStructureCompatibility.VectorShape(3));
        NDBuffer<double> y = new(DataStructureCompatibility.VectorShape(3));
        [1d, 2d, 3d].CopyTo(x.AsSpan());
        [4d, 5d, 6d].CopyTo(y.AsSpan());

        double s = LinalgApi.Dot(x.AsReadOnlySpan(), y.AsReadOnlySpan());

        Assert.Equal(32d, s, 10);
    }
}
```

數學計算流程：$s=\sum_i x_i y_i$。

#### LinalgCore/EigenSolver
說明文檔：主特徵值估計。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class LinalgEigenSolverUsageSampleTests
{
    [Fact]
    public void PowerIteration_WithNDBuffer_Sample()
    {
        NDBuffer<double> a = new(DataStructureCompatibility.MatrixShape(2, 2));
        NDBuffer<double> v = new(DataStructureCompatibility.VectorShape(2));
        [2d, 0d, 0d, 1d].CopyTo(a.AsSpan());
        [1d, 1d].CopyTo(v.AsSpan());

        var r = LinalgApi.PowerIteration(a.AsReadOnlySpan(), 2, v.AsSpan());

        Assert.True(r.Eigenvalue > 1.5);
        Assert.True(r.Converged);
    }
}
```

數學計算流程：$v_{k+1}=Av_k/\|Av_k\|$。

#### LinalgCore/Gemm
說明文檔：矩陣乘矩陣。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class LinalgGemmUsageSampleTests
{
    [Fact]
    public void Gemm_WithNDBuffer_Sample()
    {
        NDBuffer<double> a = new(DataStructureCompatibility.MatrixShape(2, 2));
        NDBuffer<double> b = new(DataStructureCompatibility.MatrixShape(2, 2));
        NDBuffer<double> c = new(DataStructureCompatibility.MatrixShape(2, 2));
        [1d, 2d, 3d, 4d].CopyTo(a.AsSpan());
        [5d, 6d, 7d, 8d].CopyTo(b.AsSpan());

        LinalgApi.Gemm(a.AsReadOnlySpan(), b.AsReadOnlySpan(), c.AsSpan(), 2, 2, 2);

        Assert.Equal([19d, 22d, 43d, 50d], c.AsReadOnlySpan().ToArray());
    }
}
```

數學計算流程：$C_{ij}=\sum_p A_{ip}B_{pj}$。

#### LinalgCore/Gemv
說明文檔：矩陣乘向量。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class LinalgGemvUsageSampleTests
{
    [Fact]
    public void Gemv_WithNDBuffer_Sample()
    {
        NDBuffer<double> a = new(DataStructureCompatibility.MatrixShape(2, 2));
        NDBuffer<double> x = new(DataStructureCompatibility.VectorShape(2));
        NDBuffer<double> y = new(DataStructureCompatibility.VectorShape(2));
        [1d, 2d, 3d, 4d].CopyTo(a.AsSpan());
        [2d, 1d].CopyTo(x.AsSpan());

        LinalgApi.Gemv(a.AsReadOnlySpan(), 2, 2, x.AsReadOnlySpan(), y.AsSpan());

        Assert.Equal([4d, 10d], y.AsReadOnlySpan().ToArray());
    }
}
```

數學計算流程：$y_i=\sum_j A_{ij}x_j$。

#### LinalgCore/Lu
說明文檔：LU 分解求解。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class LinalgLuUsageSampleTests
{
    [Fact]
    public void LuFactorAndSolve_WithNDBuffer_Sample()
    {
        NDBuffer<double> a = new(DataStructureCompatibility.MatrixShape(2, 2));
        NDBuffer<double> b = new(DataStructureCompatibility.VectorShape(2));
        NDBuffer<double> x = new(DataStructureCompatibility.VectorShape(2));
        [3d, 1d, 1d, 2d].CopyTo(a.AsSpan());
        [9d, 8d].CopyTo(b.AsSpan());

        bool ok = LinalgApi.LuFactorAndSolve(a.AsReadOnlySpan(), 2, b.AsReadOnlySpan(), x.AsSpan());

        Assert.True(ok);
        Assert.Equal(2d, x.AsReadOnlySpan()[0], 8);
        Assert.Equal(3d, x.AsReadOnlySpan()[1], 8);
    }
}
```

數學計算流程：$PA=LU$，再解 $Ly=Pb$ 與 $Ux=y$。

#### LinalgCore/MatrixAnalysis
說明文檔：矩陣指標分析。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class LinalgMatrixAnalysisUsageSampleTests
{
    [Fact]
    public void Determinant_WithNDBuffer_Sample()
    {
        NDBuffer<double> a = new(DataStructureCompatibility.MatrixShape(2, 2));
        [4d, 7d, 2d, 6d].CopyTo(a.AsSpan());

        double det = LinalgApi.Determinant(a.AsReadOnlySpan(), 2);

        Assert.Equal(10d, det, 10);
    }
}
```

數學計算流程：$\det(A)$ 由分解結果計算。

#### LinalgCore/MatrixOps
說明文檔：矩陣元素運算。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class LinalgMatrixOpsUsageSampleTests
{
    [Fact]
    public void MatrixAdd_WithNDBuffer_Sample()
    {
        NDBuffer<double> a = new(DataStructureCompatibility.MatrixShape(2, 2));
        NDBuffer<double> b = new(DataStructureCompatibility.MatrixShape(2, 2));
        NDBuffer<double> c = new(DataStructureCompatibility.MatrixShape(2, 2));
        [1d, 2d, 3d, 4d].CopyTo(a.AsSpan());
        [10d, 20d, 30d, 40d].CopyTo(b.AsSpan());

        LinalgApi.MatrixAdd(a.AsReadOnlySpan(), b.AsReadOnlySpan(), c.AsSpan(), 2, 2);

        Assert.Equal([11d, 22d, 33d, 44d], c.AsReadOnlySpan().ToArray());
    }
}
```

數學計算流程：$C=A+B$。

#### LinalgCore/Norms
說明文檔：向量範數。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class LinalgNormsUsageSampleTests
{
    [Fact]
    public void L2Norm_WithNDBuffer_Sample()
    {
        NDBuffer<double> x = new(DataStructureCompatibility.VectorShape(2));
        [3d, 4d].CopyTo(x.AsSpan());

        double n2 = LinalgApi.NormL2(x.AsReadOnlySpan());

        Assert.Equal(5d, n2, 10);
    }
}
```

數學計算流程：$\|x\|_2=\sqrt{\sum_i x_i^2}$。

#### LinalgCore/Orthogonalization
說明文檔：Gram-Schmidt 正交化。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class LinalgOrthogonalizationUsageSampleTests
{
    [Fact]
    public void GramSchmidt_WithNDBuffer_Sample()
    {
        int vectorCount = 2;
        int dim = 2;
        NDBuffer<double> v = new(DataStructureCompatibility.MatrixShape(vectorCount, dim));
        NDBuffer<double> q = new(DataStructureCompatibility.MatrixShape(vectorCount, dim));
        [1d, 0d, 1d, 1d].CopyTo(v.AsSpan());

        int rank = LinalgApi.GramSchmidtOrthonormalize(v.AsReadOnlySpan(), vectorCount, dim, q.AsSpan());

        Assert.Equal(2, rank);
    }
}
```

數學計算流程：$u_k=v_k-\sum_j proj_{q_j}(v_k)$。

#### LinalgCore/Qr
說明文檔：Thin QR 分解。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class LinalgQrUsageSampleTests
{
    [Fact]
    public void QrDecomposeThin_WithNDBuffer_Sample()
    {
        NDBuffer<double> a = new(DataStructureCompatibility.MatrixShape(2, 2));
        NDBuffer<double> q = new(DataStructureCompatibility.MatrixShape(2, 2));
        NDBuffer<double> r = new(DataStructureCompatibility.MatrixShape(2, 2));
        [1d, 0d, 0d, 1d].CopyTo(a.AsSpan());

        bool ok = LinalgApi.QrDecomposeThin(a.AsReadOnlySpan(), 2, 2, q.AsSpan(), r.AsSpan());

        Assert.True(ok);
    }
}
```

數學計算流程：$A=QR$。

#### LinalgCore/Schur
說明文檔：2x2 實 Schur。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class LinalgSchurUsageSampleTests
{
    [Fact]
    public void RealSchur2x2_WithNDBuffer_Sample()
    {
        NDBuffer<double> a = new(DataStructureCompatibility.MatrixShape(2, 2));
        NDBuffer<double> t = new(DataStructureCompatibility.MatrixShape(2, 2));
        [1d, 2d, 3d, 4d].CopyTo(a.AsSpan());

        LinalgApi.RealSchur2x2(a.AsReadOnlySpan(), t.AsSpan());

        Assert.All(t.AsReadOnlySpan().ToArray(), v => Assert.False(double.IsNaN(v)));
    }
}
```

數學計算流程：$A=QTQ^\top$。

#### LinalgCore/Sparse.Spmv
說明文檔：CSR 稀疏乘向量。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class LinalgSpmvUsageSampleTests
{
    [Fact]
    public void Spmv_WithNDBuffer_Sample()
    {
        NDBuffer<double> values = new(DataStructureCompatibility.VectorShape(4));
        NDBuffer<int> col = new(DataStructureCompatibility.VectorShape(4));
        NDBuffer<int> rowPtr = new(DataStructureCompatibility.VectorShape(3));
        NDBuffer<double> x = new(DataStructureCompatibility.VectorShape(3));
        NDBuffer<double> y = new(DataStructureCompatibility.VectorShape(2));

        [10d, 20d, 30d, 40d].CopyTo(values.AsSpan());
        [0, 2, 1, 2].CopyTo(col.AsSpan());
        [0, 2, 4].CopyTo(rowPtr.AsSpan());
        [1d, 2d, 3d].CopyTo(x.AsSpan());

        LinalgApi.Spmv(values.AsReadOnlySpan(), col.AsReadOnlySpan(), rowPtr.AsReadOnlySpan(), x.AsReadOnlySpan(), y.AsSpan());

        Assert.Equal([70d, 180d], y.AsReadOnlySpan().ToArray());
    }
}
```

數學計算流程：$y_i=\sum_{p=rowPtr_i}^{rowPtr_{i+1}-1} val_p\,x_{col_p}$。

#### LinalgCore/Svd
說明文檔：奇異值計算。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class LinalgSvdUsageSampleTests
{
    [Fact]
    public void SingularValues_WithNDBuffer_Sample()
    {
        NDBuffer<double> a = new(DataStructureCompatibility.MatrixShape(2, 2));
        NDBuffer<double> s = new(DataStructureCompatibility.VectorShape(2));
        [3d, 0d, 0d, 4d].CopyTo(a.AsSpan());

        LinalgApi.SingularValues(a.AsReadOnlySpan(), 2, 2, s.AsSpan());

        Assert.Equal(4d, Math.Max(s.AsReadOnlySpan()[0], s.AsReadOnlySpan()[1]), 8);
    }
}
```

數學計算流程：$A=U\Sigma V^\top$。

#### LinalgCore/Transpose
說明文檔：矩陣轉置。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class LinalgTransposeUsageSampleTests
{
    [Fact]
    public void TransposeMatrix_WithNDBuffer_Sample()
    {
        NDBuffer<double> a = new(DataStructureCompatibility.MatrixShape(2, 3));
        NDBuffer<double> t = new(DataStructureCompatibility.MatrixShape(3, 2));
        [1d, 2d, 3d, 4d, 5d, 6d].CopyTo(a.AsSpan());

        LinalgApi.TransposeMatrix(a.AsReadOnlySpan(), 2, 3, t.AsSpan());

        Assert.Equal([1d, 4d, 2d, 5d, 3d, 6d], t.AsReadOnlySpan().ToArray());
    }
}
```

數學計算流程：$A^\top_{ij}=A_{ji}$。

#### LinalgCore/VectorOps
說明文檔：向量元素與雙線性運算。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class LinalgVectorOpsUsageSampleTests
{
    [Fact]
    public void VectorAdd_WithNDBuffer_Sample()
    {
        NDBuffer<double> x = new(DataStructureCompatibility.VectorShape(3));
        NDBuffer<double> y = new(DataStructureCompatibility.VectorShape(3));
        NDBuffer<double> z = new(DataStructureCompatibility.VectorShape(3));
        [1d, 2d, 3d].CopyTo(x.AsSpan());
        [4d, 5d, 6d].CopyTo(y.AsSpan());

        LinalgApi.VectorAdd(x.AsReadOnlySpan(), y.AsReadOnlySpan(), z.AsSpan());

        Assert.Equal([5d, 7d, 9d], z.AsReadOnlySpan().ToArray());
    }
}
```

數學計算流程：$z_i=x_i+y_i$。

## 3. NumericalCore（14）完整可執行片段

#### NumericalCore/Differentiation.FiniteDifference
說明文檔：中央差分微分。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class NumericalFiniteDifferenceUsageSampleTests
{
    [Fact]
    public void CentralDiff_WithNDBufferInput_Sample()
    {
        NDBuffer<double> x = new(DataStructureCompatibility.VectorShape(1));
        x.Set([0], 0d);

        double d = NumericalApi.CentralDiff(Math.Sin, x.Get([0]));

        Assert.Equal(1d, d, 4);
    }
}
```

數學計算流程：$f'(x)\approx\frac{f(x+h)-f(x-h)}{2h}$。

#### NumericalCore/Integration.BasicQuadrature
說明文檔：梯形積分。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class NumericalBasicQuadratureUsageSampleTests
{
    [Fact]
    public void Trapezoidal_WithNDBufferInterval_Sample()
    {
        NDBuffer<double> interval = new(DataStructureCompatibility.VectorShape(2));
        interval.Set([0], 0d);
        interval.Set([1], 1d);

        double area = NumericalApi.Trapezoidal(x => x * x, interval.Get([0]), interval.Get([1]), 2000);

        Assert.Equal(1d / 3d, area, 3);
    }
}
```

數學計算流程：分段梯形和近似定積分。

#### NumericalCore/Integration.GaussianQuadrature
說明文檔：高斯積分。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class NumericalGaussianQuadratureUsageSampleTests
{
    [Fact]
    public void GaussianIntegrate_WithNDBufferInterval_Sample()
    {
        NDBuffer<double> interval = new(DataStructureCompatibility.VectorShape(2));
        interval.Set([0], 0d);
        interval.Set([1], 1d);

        double area = NumericalApi.GaussianIntegrate(x => x * x, interval.Get([0]), interval.Get([1]), 3);

        Assert.Equal(1d / 3d, area, 6);
    }
}
```

數學計算流程：$\int_a^b f(x)dx\approx\sum_i w_i f(x_i)$。

#### NumericalCore/Interpolation.Rbf
說明文檔：RBF 插值。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class NumericalRbfUsageSampleTests
{
    [Fact]
    public void RbfEvaluateGaussian_WithNDBuffer_Sample()
    {
        NDBuffer<double> centers = new(DataStructureCompatibility.VectorShape(2));
        NDBuffer<double> weights = new(DataStructureCompatibility.VectorShape(2));
        [0d, 1d].CopyTo(centers.AsSpan());
        [1d, -0.5d].CopyTo(weights.AsSpan());

        double y = NumericalApi.RbfEvaluateGaussian(centers.AsReadOnlySpan(), weights.AsReadOnlySpan(), 0.25, 1.0);

        Assert.True(double.IsFinite(y));
    }
}
```

數學計算流程：$\hat f(x)=\sum_i w_i\phi(\|x-c_i\|)$。

#### NumericalCore/Interpolation.Spline
說明文檔：自然三次樣條。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class NumericalSplineUsageSampleTests
{
    [Fact]
    public void NaturalSpline_WithNDBuffer_Sample()
    {
        NDBuffer<double> xs = new(DataStructureCompatibility.VectorShape(3));
        NDBuffer<double> ys = new(DataStructureCompatibility.VectorShape(3));
        NDBuffer<double> m2 = new(DataStructureCompatibility.VectorShape(3));
        [0d, 1d, 2d].CopyTo(xs.AsSpan());
        [0d, 1d, 0d].CopyTo(ys.AsSpan());

        NumericalApi.ComputeNaturalSecondDerivatives(xs.AsReadOnlySpan(), ys.AsReadOnlySpan(), m2.AsSpan());
        double yq = NumericalApi.EvaluateNaturalCubic(xs.AsReadOnlySpan(), ys.AsReadOnlySpan(), m2.AsReadOnlySpan(), 1d);

        Assert.Equal(1d, yq, 3);
    }
}
```

數學計算流程：解三對角系統得到二階導數後分段內插。

#### NumericalCore/Optimization.GradientDescent
說明文檔：梯度下降最佳化。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class NumericalGradientDescentUsageSampleTests
{
    [Fact]
    public void GradientDescent_WithNDBufferInit_Sample()
    {
        NDBuffer<double> init = new(DataStructureCompatibility.VectorShape(1));
        init.Set([0], 5d);

        var r = NumericalApi.GradientDescent(x => (x - 1d) * (x - 1d), x => 2d * (x - 1d), init.Get([0]));

        Assert.True(r.Converged);
        Assert.Equal(1d, r.X, 3);
    }
}
```

數學計算流程：$x_{k+1}=x_k-\eta\nabla f(x_k)$。

#### NumericalCore/Optimization.Lbfgs
說明文檔：L-BFGS 準牛頓法。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class NumericalLbfgsUsageSampleTests
{
    [Fact]
    public void Lbfgs_WithNDBufferInit_Sample()
    {
        NDBuffer<double> init = new(DataStructureCompatibility.VectorShape(1));
        init.Set([0], 4d);

        var r = NumericalApi.LbfgsSolveScalar(x => (x - 2d) * (x - 2d), x => 2d * (x - 2d), init.Get([0]));

        Assert.True(r.Converged);
    }
}
```

數學計算流程：以歷史梯度/位移近似 Hessian 逆矩陣方向。

#### NumericalCore/Random.Rng
說明文檔：均勻亂數抽樣。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class NumericalRngUsageSampleTests
{
    [Fact]
    public void RngUniform_WithNDBufferStorage_Sample()
    {
        uint state = 42;
        NDBuffer<double> samples = new(DataStructureCompatibility.VectorShape(16));

        for (int i = 0; i < samples.Length; i++)
        {
            samples.AsSpan()[i] = NumericalApi.RngNextUniform(ref state);
        }

        Assert.All(samples.AsReadOnlySpan().ToArray(), v => Assert.InRange(v, 0d, 1d));
    }
}
```

數學計算流程：PRNG 狀態遞推後映射到 $[0,1)$。

#### NumericalCore/RootFinding.Brent
說明文檔：Brent 求根。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class NumericalBrentUsageSampleTests
{
    [Fact]
    public void Brent_WithNDBufferBracket_Sample()
    {
        NDBuffer<double> bracket = new(DataStructureCompatibility.VectorShape(2));
        bracket.Set([0], 1d);
        bracket.Set([1], 2d);

        var r = NumericalApi.Brent(x => x * x - 2d, bracket.Get([0]), bracket.Get([1]));

        Assert.True(r.Converged);
        Assert.Equal(Math.Sqrt(2d), r.Root, 6);
    }
}
```

數學計算流程：在 bracket 內混合二分與插值迭代。

#### NumericalCore/RootFinding.Newton
說明文檔：Newton 求根。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class NumericalNewtonUsageSampleTests
{
    [Fact]
    public void Newton_WithNDBufferInit_Sample()
    {
        NDBuffer<double> init = new(DataStructureCompatibility.VectorShape(1));
        init.Set([0], 1.5d);

        var r = NumericalApi.Newton(x => x * x - 2d, x => 2d * x, init.Get([0]));

        Assert.True(r.Converged);
        Assert.Equal(Math.Sqrt(2d), r.Root, 8);
    }
}
```

數學計算流程：$x_{k+1}=x_k-f(x_k)/f'(x_k)$。

#### NumericalCore/RootFinding.Secant
說明文檔：Secant 求根。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class NumericalSecantUsageSampleTests
{
    [Fact]
    public void Secant_WithNDBufferInit_Sample()
    {
        NDBuffer<double> init = new(DataStructureCompatibility.VectorShape(2));
        init.Set([0], 1d);
        init.Set([1], 2d);

        var r = NumericalApi.Secant(x => x * x - 2d, init.Get([0]), init.Get([1]));

        Assert.True(r.Converged);
        Assert.Equal(Math.Sqrt(2d), r.Root, 6);
    }
}
```

數學計算流程：以割線近似導數更新根估計。

#### NumericalCore/Statistics.Covariance
說明文檔：協方差。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class NumericalCovarianceUsageSampleTests
{
    [Fact]
    public void Covariance_WithNDBuffer_Sample()
    {
        NDBuffer<double> x = new(DataStructureCompatibility.VectorShape(3));
        NDBuffer<double> y = new(DataStructureCompatibility.VectorShape(3));
        [1d, 2d, 3d].CopyTo(x.AsSpan());
        [1d, 2d, 3d].CopyTo(y.AsSpan());

        double cov = NumericalApi.Covariance(x.AsReadOnlySpan(), y.AsReadOnlySpan());

        Assert.Equal(1d, cov, 10);
    }
}
```

數學計算流程：$cov=\frac{1}{n-1}\sum_i(x_i-\bar x)(y_i-\bar y)$。

#### NumericalCore/Statistics.PearsonCorrelation
說明文檔：Pearson 相關係數。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class NumericalPearsonCorrelationUsageSampleTests
{
    [Fact]
    public void PearsonCompute_WithNDBuffer_Sample()
    {
        NDBuffer<double> x = new(DataStructureCompatibility.VectorShape(3));
        NDBuffer<double> y = new(DataStructureCompatibility.VectorShape(3));
        [1d, 2d, 3d].CopyTo(x.AsSpan());
        [2d, 4d, 6d].CopyTo(y.AsSpan());

        double r = NumericalApi.PearsonCompute(x.AsReadOnlySpan(), y.AsReadOnlySpan());

        Assert.Equal(1d, r, 10);
    }
}
```

數學計算流程：$r=\frac{cov(x,y)}{\sigma_x\sigma_y}$。

#### NumericalCore/Statistics.Ziggurat
說明文檔：Ziggurat 常態抽樣。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class NumericalZigguratUsageSampleTests
{
    [Fact]
    public void ZigguratNormal_WithNDBufferStorage_Sample()
    {
        uint state = 7;
        NDBuffer<double> z = new(DataStructureCompatibility.VectorShape(8));

        for (int i = 0; i < z.Length; i++)
        {
            z.AsSpan()[i] = NumericalApi.ZigguratNextNormal(ref state);
        }

        Assert.All(z.AsReadOnlySpan().ToArray(), v => Assert.True(double.IsFinite(v)));
    }
}
```

數學計算流程：分層 acceptance-rejection 取樣。

## 4. OdeCore（8）完整可執行片段

#### OdeCore/Bdf
說明文檔：後退歐拉一步。

```csharp
using LAL.Api;
using LAL.Core;
using LAL.OdeCore;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class OdeBdfUsageSampleTests
{
    [Fact]
    public void StepBackwardEuler_WithNDBuffer_Sample()
    {
        NDBuffer<double> y = new(DataStructureCompatibility.VectorShape(1));
        NDBuffer<double> yOut = new(DataStructureCompatibility.VectorShape(1));
        y.Set([0], 1d);

        var s = OdeApi.StepBackwardEuler(0d, 0.1d, y.AsReadOnlySpan(), yOut.AsSpan(), static (_, state, dydt) =>
        {
            dydt[0] = -state[0];
        });

        Assert.True(s.Iterations >= 1);
        Assert.True(yOut.Get([0]) < 1d);
    }
}
```

數學計算流程：解隱式方程 $y_{n+1}=y_n+dt f(t_{n+1},y_{n+1})$。

#### OdeCore/DenseOutput
說明文檔：步內線性插值。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class OdeDenseOutputUsageSampleTests
{
    [Fact]
    public void InterpolateLinear_WithNDBuffer_Sample()
    {
        NDBuffer<double> y0 = new(DataStructureCompatibility.VectorShape(2));
        NDBuffer<double> y1 = new(DataStructureCompatibility.VectorShape(2));
        NDBuffer<double> y = new(DataStructureCompatibility.VectorShape(2));
        [0d, 10d].CopyTo(y0.AsSpan());
        [10d, 20d].CopyTo(y1.AsSpan());

        OdeApi.InterpolateLinear(y0.AsReadOnlySpan(), y1.AsReadOnlySpan(), 0.5, y.AsSpan());

        Assert.Equal([5d, 15d], y.AsReadOnlySpan().ToArray());
    }
}
```

數學計算流程：$y(\theta)=(1-\theta)y_0+\theta y_1$。

#### OdeCore/Euler
說明文檔：顯式 Euler。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class OdeEulerUsageSampleTests
{
    [Fact]
    public void EulerStep_WithNDBuffer_Sample()
    {
        NDBuffer<double> y = new(DataStructureCompatibility.VectorShape(1));
        NDBuffer<double> yOut = new(DataStructureCompatibility.VectorShape(1));
        y.Set([0], 1d);

        OdeApi.EulerStep(0d, 0.1d, y.AsReadOnlySpan(), yOut.AsSpan(), static (_, state, dydt) =>
        {
            dydt[0] = state[0];
        });

        Assert.Equal(1.1d, yOut.Get([0]), 10);
    }
}
```

數學計算流程：$y_{n+1}=y_n+dt f(t_n,y_n)$。

#### OdeCore/JacobianEstimator
說明文檔：前向差分 Jacobian。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class OdeJacobianEstimatorUsageSampleTests
{
    [Fact]
    public void EstimateForwardDifference_WithNDBuffer_Sample()
    {
        NDBuffer<double> y = new(DataStructureCompatibility.VectorShape(2));
        NDBuffer<double> j = new(DataStructureCompatibility.MatrixShape(2, 2));
        [2d, 3d].CopyTo(y.AsSpan());

        OdeApi.EstimateForwardDifference(0d, y.AsReadOnlySpan(), j.AsSpan(), static (_, state, dydt) =>
        {
            dydt[0] = 2d * state[0];
            dydt[1] = 3d * state[1];
        });

        Assert.Equal(2d, j.AsReadOnlySpan()[0], 2);
        Assert.Equal(3d, j.AsReadOnlySpan()[3], 2);
    }
}
```

數學計算流程：$J_{ij}\approx \frac{f_i(y+\epsilon e_j)-f_i(y)}{\epsilon}$。

#### OdeCore/Radau
說明文檔：一階段 Radau。

```csharp
using LAL.Api;
using LAL.Core;
using LAL.OdeCore;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class OdeRadauUsageSampleTests
{
    [Fact]
    public void StepOneStage_WithNDBuffer_Sample()
    {
        NDBuffer<double> y = new(DataStructureCompatibility.VectorShape(1));
        NDBuffer<double> yOut = new(DataStructureCompatibility.VectorShape(1));
        y.Set([0], 1d);

        var s = OdeApi.StepOneStage(0d, 0.1d, y.AsReadOnlySpan(), yOut.AsSpan(), static (_, state, dydt) =>
        {
            dydt[0] = -state[0];
        });

        Assert.True(s.Iterations >= 1);
        Assert.True(yOut.Get([0]) < 1d);
    }
}
```

數學計算流程：隱式 stage 求解 + 加權更新。

#### OdeCore/Rk4
說明文檔：四階 Runge-Kutta。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class OdeRk4UsageSampleTests
{
    [Fact]
    public void Rk4Step_WithNDBuffer_Sample()
    {
        NDBuffer<double> y = new(DataStructureCompatibility.VectorShape(1));
        NDBuffer<double> yOut = new(DataStructureCompatibility.VectorShape(1));
        y.Set([0], 1d);

        OdeApi.Rk4Step(0d, 0.1d, y.AsReadOnlySpan(), yOut.AsSpan(), static (_, state, dydt) =>
        {
            dydt[0] = state[0];
        });

        Assert.Equal(1.10517d, yOut.Get([0]), 3);
    }
}
```

數學計算流程：$k_1..k_4$ 加權合成一步。

#### OdeCore/Rk45
說明文檔：自適應 RK45。

```csharp
using LAL.Api;
using LAL.Core;
using LAL.OdeCore;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class OdeRk45UsageSampleTests
{
    [Fact]
    public void Rk45Step_WithNDBuffer_Sample()
    {
        NDBuffer<double> y = new(DataStructureCompatibility.VectorShape(1));
        NDBuffer<double> yOut = new(DataStructureCompatibility.VectorShape(1));
        y.Set([0], 1d);

        var s = OdeApi.Rk45Step(0d, 0.1d, y.AsReadOnlySpan(), yOut.AsSpan(), static (_, state, dydt) =>
        {
            dydt[0] = -state[0];
        });

        Assert.True(s.EstimatedError >= 0d);
        Assert.True(yOut.Get([0]) < 1d);
    }
}
```

數學計算流程：同時計算 4/5 階估計並取差做誤差控制。

#### OdeCore/StepController
說明文檔：步長控制器。

```csharp
using LAL.Api;
using LAL.Core;
using LAL.OdeCore;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class OdeStepControllerUsageSampleTests
{
    [Fact]
    public void ProposeStep_WithNDBufferMetrics_Sample()
    {
        NDBuffer<double> metrics = new(DataStructureCompatibility.VectorShape(3));
        metrics.Set([0], 0.1d);   // currentStep
        metrics.Set([1], 1e-5d);  // estimatedError
        metrics.Set([2], 1e-4d);  // tolerance

        var r = OdeApi.ProposeStep(metrics.Get([0]), metrics.Get([1]), metrics.Get([2]));

        Assert.True(r.Accepted);
        Assert.True(r.NextStep > 0d);
    }
}
```

數學計算流程：$h_{new}=h\cdot safety\cdot (tol/err)^p$（含上下限裁剪）。

## 5. TensorCore（16）完整可執行片段

#### TensorCore/Broadcasting
說明文檔：廣播 shape 推導。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class TensorBroadcastingUsageSampleTests
{
    [Fact]
    public void BroadcastShape_WithNDBufferShape_Sample()
    {
        NDBuffer<double> a = new([3, 1, 5]);
        NDBuffer<double> b = new([1, 4, 5]);

        int[] shape = TensorApi.BroadcastShape(a.Shape, b.Shape);

        Assert.Equal([3, 4, 5], shape);
    }
}
```

數學計算流程：尾端對齊逐維取 max（需相等或含 1）。

#### TensorCore/ComplexOps
說明文檔：複數共軛/實虛部運算。

```csharp
using LAL.Api;
using LAL.Core;
using System.Numerics;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class TensorComplexOpsUsageSampleTests
{
    [Fact]
    public void Conjugate_WithNDBuffer_Sample()
    {
        NDBuffer<Complex> z = new(DataStructureCompatibility.VectorShape(2));
        NDBuffer<Complex> c = new(DataStructureCompatibility.VectorShape(2));
        [new Complex(1, 2), new Complex(-3, 4)].CopyTo(z.AsSpan());

        TensorApi.Conjugate(z.AsReadOnlySpan(), c.AsSpan());

        Assert.Equal(new Complex(1, -2), c.AsReadOnlySpan()[0]);
        Assert.Equal(new Complex(-3, -4), c.AsReadOnlySpan()[1]);
    }
}
```

數學計算流程：$(a+bi)^*=a-bi$。

#### TensorCore/ConcatStack
說明文檔：向量拼接與堆疊。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class TensorConcatStackUsageSampleTests
{
    [Fact]
    public void Concatenate_WithNDBuffer_Sample()
    {
        NDBuffer<double> left = new(DataStructureCompatibility.VectorShape(2));
        NDBuffer<double> right = new(DataStructureCompatibility.VectorShape(2));
        NDBuffer<double> dst = new(DataStructureCompatibility.VectorShape(4));
        [1d, 2d].CopyTo(left.AsSpan());
        [3d, 4d].CopyTo(right.AsSpan());

        TensorApi.Concatenate(left.AsReadOnlySpan(), right.AsReadOnlySpan(), dst.AsSpan());

        Assert.Equal([1d, 2d, 3d, 4d], dst.AsReadOnlySpan().ToArray());
    }
}
```

數學計算流程：順序區塊拷貝到連續目標空間。

#### TensorCore/Convolution
說明文檔：卷積策略管理入口。

```csharp
using LAL.Api;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class TensorConvolutionUsageSampleTests
{
    [Fact]
    public void ConvolutionSettings_GetSetReset_Sample()
    {
        var original = TensorApi.GetConvolutionParallelSettings();

        var tuned = original with { Parallel2DOutputThreshold = 2048 };
        TensorApi.SetConvolutionParallelSettings(tuned);

        var current = TensorApi.GetConvolutionParallelSettings();
        Assert.Equal(2048, current.Parallel2DOutputThreshold);

        TensorApi.ResetConvolutionParallelSettings();
    }
}
```

數學計算流程：卷積核心仍為 $y[n]=\sum_k x[k]h[n-k]$，此處示範策略參數控制。

#### TensorCore/Cumulative
說明文檔：前綴和/前綴積。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class TensorCumulativeUsageSampleTests
{
    [Fact]
    public void CumSum_WithNDBuffer_Sample()
    {
        NDBuffer<double> x = new(DataStructureCompatibility.VectorShape(4));
        NDBuffer<double> s = new(DataStructureCompatibility.VectorShape(4));
        [1d, 2d, 3d, 4d].CopyTo(x.AsSpan());

        TensorApi.CumSum(x.AsReadOnlySpan(), s.AsSpan());

        Assert.Equal([1d, 3d, 6d, 10d], s.AsReadOnlySpan().ToArray());
    }
}
```

數學計算流程：$s_i=\sum_{j=0}^{i}x_j$。

#### TensorCore/Einsum
說明文檔：張量縮約。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class TensorEinsumUsageSampleTests
{
    [Fact]
    public void EvaluateDot_WithNDBuffer_Sample()
    {
        NDBuffer<double> x = new(DataStructureCompatibility.VectorShape(3));
        NDBuffer<double> y = new(DataStructureCompatibility.VectorShape(3));
        [1d, 2d, 3d].CopyTo(x.AsSpan());
        [4d, 5d, 6d].CopyTo(y.AsSpan());

        double d = TensorApi.Evaluate("i,i->", x.AsReadOnlySpan(), y.AsReadOnlySpan());

        Assert.Equal(32d, d, 10);
    }
}
```

數學計算流程：依簽名對共享索引求和。

#### TensorCore/Fft
說明文檔：傅立葉變換。

```csharp
using LAL.Api;
using LAL.Core;
using System.Numerics;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class TensorFftUsageSampleTests
{
    [Fact]
    public void Forward_WithNDBuffer_Sample()
    {
        NDBuffer<Complex> time = new(DataStructureCompatibility.VectorShape(4));
        NDBuffer<Complex> freq = new(DataStructureCompatibility.VectorShape(4));
        [Complex.One, Complex.Zero, Complex.Zero, Complex.Zero].CopyTo(time.AsSpan());

        TensorApi.Forward(time.AsReadOnlySpan(), freq.AsSpan());

        Assert.Equal(1d, freq.AsReadOnlySpan()[0].Real, 10);
    }
}
```

數學計算流程：$X_k=\sum_n x_n e^{-i2\pi kn/N}$。

#### TensorCore/MaskOps
說明文檔：遮罩與條件選擇。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class TensorMaskOpsUsageSampleTests
{
    [Fact]
    public void GreaterThan_WithNDBuffer_Sample()
    {
        NDBuffer<double> x = new(DataStructureCompatibility.VectorShape(4));
        NDBuffer<bool> m = new(DataStructureCompatibility.VectorShape(4));
        [1d, 3d, -1d, 2d].CopyTo(x.AsSpan());

        TensorApi.GreaterThan(x.AsReadOnlySpan(), 1.5, m.AsSpan());

        Assert.Equal([false, true, false, true], m.AsReadOnlySpan().ToArray());
    }
}
```

數學計算流程：$m_i = (x_i > threshold)$。

#### TensorCore/Padding
說明文檔：一維邊界補值。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class TensorPaddingUsageSampleTests
{
    [Fact]
    public void ZeroPad1D_WithNDBuffer_Sample()
    {
        NDBuffer<double> x = new(DataStructureCompatibility.VectorShape(2));
        [1d, 2d].CopyTo(x.AsSpan());

        double[] padded = TensorApi.ZeroPad1D(x.AsReadOnlySpan(), 1, 2);
        NDBuffer<double> p = new(padded, DataStructureCompatibility.VectorShape(padded.Length));

        Assert.Equal([0d, 1d, 2d, 0d, 0d], p.AsReadOnlySpan().ToArray());
    }
}
```

數學計算流程：擴張索引域並填入規則值。

#### TensorCore/Reductions
說明文檔：降維彙總。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class TensorReductionsUsageSampleTests
{
    [Fact]
    public void Sum_WithNDBuffer_Sample()
    {
        NDBuffer<double> x = new(DataStructureCompatibility.VectorShape(4));
        [1d, 2d, 3d, 4d].CopyTo(x.AsSpan());

        double s = TensorApi.Sum(x.AsReadOnlySpan());

        Assert.Equal(10d, s, 10);
    }
}
```

數學計算流程：$s=\sum_i x_i$。

#### TensorCore/ShapeOps
說明文檔：reshape/transpose 等形狀操作。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class TensorShapeOpsUsageSampleTests
{
    [Fact]
    public void Transpose2D_WithNDBuffer_Sample()
    {
        NDBuffer<double> a = new(DataStructureCompatibility.MatrixShape(2, 3));
        NDBuffer<double> t = new(DataStructureCompatibility.MatrixShape(3, 2));
        [1d, 2d, 3d, 4d, 5d, 6d].CopyTo(a.AsSpan());

        TensorApi.Transpose2D(a.AsReadOnlySpan(), 2, 3, t.AsSpan());

        Assert.Equal([1d, 4d, 2d, 5d, 3d, 6d], t.AsReadOnlySpan().ToArray());
    }
}
```

數學計算流程：重映射索引，不改變元素集合。

#### TensorCore/SortSearch
說明文檔：排序序列二分定位。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class TensorSortSearchUsageSampleTests
{
    [Fact]
    public void SearchSorted_WithNDBuffer_Sample()
    {
        NDBuffer<double> sorted = new(DataStructureCompatibility.VectorShape(4));
        [1d, 3d, 5d, 7d].CopyTo(sorted.AsSpan());

        int idx = TensorApi.SearchSorted(sorted.AsReadOnlySpan(), 4d);

        Assert.Equal(2, idx);
    }
}
```

數學計算流程：二分搜尋插入索引。

#### TensorCore/StridedView
說明文檔：步長切片視圖。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class TensorStridedViewUsageSampleTests
{
    [Fact]
    public void Slice1D_WithNDBuffer_Sample()
    {
        NDBuffer<double> x = new(DataStructureCompatibility.VectorShape(5));
        [10d, 20d, 30d, 40d, 50d].CopyTo(x.AsSpan());

        ReadOnlySpan<double> s = TensorApi.Slice1D(x.AsReadOnlySpan(), 1, 3);

        Assert.Equal([20d, 30d, 40d], s.ToArray());
    }
}
```

數學計算流程：$view_i = data[start + i\cdot step]$。

#### TensorCore/TensorShape
說明文檔：shape/stride/offset 工具。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class TensorTensorShapeUsageSampleTests
{
    [Fact]
    public void RowMajorStridesAndOffset_WithNDBufferShape_Sample()
    {
        NDBuffer<double> a = new(DataStructureCompatibility.MatrixShape(2, 3));
        int[] strides = TensorApi.RowMajorStrides(a.Shape);
        int off = TensorApi.Offset([1, 2], a.Shape, strides);

        Assert.Equal([3, 1], strides);
        Assert.Equal(5, off);
    }
}
```

數學計算流程：$offset=\sum_d i_d\cdot stride_d$。

#### TensorCore/UFuncArithmetic
說明文檔：元素級加減乘除。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class TensorUFuncArithmeticUsageSampleTests
{
    [Fact]
    public void Add_WithNDBuffer_Sample()
    {
        NDBuffer<double> a = new(DataStructureCompatibility.VectorShape(3));
        NDBuffer<double> b = new(DataStructureCompatibility.VectorShape(3));
        NDBuffer<double> c = new(DataStructureCompatibility.VectorShape(3));
        [1d, 2d, 3d].CopyTo(a.AsSpan());
        [4d, 5d, 6d].CopyTo(b.AsSpan());

        TensorApi.Add(a.AsReadOnlySpan(), b.AsReadOnlySpan(), c.AsSpan());

        Assert.Equal([5d, 7d, 9d], c.AsReadOnlySpan().ToArray());
    }
}
```

數學計算流程：$c_i = a_i + b_i$。

#### TensorCore/UFuncTranscendental
說明文檔：元素級超越函數。

```csharp
using LAL.Api;
using LAL.Core;
using Xunit;

namespace LAL.Tests.UsageSamples;

public class TensorUFuncTranscendentalUsageSampleTests
{
    [Fact]
    public void Exp_WithNDBuffer_Sample()
    {
        NDBuffer<double> x = new(DataStructureCompatibility.VectorShape(2));
        NDBuffer<double> y = new(DataStructureCompatibility.VectorShape(2));
        [0d, 1d].CopyTo(x.AsSpan());

        TensorApi.Exp(x.AsReadOnlySpan(), y.AsSpan());

        Assert.Equal(1d, y.AsReadOnlySpan()[0], 10);
        Assert.Equal(Math.E, y.AsReadOnlySpan()[1], 10);
    }
}
```

數學計算流程：$y_i=e^{x_i}$。

## 6. 補充說明

1. 所有片段皆為獨立測試檔格式（含 using / namespace / class / [Fact]）。
2. 貼到測試專案時，建議每個片段存為獨立 `*UsageSampleTests.cs`。
3. 範例以可讀性為主，若要納入正式回歸測試可再加上更嚴格容差與邊界案例。
