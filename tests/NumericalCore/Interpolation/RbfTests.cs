using System.Numerics;
using LAL.NumericalCore.Interpolation;

namespace LAL.Tests.NumericalCore.Interpolation;

public class RbfTests
{
    [Fact]
    public void ComputeWeights_InterpolatesCenters()
    {
        double[] centers = [0.0, 1.0];
        double[] values = [1.0, 2.0];
        double[] weights = new double[2];

        bool ok = Rbf.ComputeGaussianWeights(centers, values, epsilon: 1.0, weights);

        Assert.True(ok);

        double at0 = Rbf.EvaluateGaussian(centers, weights, x: 0.0, epsilon: 1.0);
        double at1 = Rbf.EvaluateGaussian(centers, weights, x: 1.0, epsilon: 1.0);

        Assert.InRange(at0, 1.0 - 1e-8, 1.0 + 1e-8);
        Assert.InRange(at1, 2.0 - 1e-8, 2.0 + 1e-8);
    }

    [Fact]
    public void ComputeWeights_Float_InterpolatesCenters()
    {
        float[] centers = [0f, 1f];
        float[] values = [1f, 2f];
        float[] weights = new float[2];

        bool ok = Rbf.ComputeGaussianWeights(centers, values, epsilon: 1f, weights);

        Assert.True(ok);

        float at0 = Rbf.EvaluateGaussian(centers, weights, x: 0f, epsilon: 1f);
        float at1 = Rbf.EvaluateGaussian(centers, weights, x: 1f, epsilon: 1f);

        Assert.InRange(at0, 1f - 1e-4f, 1f + 1e-4f);
        Assert.InRange(at1, 2f - 1e-4f, 2f + 1e-4f);
    }

    [Fact]
    public void ComputeWeights_Complex_InterpolatesCenters()
    {
        double[] centers = [0.0, 1.0];
        Complex[] values = [new Complex(1, 1), new Complex(2, -1)];
        Complex[] weights = new Complex[2];

        bool ok = Rbf.ComputeGaussianWeights(centers, values, epsilon: 1.0, weights);

        Assert.True(ok);

        Complex at0 = Rbf.EvaluateGaussian(centers, weights, x: 0.0, epsilon: 1.0);
        Complex at1 = Rbf.EvaluateGaussian(centers, weights, x: 1.0, epsilon: 1.0);

        Assert.InRange(at0.Real, 1.0 - 1e-8, 1.0 + 1e-8);
        Assert.InRange(at0.Imaginary, 1.0 - 1e-8, 1.0 + 1e-8);
        Assert.InRange(at1.Real, 2.0 - 1e-8, 2.0 + 1e-8);
        Assert.InRange(at1.Imaginary, -1.0 - 1e-8, -1.0 + 1e-8);
    }

    [Fact]
    public void ComputeWeights_Double_ParallelMatchesSequential()
    {
        const int n = 96;
        double[] centers = new double[n];
        double[] values = new double[n];
        double[] weightsSequential = new double[n];
        double[] weightsParallel = new double[n];

        for (int i = 0; i < n; i++)
        {
            centers[i] = i * 0.5;
            values[i] = Math.Sin(centers[i]);
        }

        bool okSequential = Rbf.ComputeGaussianWeights(centers, values, epsilon: 5.0, weightsSequential, allowParallel: false);
        bool okParallel = Rbf.ComputeGaussianWeights(centers, values, epsilon: 5.0, weightsParallel, allowParallel: true);

        Assert.True(okSequential);
        Assert.True(okParallel);

        for (int i = 0; i < n; i++)
        {
            Assert.InRange(weightsParallel[i], weightsSequential[i] - 1e-7, weightsSequential[i] + 1e-7);
        }
    }

    [Fact]
    public void ComputeWeights_Float_ParallelMatchesSequential()
    {
        const int n = 96;
        float[] centers = new float[n];
        float[] values = new float[n];
        float[] weightsSequential = new float[n];
        float[] weightsParallel = new float[n];

        for (int i = 0; i < n; i++)
        {
            centers[i] = i * 0.5f;
            values[i] = MathF.Sin(centers[i]);
        }

        bool okSequential = Rbf.ComputeGaussianWeights(centers, values, epsilon: 5f, weightsSequential, allowParallel: false);
        bool okParallel = Rbf.ComputeGaussianWeights(centers, values, epsilon: 5f, weightsParallel, allowParallel: true);

        Assert.True(okSequential);
        Assert.True(okParallel);

        for (int i = 0; i < n; i++)
        {
            Assert.InRange(weightsParallel[i], weightsSequential[i] - 1e-4f, weightsSequential[i] + 1e-4f);
        }
    }

    [Fact]
    public void ComputeWeights_Complex_ParallelMatchesSequential()
    {
        const int n = 96;
        double[] centers = new double[n];
        Complex[] values = new Complex[n];
        Complex[] weightsSequential = new Complex[n];
        Complex[] weightsParallel = new Complex[n];

        for (int i = 0; i < n; i++)
        {
            centers[i] = i * 0.5;
            values[i] = new Complex(Math.Cos(centers[i]), Math.Sin(centers[i]));
        }

        bool okSequential = Rbf.ComputeGaussianWeights(centers, values, epsilon: 5.0, weightsSequential, allowParallel: false);
        bool okParallel = Rbf.ComputeGaussianWeights(centers, values, epsilon: 5.0, weightsParallel, allowParallel: true);

        Assert.True(okSequential);
        Assert.True(okParallel);

        for (int i = 0; i < n; i++)
        {
            Assert.InRange(weightsParallel[i].Real, weightsSequential[i].Real - 1e-6, weightsSequential[i].Real + 1e-6);
            Assert.InRange(weightsParallel[i].Imaginary, weightsSequential[i].Imaginary - 1e-6, weightsSequential[i].Imaginary + 1e-6);
        }
    }
}
