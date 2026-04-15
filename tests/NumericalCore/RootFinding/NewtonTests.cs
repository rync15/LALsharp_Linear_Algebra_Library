using System.Numerics;
using LAL.NumericalCore.RootFinding;

namespace LAL.Tests.NumericalCore.RootFinding;

public class NewtonTests
{
    [Fact]
    public void Solve_FindsSquareRootOfTwo()
    {
        NewtonResult result = Newton.Solve(
            f: static x => (x * x) - 2.0,
            df: static x => 2.0 * x,
            initialGuess: 1.0,
            tolerance: 1e-12,
            maxIterations: 100);

        Assert.True(result.Converged);
        Assert.InRange(result.Root, Math.Sqrt(2.0) - 1e-9, Math.Sqrt(2.0) + 1e-9);
    }

    [Fact]
    public void Solve_Float_FindsSquareRootOfTwo()
    {
        NewtonResultFloat result = Newton.Solve(
            f: static x => (x * x) - 2f,
            df: static x => 2f * x,
            initialGuess: 1f,
            tolerance: 1e-5f,
            maxIterations: 100);

        Assert.True(result.Converged);
        Assert.InRange(result.Root, (float)Math.Sqrt(2.0) - 1e-3f, (float)Math.Sqrt(2.0) + 1e-3f);
    }

    [Fact]
    public void Solve_Complex_FindsImaginaryUnitRoot()
    {
        NewtonResultComplex result = Newton.Solve(
            f: static z => (z * z) + Complex.One,
            df: static z => 2d * z,
            initialGuess: new Complex(0.4, 0.8),
            tolerance: 1e-10,
            maxIterations: 100);

        Assert.True(result.Converged);
        Assert.InRange(Complex.Abs((result.Root * result.Root) + Complex.One), 0d, 1e-8);
    }
}
