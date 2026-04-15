using System.Numerics;
using LAL.NumericalCore.RootFinding;

namespace LAL.Tests.NumericalCore.RootFinding;

public class SecantTests
{
    [Fact]
    public void Solve_FindsSquareRootOfTwo()
    {
        SecantResult result = Secant.Solve(
            static x => (x * x) - 2.0,
            x0: 1.0,
            x1: 2.0,
            tolerance: 1e-12,
            maxIterations: 100);

        Assert.True(result.Converged);
        Assert.InRange(result.Root, Math.Sqrt(2.0) - 1e-9, Math.Sqrt(2.0) + 1e-9);
    }

    [Fact]
    public void Solve_ReturnsNotConvergedWhenDenominatorDegenerates()
    {
        SecantResult result = Secant.Solve(
            static _ => 1.0,
            x0: 0.0,
            x1: 1.0,
            tolerance: 1e-12,
            maxIterations: 10);

        Assert.False(result.Converged);
    }

    [Fact]
    public void Solve_Float_FindsSquareRootOfTwo()
    {
        SecantResultFloat result = Secant.Solve(
            static x => (x * x) - 2f,
            x0: 1f,
            x1: 2f,
            tolerance: 1e-5f,
            maxIterations: 100);

        Assert.True(result.Converged);
        Assert.InRange(result.Root, (float)Math.Sqrt(2.0) - 1e-3f, (float)Math.Sqrt(2.0) + 1e-3f);
    }

    [Fact]
    public void Solve_Complex_FindsImaginaryUnitRoot()
    {
        SecantResultComplex result = Secant.Solve(
            static z => (z * z) + Complex.One,
            x0: new Complex(0.3, 0.9),
            x1: new Complex(0.1, 1.2),
            tolerance: 1e-10,
            maxIterations: 100);

        Assert.True(result.Converged);
        Assert.InRange(Complex.Abs((result.Root * result.Root) + Complex.One), 0d, 1e-8);
    }
}
