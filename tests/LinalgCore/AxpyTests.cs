using System.Numerics;
using LAL.LinalgCore;

namespace LAL.Tests.LinalgCore;

public class AxpyTests
{
    [Fact]
    public void Compute_UpdatesYWithAlphaXPlusY()
    {
        double[] y = [10.0, 20.0, 30.0];

        Axpy.Compute(2.0, [1.0, 2.0, 3.0], y);

        Assert.Equal([12.0, 24.0, 36.0], y);
    }

    [Fact]
    public void Compute_Float_UpdatesYWithAlphaXPlusY()
    {
        float[] y = [10f, 20f, 30f];

        Axpy.Compute(0.5f, [2f, 4f, 6f], y);

        Assert.Equal([11f, 22f, 33f], y);
    }

    [Fact]
    public void Compute_Complex_UpdatesYWithAlphaXPlusY()
    {
        Complex[] y = [new Complex(1, 1), new Complex(2, -1)];
        Complex alpha = new(0, 1);
        Complex[] x = [new Complex(2, 0), new Complex(0, 3)];

        Axpy.Compute(alpha, x, y);

        Assert.Equal(new Complex(1, 3), y[0]);
        Assert.Equal(new Complex(-1, -1), y[1]);
    }
}
