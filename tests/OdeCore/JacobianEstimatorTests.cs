using LAL.OdeCore;
using System.Numerics;

namespace LAL.Tests.OdeCore;

public class JacobianEstimatorTests
{
    [Fact]
    public void EstimateForwardDifference_ApproximatesLinearSystemJacobian()
    {
        double[] y = [1.0, 2.0];
        double[] jacobian = new double[4];

        JacobianEstimator.EstimateForwardDifference(
            t: 0.0,
            y: y,
            jacobian: jacobian,
            system: static (_, state, dydt) =>
            {
                dydt[0] = (2.0 * state[0]) + (3.0 * state[1]);
                dydt[1] = (5.0 * state[0]) + (7.0 * state[1]);
            },
            epsilon: 1e-7);

        Assert.InRange(jacobian[0], 2.0 - 1e-4, 2.0 + 1e-4);
        Assert.InRange(jacobian[1], 3.0 - 1e-4, 3.0 + 1e-4);
        Assert.InRange(jacobian[2], 5.0 - 1e-4, 5.0 + 1e-4);
        Assert.InRange(jacobian[3], 7.0 - 1e-4, 7.0 + 1e-4);
    }

    [Fact]
    public void EstimateForwardDifference_ParallelMode_MatchesSequential()
    {
        const int n = 64;
        double[] y = new double[n];
        for (int i = 0; i < n; i++)
        {
            y[i] = 1.0 + (0.01 * i);
        }

        double[] sequential = new double[n * n];
        double[] parallel = new double[n * n];

        OdeSystem system = static (_, state, dydt) =>
        {
            for (int i = 0; i < state.Length; i++)
            {
                dydt[i] = (2.0 * state[i]) + (0.1 * state[(i + 1) % state.Length]);
            }
        };

        JacobianEstimator.EstimateForwardDifference(0.0, y, sequential, system, epsilon: 1e-7, allowParallel: false);
        JacobianEstimator.EstimateForwardDifference(0.0, y, parallel, system, epsilon: 1e-7, allowParallel: true);

        for (int i = 0; i < sequential.Length; i++)
        {
            Assert.InRange(parallel[i], sequential[i] - 1e-8, sequential[i] + 1e-8);
        }
    }

    [Fact]
    public void EstimateForwardDifference_Float_ApproximatesLinearSystemJacobian()
    {
        float[] y = [1f, 2f];
        float[] jacobian = new float[4];

        JacobianEstimator.EstimateForwardDifference(
            t: 0f,
            y: y,
            jacobian: jacobian,
            system: static (_, state, dydt) =>
            {
                dydt[0] = (2f * state[0]) + (3f * state[1]);
                dydt[1] = (5f * state[0]) + (7f * state[1]);
            },
            epsilon: 1e-4f);

        Assert.InRange(jacobian[0], 2f - 2e-2f, 2f + 2e-2f);
        Assert.InRange(jacobian[1], 3f - 2e-2f, 3f + 2e-2f);
        Assert.InRange(jacobian[2], 5f - 2e-2f, 5f + 2e-2f);
        Assert.InRange(jacobian[3], 7f - 2e-2f, 7f + 2e-2f);
    }

    [Fact]
    public void EstimateForwardDifference_Complex_ApproximatesLinearSystemJacobian()
    {
        Complex[] y = [new Complex(1.0, 2.0), new Complex(-1.0, 0.5)];
        Complex[] jacobian = new Complex[4];

        Complex a11 = new(2.0, 1.0);
        Complex a12 = new(-1.0, 0.5);
        Complex a21 = new(0.25, -0.75);
        Complex a22 = new(3.0, -2.0);

        JacobianEstimator.EstimateForwardDifference(
            t: 0.0,
            y: y,
            jacobian: jacobian,
            system: (_, state, dydt) =>
            {
                dydt[0] = (a11 * state[0]) + (a12 * state[1]);
                dydt[1] = (a21 * state[0]) + (a22 * state[1]);
            },
            epsilon: 1e-7);

        Assert.InRange(jacobian[0].Real, a11.Real - 1e-4, a11.Real + 1e-4);
        Assert.InRange(jacobian[0].Imaginary, a11.Imaginary - 1e-4, a11.Imaginary + 1e-4);
        Assert.InRange(jacobian[1].Real, a12.Real - 1e-4, a12.Real + 1e-4);
        Assert.InRange(jacobian[1].Imaginary, a12.Imaginary - 1e-4, a12.Imaginary + 1e-4);
        Assert.InRange(jacobian[2].Real, a21.Real - 1e-4, a21.Real + 1e-4);
        Assert.InRange(jacobian[2].Imaginary, a21.Imaginary - 1e-4, a21.Imaginary + 1e-4);
        Assert.InRange(jacobian[3].Real, a22.Real - 1e-4, a22.Real + 1e-4);
        Assert.InRange(jacobian[3].Imaginary, a22.Imaginary - 1e-4, a22.Imaginary + 1e-4);
    }
}
