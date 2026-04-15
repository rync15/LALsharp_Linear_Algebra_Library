using LAL.OdeCore;
using System.Numerics;

namespace LAL.Tests.OdeCore;

public class Rk45Tests
{
    [Fact]
    public void Step_ProducesAccurateEstimateForExpGrowth()
    {
        double[] y = [1.0];
        double[] yOut = new double[1];

        Rk45StepResult result = Rk45.Step(0.0, 0.1, y, yOut, static (_, state, dydt) =>
        {
            dydt[0] = state[0];
        });

        Assert.InRange(yOut[0], Math.Exp(0.1) - 1e-6, Math.Exp(0.1) + 1e-6);
        Assert.True(result.EstimatedError >= 0.0);
    }

    [Fact]
    public void Step_Float_ProducesAccurateEstimateForExpGrowth()
    {
        float[] y = [1f];
        float[] yOut = new float[1];

        Rk45StepResultFloat result = Rk45.Step(0f, 0.1f, y, yOut, static (_, state, dydt) =>
        {
            dydt[0] = state[0];
        });

        float expected = MathF.Exp(0.1f);
        Assert.InRange(yOut[0], expected - 1e-4f, expected + 1e-4f);
        Assert.True(result.EstimatedError >= 0f);
    }

    [Fact]
    public void Step_Complex_ProducesAccurateEstimateForExpGrowth()
    {
        Complex[] y = [new Complex(1.0, 1.0)];
        Complex[] yOut = new Complex[1];

        Rk45StepResult result = Rk45.Step(0.0, 0.1, y, yOut, static (_, state, dydt) =>
        {
            dydt[0] = state[0];
        });

        double expected = Math.Exp(0.1);
        Assert.InRange(yOut[0].Real, expected - 1e-6, expected + 1e-6);
        Assert.InRange(yOut[0].Imaginary, expected - 1e-6, expected + 1e-6);
        Assert.True(result.EstimatedError >= 0.0);
    }
}
