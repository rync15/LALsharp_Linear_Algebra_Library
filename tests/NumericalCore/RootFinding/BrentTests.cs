using System.Numerics;
using LAL.NumericalCore.RootFinding;

namespace LAL.Tests.NumericalCore.RootFinding;

public class BrentTests
{
    [Fact]
    public void Solve_FindsRootWithinBracket()
    {
        BrentResult result = Brent.Solve(
            f: static x => (x * x * x) - x - 2.0,
            lower: 1.0,
            upper: 2.0,
            tolerance: 1e-12,
            maxIterations: 200);

        Assert.True(result.Converged);
        Assert.InRange(result.Root, 1.5213797 - 1e-7, 1.5213797 + 1e-7);
    }

    [Fact]
    public void Solve_Float_FindsRootWithinBracket()
    {
        BrentResultFloat result = Brent.Solve(
            f: static x => (x * x * x) - x - 2f,
            lower: 1f,
            upper: 2f,
            tolerance: 1e-5f,
            maxIterations: 200);

        Assert.True(result.Converged);
        Assert.InRange(result.Root, 1.5213797f - 1e-3f, 1.5213797f + 1e-3f);
    }

    [Fact]
    public void Solve_Complex_FindsImaginaryUnitRoot()
    {
        BrentResultComplex result = Brent.Solve(
            f: static z => (z * z) + Complex.One,
            x0: new Complex(0.3, 0.9),
            x1: new Complex(0.1, 1.2),
            tolerance: 1e-10,
            maxIterations: 100);

        Assert.True(result.Converged);
        Assert.InRange(Complex.Abs((result.Root * result.Root) + Complex.One), 0d, 1e-8);
    }
}
