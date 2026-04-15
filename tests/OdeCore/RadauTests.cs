using LAL.OdeCore;
using System.Numerics;

namespace LAL.Tests.OdeCore;

public class RadauTests
{
    [Fact]
    public void StepOneStage_ProducesStableAdvance()
    {
        double[] y = [1.0];
        double[] yOut = new double[1];

        RadauStepResult result = Radau.StepOneStage(
            t: 0.0,
            dt: 0.1,
            y,
            yOut,
            static (_, state, dydt) =>
            {
                dydt[0] = -state[0];
            },
            maxIterations: 16,
            tolerance: 1e-12);

        Assert.InRange(yOut[0], 0.85, 0.95);
        Assert.True(result.Iterations >= 1);
    }

    [Fact]
    public void StepOneStage_StaysBoundedOnStiffProblem()
    {
        double[] y = [1.0];
        double[] yOut = new double[1];

        RadauStepResult result = Radau.StepOneStage(
            t: 0.0,
            dt: 0.01,
            y,
            yOut,
            static (_, state, dydt) =>
            {
                dydt[0] = -100.0 * state[0];
            },
            maxIterations: 32,
            tolerance: 1e-12);

        Assert.InRange(yOut[0], 0.2, 0.6);
        Assert.True(result.Iterations >= 1);
    }

    [Fact]
    public void StepOneStage_Float_ProducesStableAdvance()
    {
        float[] y = [1f];
        float[] yOut = new float[1];

        RadauStepResultFloat result = Radau.StepOneStage(
            t: 0f,
            dt: 0.1f,
            y,
            yOut,
            static (_, state, dydt) =>
            {
                dydt[0] = -state[0];
            },
            maxIterations: 16,
            tolerance: 1e-6f);

        Assert.InRange(yOut[0], 0.85f, 0.95f);
        Assert.True(result.Iterations >= 1);
    }

    [Fact]
    public void StepOneStage_Complex_ProducesStableAdvance()
    {
        Complex[] y = [new Complex(1.0, 1.0)];
        Complex[] yOut = new Complex[1];

        RadauStepResult result = Radau.StepOneStage(
            t: 0.0,
            dt: 0.1,
            y,
            yOut,
            static (_, state, dydt) =>
            {
                dydt[0] = -state[0];
            },
            maxIterations: 16,
            tolerance: 1e-10);

        Assert.InRange(yOut[0].Real, 0.85, 0.95);
        Assert.InRange(yOut[0].Imaginary, 0.85, 0.95);
        Assert.True(result.Iterations >= 1);
    }
}
