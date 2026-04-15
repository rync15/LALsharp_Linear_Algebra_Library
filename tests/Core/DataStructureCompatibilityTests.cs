using System.Numerics;
using LAL.Core;
using LAL.LinalgCore;
using LAL.NumericalCore.Statistics;
using LAL.OdeCore;
using LAL.TensorCore;

namespace LAL.Tests.Core;

public class DataStructureCompatibilityTests
{
    [Fact]
    public void VectorAndMatrixShapeFactories_Work()
    {
        int[] vectorShape = DataStructureCompatibility.VectorShape(5);
        int[] matrixShape = DataStructureCompatibility.MatrixShape(2, 3);

        Assert.Equal([5], vectorShape);
        Assert.Equal([2, 3], matrixShape);
        Assert.Equal(6, DataStructureCompatibility.GetElementCount(matrixShape));
    }

    [Fact]
    public void EnsureBufferMatchesShape_Throws_OnMismatch()
    {
        ArgumentException ex = Assert.Throws<ArgumentException>(
            () => DataStructureCompatibility.EnsureBufferMatchesShape([1d, 2d], [3]));

        Assert.Contains("does not match shape element count", ex.Message);
    }

    [Fact]
    public void NDBuffer_RowMajor_Matrix_IsCompatible_WithLinalg()
    {
        NDBuffer<double> matrix = new([2, 2]);
        double[] matrixSource = [1d, 2d, 3d, 4d];
        matrixSource.CopyTo(matrix.AsSpan());

        DataStructureCompatibility.EnsureMatrixCompatible(matrix.AsReadOnlySpan(), matrix.Shape);

        double[] y = new double[2];
        Gemv.Multiply(matrix.AsReadOnlySpan(), 2, 2, [1d, 1d], y);

        Assert.Equal(3d, y[0]);
        Assert.Equal(7d, y[1]);
        Assert.True(matrix.IsRowMajorContiguous);
    }

    [Fact]
    public void SharedBuffer_WorksAcross_Tensor_Linalg_Ode_Numerical()
    {
        NDBuffer<double> state = new([3]);
        double[] stateSource = [1d, 2d, 3d];
        stateSource.CopyTo(state.AsSpan());

        NDBuffer<double> yOut = new([3]);
        Euler.Step(
            0d,
            0.1d,
            state.AsReadOnlySpan(),
            yOut.AsSpan(),
            static (_, y, dydt) =>
            {
                for (int i = 0; i < y.Length; i++)
                {
                    dydt[i] = y[i];
                }
            });

        double sum = Reductions.Sum(yOut.AsReadOnlySpan());
        double norm = Norms.L2(yOut.AsReadOnlySpan());
        double corr = Covariance.Correlation(state.AsReadOnlySpan(), yOut.AsReadOnlySpan());

        Assert.True(sum > 0d);
        Assert.True(norm > 0d);
        Assert.True(corr > 0.99d);
    }

    [Fact]
    public void ComplexMatrixCompatibility_WorksWithShapeStrideAndOffset()
    {
        NDBuffer<Complex> matrix = DataStructureCompatibility.CreateComplexMatrix(2, 2);
        Complex[] source =
        [
            new Complex(1d, 1d),
            new Complex(2d, -1d),
            new Complex(3d, 0d),
            new Complex(4d, 2d)
        ];
        source.CopyTo(matrix.AsSpan());

        DataStructureCompatibility.EnsureComplexMatrixCompatible(matrix.AsReadOnlySpan(), matrix.Shape);
        int offset = DataStructureCompatibility.Offset([1, 0], matrix.Shape, matrix.Strides);

        Assert.Equal(2, offset);
        Assert.Equal(new Complex(3d, 0d), matrix.AsReadOnlySpan()[offset]);
        Assert.True(matrix.IsRowMajorContiguous);
    }

    [Fact]
    public void ComplexBuffer_WorksAcross_Tensor_Linalg_Ode_Numerical()
    {
        NDBuffer<Complex> state = DataStructureCompatibility.CreateComplexVector(3);
        Complex[] source =
        [
            new Complex(1d, 2d),
            new Complex(2d, 1d),
            new Complex(3d, -1d)
        ];
        source.CopyTo(state.AsSpan());

        DataStructureCompatibility.EnsureComplexVectorCompatible(state.AsReadOnlySpan(), state.Shape);

        NDBuffer<Complex> yOut = DataStructureCompatibility.CreateComplexVector(3);
        Euler.Step(
            0d,
            0.1d,
            state.AsReadOnlySpan(),
            yOut.AsSpan(),
            static (_, y, dydt) =>
            {
                for (int i = 0; i < y.Length; i++)
                {
                    dydt[i] = Complex.ImaginaryOne * y[i];
                }
            });

        Complex sum = Reductions.Sum(yOut.AsReadOnlySpan());
        double l2 = Norms.L2(yOut.AsReadOnlySpan());
        Complex corr = PearsonCorrelation.Compute(state.AsReadOnlySpan(), yOut.AsReadOnlySpan());

        Assert.True(sum != Complex.Zero);
        Assert.True(l2 > 0d);
        Assert.True(corr.Magnitude > 0d);
    }

    [Fact]
    public void PerformanceProfiles_CoverAllCoreModules()
    {
        ReadOnlySpan<ModulePerformanceProfile> profiles = DataStructureCompatibility.GetModulePerformanceProfiles();
        Assert.Equal(57, profiles.Length);

        int linalgCount = 0;
        int numericalCount = 0;
        int odeCount = 0;
        int tensorCount = 0;

        foreach (ModulePerformanceProfile profile in profiles)
        {
            switch (profile.Core)
            {
                case "LinalgCore":
                    linalgCount++;
                    break;
                case "NumericalCore":
                    numericalCount++;
                    break;
                case "OdeCore":
                    odeCount++;
                    break;
                case "TensorCore":
                    tensorCount++;
                    break;
                default:
                    throw new Xunit.Sdk.XunitException($"Unexpected core category: {profile.Core}");
            }
        }

        Assert.Equal(19, linalgCount);
        Assert.Equal(14, numericalCount);
        Assert.Equal(8, odeCount);
        Assert.Equal(16, tensorCount);

        string[] expectedModules =
        [
            "LinalgCore:Axpy", "LinalgCore:Cholesky", "LinalgCore:DenseSolver", "LinalgCore:DistanceMetrics", "LinalgCore:Dot",
            "LinalgCore:EigenSolver", "LinalgCore:Gemm", "LinalgCore:Gemv", "LinalgCore:Lu", "LinalgCore:MatrixAnalysis",
            "LinalgCore:MatrixOps", "LinalgCore:Norms", "LinalgCore:Orthogonalization", "LinalgCore:Qr", "LinalgCore:Schur",
            "LinalgCore:Sparse.Spmv", "LinalgCore:Svd", "LinalgCore:Transpose", "LinalgCore:VectorOps",
            "NumericalCore:Differentiation.FiniteDifference", "NumericalCore:Integration.BasicQuadrature", "NumericalCore:Integration.GaussianQuadrature",
            "NumericalCore:Interpolation.Rbf", "NumericalCore:Interpolation.Spline", "NumericalCore:Optimization.GradientDescent",
            "NumericalCore:Optimization.Lbfgs", "NumericalCore:Random.Rng", "NumericalCore:RootFinding.Brent", "NumericalCore:RootFinding.Newton",
            "NumericalCore:RootFinding.Secant", "NumericalCore:Statistics.Covariance", "NumericalCore:Statistics.PearsonCorrelation",
            "NumericalCore:Statistics.Ziggurat",
            "OdeCore:Bdf", "OdeCore:DenseOutput", "OdeCore:Euler", "OdeCore:JacobianEstimator", "OdeCore:Radau",
            "OdeCore:Rk4", "OdeCore:Rk45", "OdeCore:StepController",
            "TensorCore:Broadcasting", "TensorCore:ComplexOps", "TensorCore:ConcatStack", "TensorCore:Convolution", "TensorCore:Cumulative",
            "TensorCore:Einsum", "TensorCore:Fft", "TensorCore:MaskOps", "TensorCore:Padding", "TensorCore:Reductions",
            "TensorCore:ShapeOps", "TensorCore:SortSearch", "TensorCore:StridedView", "TensorCore:TensorShape",
            "TensorCore:UFuncArithmetic", "TensorCore:UFuncTranscendental"
        ];

        foreach (string expected in expectedModules)
        {
            string[] parts = expected.Split(':', 2);
            Assert.True(
                DataStructureCompatibility.TryGetModulePerformanceProfile(parts[0], parts[1], out _),
                $"Missing module performance profile: {expected}");
        }

        PerformanceStrategyFlags capabilities = DataStructureCompatibility.GetRuntimeHardwareCapabilities();
        Assert.True((capabilities & PerformanceStrategyFlags.Scalar) != 0);
    }

    [Fact]
    public void AllocationGcProfiles_CoverAllModules_AndSatisfyGovernanceFlags()
    {
        ReadOnlySpan<ModulePerformanceProfile> performanceProfiles = DataStructureCompatibility.GetModulePerformanceProfiles();
        ReadOnlySpan<ModuleAllocationGcProfile> allocationProfiles = DataStructureCompatibility.GetModuleAllocationGcProfiles();

        Assert.Equal(performanceProfiles.Length, allocationProfiles.Length);

        foreach (ModulePerformanceProfile performanceProfile in performanceProfiles)
        {
            Assert.True(
                DataStructureCompatibility.TryGetModuleAllocationGcProfile(performanceProfile.Core, performanceProfile.Module, out ModuleAllocationGcProfile allocationProfile),
                $"Missing allocation/GC profile: {performanceProfile.Core}:{performanceProfile.Module}");

            Assert.True(allocationProfile.Rule4SharedDataStructures);
            Assert.True(allocationProfile.Rule5SpanBoundaries);
            Assert.True(allocationProfile.Rule6SpanFirstDefaults);
            Assert.True(allocationProfile.UnsafeRulesCompliant);

            Assert.NotEqual(AllocationOptimizationFlags.None, allocationProfile.RecommendedOptimizations);
            Assert.True(allocationProfile.Thresholds.StackAllocMaxElements > 0);
            Assert.True(allocationProfile.Thresholds.ArrayPoolMinElements > 0);
            Assert.True(allocationProfile.Thresholds.WorkspaceReuseMinElements > 0);
        }

        Assert.True(DataStructureCompatibility.TryGetModuleAllocationGcProfile("LinalgCore", "Gemm", out ModuleAllocationGcProfile gemmProfile));
        Assert.True((gemmProfile.CurrentOptimizations & AllocationOptimizationFlags.ThreadLocalScratch) != 0);
        Assert.True((gemmProfile.RecommendedOptimizations & AllocationOptimizationFlags.ThreadLocalScratch) != 0);

        Assert.True(DataStructureCompatibility.TryGetModuleAllocationGcProfile("LinalgCore", "Gemv", out ModuleAllocationGcProfile gemvProfile));
        Assert.True((gemvProfile.CurrentOptimizations & AllocationOptimizationFlags.ThreadLocalScratch) != 0);
        Assert.True((gemvProfile.RecommendedOptimizations & AllocationOptimizationFlags.ThreadLocalScratch) != 0);

        AllocationGcGovernanceSummary summary = DataStructureCompatibility.GetAllocationGcGovernanceSummary();
        Assert.Equal(performanceProfiles.Length, summary.ModuleCount);
        Assert.Equal(0, summary.RuleViolationModules);
        Assert.True(summary.RecommendedStackAllocModules > 0);
        Assert.True(summary.RecommendedArrayPoolModules > 0);
        Assert.True(summary.RecommendedWorkspaceReuseModules > 0);
    }

    [Fact]
    public void CompatibilityPerformanceEntry_LinalgTensorNumerical_Works()
    {
        NDBuffer<double> x = new([4]);
        NDBuffer<double> y = new([4]);
        NDBuffer<double> z = new([4]);

        double[] xSource = [1d, 2d, 3d, 4d];
        double[] ySource = [2d, 3d, 4d, 5d];
        xSource.CopyTo(x.AsSpan());
        ySource.CopyTo(y.AsSpan());

        DataStructureCompatibility.LinalgAxpy(2d, x, y);

        Assert.Equal([4d, 7d, 10d, 13d], y.AsReadOnlySpan().ToArray());

        double dot = DataStructureCompatibility.LinalgDot(x, y);
        Assert.InRange(dot, 100d - 1e-10, 100d + 1e-10);

        DataStructureCompatibility.TensorAdd(x, y, z);
        Assert.Equal([5d, 9d, 13d, 17d], z.AsReadOnlySpan().ToArray());

        double corr = DataStructureCompatibility.NumericalCorrelation(x, y);
        Assert.True(corr > 0.99d);
    }

    [Fact]
    public void CompatibilityPerformanceEntry_OdeEuler_Works()
    {
        NDBuffer<double> y = new([3]);
        NDBuffer<double> yOut = new([3]);
        double[] source = [1d, 2d, 3d];
        source.CopyTo(y.AsSpan());

        DataStructureCompatibility.OdeEulerStep(
            0d,
            0.1d,
            y,
            yOut,
            static (_, state, dydt) =>
            {
                for (int i = 0; i < state.Length; i++)
                {
                    dydt[i] = state[i];
                }
            });

        ReadOnlySpan<double> actual = yOut.AsReadOnlySpan();
        Assert.InRange(actual[0], 1.1d - 1e-12, 1.1d + 1e-12);
        Assert.InRange(actual[1], 2.2d - 1e-12, 2.2d + 1e-12);
        Assert.InRange(actual[2], 3.3d - 1e-12, 3.3d + 1e-12);
    }
}
