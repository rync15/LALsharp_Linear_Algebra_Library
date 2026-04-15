using System.Numerics;
using LAL.LinalgCore;

namespace LAL.Tests.LinalgCore;

public class DenseSolverTests
{
    [Fact]
    public void Solve_ComputesSolution()
    {
        double[] a =
        [
            4.0, 3.0,
            6.0, 3.0
        ];
        double[] b = [10.0, 12.0];
        double[] x = new double[2];

        bool ok = DenseSolver.Solve(a, n: 2, b, x);

        Assert.True(ok);
        Assert.InRange(x[0], 1.0 - 1e-10, 1.0 + 1e-10);
        Assert.InRange(x[1], 2.0 - 1e-10, 2.0 + 1e-10);
    }

    [Fact]
    public void Solve_Complex_ComputesSolution()
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

        bool ok = DenseSolver.Solve(a, n: 2, b, x);

        Assert.True(ok);
        Assert.InRange(Complex.Abs(x[0] - new Complex(1.0, 1.0)), 0d, 1e-10);
        Assert.InRange(Complex.Abs(x[1] - new Complex(2.0, -1.0)), 0d, 1e-10);
    }
}
