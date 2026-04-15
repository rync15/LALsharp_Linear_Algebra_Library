using System.Numerics;
using LAL.NumericalCore.Optimization;

namespace LAL.Tests.NumericalCore.Optimization;

public class GradientDescentTests
{
    [Fact]
    public void SolveScalar_ConvergesOnQuadraticMinimum()
    {
        GradientDescentResult result = GradientDescent.SolveScalar(
            f: static x => (x - 3.0) * (x - 3.0),
            gradient: static x => 2.0 * (x - 3.0),
            initialX: 0.0,
            learningRate: 0.1,
            tolerance: 1e-10,
            maxIterations: 5000);

        Assert.True(result.Converged);
        Assert.InRange(result.X, 3.0 - 1e-6, 3.0 + 1e-6);
        Assert.InRange(result.Fx, 0.0, 1e-10);
    }

    [Fact]
    public void SolveScalar_Float_ConvergesOnQuadraticMinimum()
    {
        GradientDescentResultFloat result = GradientDescent.SolveScalar(
            f: static x => (x - 3f) * (x - 3f),
            gradient: static x => 2f * (x - 3f),
            initialX: 0f,
            learningRate: 0.1f,
            tolerance: 1e-5f,
            maxIterations: 5000);

        Assert.True(result.Converged);
        Assert.InRange(result.X, 3f - 1e-2f, 3f + 1e-2f);
    }

    [Fact]
    public void SolveScalar_Complex_ConvergesOnQuadraticMinimum()
    {
        GradientDescentComplexResult result = GradientDescent.SolveScalar(
            f: static z => Complex.Abs(z - new Complex(3, -2)) * Complex.Abs(z - new Complex(3, -2)),
            gradient: static z => 2d * (z - new Complex(3, -2)),
            initialX: Complex.Zero,
            learningRate: 0.1,
            tolerance: 1e-8,
            maxIterations: 5000);

        Assert.True(result.Converged);
        Assert.InRange(result.X.Real, 3.0 - 1e-5, 3.0 + 1e-5);
        Assert.InRange(result.X.Imaginary, -2.0 - 1e-5, -2.0 + 1e-5);
        Assert.InRange(result.Fx, 0.0, 1e-8);
    }
}
