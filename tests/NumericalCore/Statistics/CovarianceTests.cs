using System.Numerics;
using LAL.NumericalCore.Statistics;

namespace LAL.Tests.NumericalCore.Statistics;

public class CovarianceTests
{
    [Fact]
    public void ComputeAndCorrelation_Work()
    {
        double[] x = [1.0, 2.0, 3.0];
        double[] y = [2.0, 4.0, 6.0];

        double covariance = Covariance.Compute(x, y, sample: true);
        double correlation = Covariance.Correlation(x, y);

        Assert.InRange(covariance, 2.0 - 1e-12, 2.0 + 1e-12);
        Assert.InRange(correlation, 1.0 - 1e-12, 1.0 + 1e-12);
    }

    [Fact]
    public void Correlation_RemainsHighWithSmallDeterministicNoise()
    {
        const int n = 100;
        double[] x = new double[n];
        double[] y = new double[n];

        for (int i = 0; i < n; i++)
        {
            x[i] = i + 1;
            double noise = ((i % 5) - 2) * 0.01;
            y[i] = (2.0 * x[i]) + noise;
        }

        double correlation = Covariance.Correlation(x, y);

        Assert.InRange(correlation, 0.999, 1.0);
    }

    [Fact]
    public void Compute_WithParallelEnabled_MatchesSequentialOnLargeInput()
    {
        const int n = 40_000;
        double[] x = new double[n];
        double[] y = new double[n];

        for (int i = 0; i < n; i++)
        {
            x[i] = i * 0.001;
            y[i] = (3.0 * x[i]) + ((i % 7) - 3) * 0.0001;
        }

        double covarianceSequential = Covariance.Compute(x, y, sample: true, allowParallel: false);
        double covarianceParallel = Covariance.Compute(x, y, sample: true, allowParallel: true);

        Assert.InRange(covarianceParallel, covarianceSequential - 1e-9, covarianceSequential + 1e-9);

        double correlationSequential = Covariance.Correlation(x, y, allowParallel: false);
        double correlationParallel = Covariance.Correlation(x, y, allowParallel: true);

        Assert.InRange(correlationParallel, correlationSequential - 1e-9, correlationSequential + 1e-9);
    }

    [Fact]
    public void ComputeAndCorrelation_Float_Work()
    {
        float[] x = [1f, 2f, 3f];
        float[] y = [2f, 4f, 6f];

        float covariance = Covariance.Compute(x, y, sample: true);
        float correlation = Covariance.Correlation(x, y);

        Assert.InRange(covariance, 2f - 1e-4f, 2f + 1e-4f);
        Assert.InRange(correlation, 1f - 1e-4f, 1f + 1e-4f);
    }

    [Fact]
    public void ComputeAndCorrelation_Complex_Work()
    {
        Complex[] x = [new Complex(1, 1), new Complex(2, 1), new Complex(3, 1)];
        Complex[] y = [new Complex(2, -1), new Complex(4, -1), new Complex(6, -1)];

        Complex covariance = Covariance.Compute(x, y, sample: true);
        Complex correlation = Covariance.Correlation(x, y);

        Assert.True(covariance.Real > 0d);
        Assert.InRange(correlation.Real, 1.0 - 1e-12, 1.0 + 1e-12);
        Assert.InRange(correlation.Imaginary, -1e-12, 1e-12);
    }

    [Fact]
    public void Compute_Float_WithParallelEnabled_MatchesSequentialOnLargeInput()
    {
        const int n = 40_000;
        float[] x = new float[n];
        float[] y = new float[n];

        for (int i = 0; i < n; i++)
        {
            x[i] = i * 0.001f;
            y[i] = (2.5f * x[i]) + (((i % 7) - 3) * 0.0001f);
        }

        float covarianceSequential = Covariance.Compute(x, y, sample: true, allowParallel: false);
        float covarianceParallel = Covariance.Compute(x, y, sample: true, allowParallel: true);

        Assert.InRange(covarianceParallel, covarianceSequential - 1e-4f, covarianceSequential + 1e-4f);

        float correlationSequential = Covariance.Correlation(x, y, allowParallel: false);
        float correlationParallel = Covariance.Correlation(x, y, allowParallel: true);

        Assert.InRange(correlationParallel, correlationSequential - 1e-4f, correlationSequential + 1e-4f);
    }

    [Fact]
    public void Compute_Complex_WithParallelEnabled_MatchesSequentialOnLargeInput()
    {
        const int n = 40_000;
        Complex[] x = new Complex[n];
        Complex[] y = new Complex[n];

        for (int i = 0; i < n; i++)
        {
            double t = i * 0.001;
            x[i] = new Complex(t, 0.5 * t);
            y[i] = new Complex((2.0 * t) + ((i % 5) * 0.001), (-0.3 * t) + ((i % 3) * 0.001));
        }

        Complex covarianceSequential = Covariance.Compute(x, y, sample: true, allowParallel: false);
        Complex covarianceParallel = Covariance.Compute(x, y, sample: true, allowParallel: true);

        Assert.InRange(covarianceParallel.Real, covarianceSequential.Real - 1e-7, covarianceSequential.Real + 1e-7);
        Assert.InRange(covarianceParallel.Imaginary, covarianceSequential.Imaginary - 1e-7, covarianceSequential.Imaginary + 1e-7);

        Complex correlationSequential = Covariance.Correlation(x, y, allowParallel: false);
        Complex correlationParallel = Covariance.Correlation(x, y, allowParallel: true);

        Assert.InRange(correlationParallel.Real, correlationSequential.Real - 1e-7, correlationSequential.Real + 1e-7);
        Assert.InRange(correlationParallel.Imaginary, correlationSequential.Imaginary - 1e-7, correlationSequential.Imaginary + 1e-7);
    }
}
