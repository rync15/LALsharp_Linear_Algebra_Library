using System.Numerics;
using LAL.LinalgCore;

namespace LAL.Tests.LinalgCore;

public class LuTests
{
    [Fact]
    public void FactorAndSolve_SolvesLinearSystem()
    {
        double[] a =
        [
            4.0, 3.0,
            6.0, 3.0
        ];

        double[] b = [10.0, 12.0];
        double[] x = new double[2];

        bool ok = Lu.FactorAndSolve(a, n: 2, b, x);

        Assert.True(ok);
        Assert.InRange(x[0], 1.0 - 1e-10, 1.0 + 1e-10);
        Assert.InRange(x[1], 2.0 - 1e-10, 2.0 + 1e-10);
    }

    [Fact]
    public void DecomposeInPlace_ReturnsFalseForSingularMatrix()
    {
        double[] singular =
        [
            1.0, 2.0,
            2.0, 4.0
        ];

        int[] pivots = new int[2];

        LuDecompositionResult result = Lu.DecomposeInPlace(singular, n: 2, pivots, singularTolerance: 1e-12);

        Assert.False(result.Success);
    }

    [Fact]
    public void FactorAndSolve_ReturnsFalseForNearSingularMatrixAtStrictTolerance()
    {
        double[] nearSingular =
        [
            1.0, 1.0,
            1.0, 1.0 + 1e-14
        ];

        double[] b = [2.0, 2.0 + 1e-14];
        double[] x = new double[2];

        bool ok = Lu.FactorAndSolve(nearSingular, n: 2, b, x, singularTolerance: 1e-12);

        Assert.False(ok);
    }

    [Fact]
    public void FactorAndSolve_Complex_SolvesLinearSystem()
    {
        Complex[] a =
        [
            new Complex(2.0, 0.0), new Complex(0.0, 1.0),
            new Complex(0.0, -1.0), new Complex(2.0, 0.0)
        ];

        Complex[] b =
        [
            new Complex(3.0, 4.0),
            new Complex(5.0, -3.0)
        ];
        Complex[] x = new Complex[2];

        bool ok = Lu.FactorAndSolve(a, n: 2, b, x);

        Assert.True(ok);
        Assert.InRange(Complex.Abs(x[0] - new Complex(1.0, 1.0)), 0d, 1e-10);
        Assert.InRange(Complex.Abs(x[1] - new Complex(2.0, -1.0)), 0d, 1e-10);
    }
}
