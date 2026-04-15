using LAL.OdeCore;
using System.Numerics;

namespace LAL.Tests.OdeCore;

public class Rk4Tests
{
    [Fact]
    public void Step_ProducesFourthOrderEstimate()
    {
        double[] y = [1.0];
        double[] yOut = new double[1];

        Rk4.Step(0.0, 0.1, y, yOut, static (_, state, dydt) =>
        {
            dydt[0] = state[0];
        });

        Assert.InRange(yOut[0], 1.10517 - 1e-5, 1.10517 + 1e-5);
    }

    [Fact]
    public void Step_Float_ProducesFourthOrderEstimate()
    {
        float[] y = [1f];
        float[] yOut = new float[1];

        Rk4.Step(0f, 0.1f, y, yOut, static (_, state, dydt) =>
        {
            dydt[0] = state[0];
        });

        Assert.InRange(yOut[0], 1.10517f - 1e-4f, 1.10517f + 1e-4f);
    }

    [Fact]
    public void Step_Complex_ProducesFourthOrderEstimate()
    {
        Complex[] y = [new Complex(1.0, 1.0)];
        Complex[] yOut = new Complex[1];

        Rk4.Step(0.0, 0.1, y, yOut, static (_, state, dydt) =>
        {
            dydt[0] = state[0];
        });

        double expected = Math.Exp(0.1);
        Assert.InRange(yOut[0].Real, expected - 1e-5, expected + 1e-5);
        Assert.InRange(yOut[0].Imaginary, expected - 1e-5, expected + 1e-5);
    }
}
