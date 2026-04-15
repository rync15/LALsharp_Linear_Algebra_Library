using LAL.OdeCore;
using System.Numerics;

namespace LAL.Tests.OdeCore;

public class EulerTests
{
    [Fact]
    public void Step_AdvancesStateForLinearSystem()
    {
        double[] y = [1.0];
        double[] yOut = new double[1];

        Euler.Step(0.0, 0.1, y, yOut, static (_, state, dydt) =>
        {
            dydt[0] = state[0];
        });

        Assert.InRange(yOut[0], 1.099999, 1.100001);
    }

    [Fact]
    public void Step_Float_AdvancesStateForLinearSystem()
    {
        float[] y = [1f];
        float[] yOut = new float[1];

        Euler.Step(0f, 0.1f, y, yOut, static (_, state, dydt) =>
        {
            dydt[0] = state[0];
        });

        Assert.InRange(yOut[0], 1.0999f, 1.1001f);
    }

    [Fact]
    public void Step_Complex_AdvancesStateForLinearSystem()
    {
        Complex[] y = [new Complex(1.0, 1.0)];
        Complex[] yOut = new Complex[1];

        Euler.Step(0.0, 0.1, y, yOut, static (_, state, dydt) =>
        {
            dydt[0] = state[0];
        });

        Assert.InRange(yOut[0].Real, 1.1 - 1e-10, 1.1 + 1e-10);
        Assert.InRange(yOut[0].Imaginary, 1.1 - 1e-10, 1.1 + 1e-10);
    }
}
