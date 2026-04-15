using LAL.OdeCore;
using System.Numerics;

namespace LAL.Tests.OdeCore;

public class DenseOutputTests
{
    [Fact]
    public void InterpolateLinear_ReturnsExpectedPoint()
    {
        double[] y0 = [0.0, 2.0];
        double[] y1 = [2.0, 4.0];
        double[] yOut = new double[2];

        DenseOutput.InterpolateLinear(y0, y1, theta: 0.25, yOut);

        Assert.Equal(new double[] { 0.5, 2.5 }, yOut);
    }

    [Fact]
    public void InterpolateLinear_Float_ReturnsExpectedPoint()
    {
        float[] y0 = [0f, 2f];
        float[] y1 = [2f, 4f];
        float[] yOut = new float[2];

        DenseOutput.InterpolateLinear(y0, y1, theta: 0.25f, yOut);

        Assert.InRange(yOut[0], 0.5f - 1e-6f, 0.5f + 1e-6f);
        Assert.InRange(yOut[1], 2.5f - 1e-6f, 2.5f + 1e-6f);
    }

    [Fact]
    public void InterpolateLinear_Complex_ReturnsExpectedPoint()
    {
        Complex[] y0 = [new Complex(0, 0), new Complex(2, -1)];
        Complex[] y1 = [new Complex(2, 2), new Complex(4, 1)];
        Complex[] yOut = new Complex[2];

        DenseOutput.InterpolateLinear(y0, y1, theta: 0.25, yOut);

        Assert.InRange(yOut[0].Real, 0.5 - 1e-12, 0.5 + 1e-12);
        Assert.InRange(yOut[0].Imaginary, 0.5 - 1e-12, 0.5 + 1e-12);
        Assert.InRange(yOut[1].Real, 2.5 - 1e-12, 2.5 + 1e-12);
        Assert.InRange(yOut[1].Imaginary, -0.5 - 1e-12, -0.5 + 1e-12);
    }
}
