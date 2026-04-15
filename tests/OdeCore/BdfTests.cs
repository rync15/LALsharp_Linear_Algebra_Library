using LAL.OdeCore;
using System.Numerics;

namespace LAL.Tests.OdeCore;

public class BdfTests
{
    [Fact]
    public void StepBackwardEuler_ApproximatesImplicitStep()
    {
        double[] y = [1.0];
        double[] yOut = new double[1];

        BdfStepResult result = Bdf.StepBackwardEuler(
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

        Assert.InRange(yOut[0], 0.90909 - 1e-4, 0.90909 + 1e-4);
        Assert.True(result.Iterations >= 1);
    }

    [Fact]
    public void StepBackwardEuler_RemainsStableForStiffDecayWithSmallStep()
    {
        double[] y = [1.0];
        double[] yOut = new double[1];

        BdfStepResult result = Bdf.StepBackwardEuler(
            t: 0.0,
            dt: 0.01,
            y,
            yOut,
            static (_, state, dydt) =>
            {
                dydt[0] = -50.0 * state[0];
            },
            maxIterations: 32,
            tolerance: 1e-12);

        Assert.InRange(yOut[0], 0.64, 0.69);
        Assert.InRange(result.Residual, 0.0, 1e-8);
    }

    [Fact]
    public void StepBackwardEuler_Float_ApproximatesImplicitStep()
    {
        float[] y = [1f];
        float[] yOut = new float[1];

        BdfStepResultFloat result = Bdf.StepBackwardEuler(
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

        Assert.InRange(yOut[0], 0.90909f - 1e-3f, 0.90909f + 1e-3f);
        Assert.True(result.Iterations >= 1);
    }

    [Fact]
    public void StepBackwardEuler_Complex_ApproximatesImplicitStep()
    {
        Complex[] y = [new Complex(1.0, 1.0)];
        Complex[] yOut = new Complex[1];

        BdfStepResult result = Bdf.StepBackwardEuler(
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

        Assert.InRange(yOut[0].Real, 0.90909 - 1e-4, 0.90909 + 1e-4);
        Assert.InRange(yOut[0].Imaginary, 0.90909 - 1e-4, 0.90909 + 1e-4);
        Assert.True(result.Iterations >= 1);
    }
}
