using System.Numerics;
using LAL.NumericalCore.Optimization;

namespace LAL.Tests.NumericalCore.Optimization;

public class LbfgsTests
{
    [Fact]
    public void SolveScalar_ConvergesOnQuadratic()
    {
        LbfgsResult result = Lbfgs.SolveScalar(
            static x => (x - 3.0) * (x - 3.0),
            static x => 2.0 * (x - 3.0),
            initialX: 10.0,
            tolerance: 1e-10,
            maxIterations: 200);

        Assert.True(result.Converged);
        Assert.InRange(result.X, 3.0 - 1e-8, 3.0 + 1e-8);
        Assert.InRange(result.Fx, 0.0, 1e-12);
    }

    [Fact]
    public void SolveScalar_Float_ConvergesOnQuadratic()
    {
        LbfgsResultFloat result = Lbfgs.SolveScalar(
            static x => (x - 3f) * (x - 3f),
            static x => 2f * (x - 3f),
            initialX: 10f,
            tolerance: 1e-5f,
            maxIterations: 200);

        Assert.True(result.Converged);
        Assert.InRange(result.X, 3f - 1e-3f, 3f + 1e-3f);
        Assert.InRange(result.Fx, 0f, 1e-4f);
    }

    [Fact]
    public void SolveScalar_Complex_ConvergesOnQuadratic()
    {
        LbfgsComplexResult result = Lbfgs.SolveScalar(
            static z => Complex.Abs(z - new Complex(3, -2)) * Complex.Abs(z - new Complex(3, -2)),
            static z => 2d * (z - new Complex(3, -2)),
            initialX: new Complex(10, 10),
            tolerance: 1e-8,
            maxIterations: 200);

        Assert.True(result.Converged);
        Assert.InRange(result.X.Real, 3.0 - 1e-5, 3.0 + 1e-5);
        Assert.InRange(result.X.Imaginary, -2.0 - 1e-5, -2.0 + 1e-5);
        Assert.InRange(result.Fx, 0.0, 1e-8);
    }
}
