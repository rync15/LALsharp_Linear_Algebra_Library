using System.Numerics;
using LAL.NumericalCore.Interpolation;

namespace LAL.Tests.NumericalCore.Interpolation;

public class SplineTests
{
    [Fact]
    public void NaturalCubic_ReproducesLinearData()
    {
        double[] xs = [0.0, 1.0, 2.0];
        double[] ys = [0.0, 1.0, 2.0];
        double[] second = new double[3];

        Spline.ComputeNaturalSecondDerivatives(xs, ys, second);

        Assert.InRange(Math.Abs(second[0]), 0, 1e-12);
        Assert.InRange(Math.Abs(second[1]), 0, 1e-12);
        Assert.InRange(Math.Abs(second[2]), 0, 1e-12);

        double value = Spline.EvaluateNaturalCubic(xs, ys, second, x: 1.5);
        Assert.InRange(value, 1.5 - 1e-10, 1.5 + 1e-10);
    }

    [Fact]
    public void NaturalCubic_Float_ReproducesLinearData()
    {
        float[] xs = [0f, 1f, 2f];
        float[] ys = [0f, 1f, 2f];
        float[] second = new float[3];

        Spline.ComputeNaturalSecondDerivatives(xs, ys, second);

        float value = Spline.EvaluateNaturalCubic(xs, ys, second, x: 1.5f);
        Assert.InRange(value, 1.5f - 1e-4f, 1.5f + 1e-4f);
    }

    [Fact]
    public void NaturalCubic_ComplexValues_ReproducesLinearData()
    {
        double[] xs = [0.0, 1.0, 2.0];
        Complex[] ys = [new Complex(0, 0), new Complex(1, 2), new Complex(2, 4)];
        Complex[] second = new Complex[3];

        Spline.ComputeNaturalSecondDerivatives(xs, ys, second);

        Complex value = Spline.EvaluateNaturalCubic(xs, ys, second, x: 1.5);
        Assert.InRange(value.Real, 1.5 - 1e-10, 1.5 + 1e-10);
        Assert.InRange(value.Imaginary, 3.0 - 1e-10, 3.0 + 1e-10);
    }
}
