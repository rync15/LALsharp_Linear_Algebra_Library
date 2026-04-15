using System.Numerics;
using LAL.LinalgCore;

namespace LAL.Tests.LinalgCore;

public class EigenSolverTests
{
    [Fact]
    public void PowerIteration_FindsDominantEigenvalue()
    {
        double[] matrix =
        [
            3.0, 0.0,
            0.0, 1.0
        ];
        double[] eigenvector = new double[2];

        EigenResult result = EigenSolver.PowerIteration(matrix, n: 2, eigenvector, tolerance: 1e-12, maxIterations: 200);

        Assert.True(result.Converged);
        Assert.InRange(result.Eigenvalue, 3.0 - 1e-8, 3.0 + 1e-8);
        Assert.True(Math.Abs(eigenvector[0]) > Math.Abs(eigenvector[1]));
    }

    [Fact]
    public void PowerIteration_Complex_FindsDominantEigenvalue()
    {
        Complex[] matrix =
        [
            new Complex(2.0, 1.0), Complex.Zero,
            Complex.Zero, new Complex(1.0, -1.0)
        ];
        Complex[] eigenvector = new Complex[2];

        ComplexEigenResult result = EigenSolver.PowerIteration(matrix, n: 2, eigenvector, tolerance: 1e-12, maxIterations: 300);

        Assert.True(result.Converged);
        Assert.InRange(Complex.Abs(result.Eigenvalue - new Complex(2.0, 1.0)), 0d, 1e-6);
        Assert.True(Complex.Abs(eigenvector[0]) > Complex.Abs(eigenvector[1]));
    }
}
