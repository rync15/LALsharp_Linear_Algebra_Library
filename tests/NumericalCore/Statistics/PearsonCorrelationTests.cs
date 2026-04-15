using System.Numerics;
using LAL.NumericalCore.Statistics;

namespace LAL.Tests.NumericalCore.Statistics;

public class PearsonCorrelationTests
{
    [Fact]
    public void Compute_ReturnsOneForPerfectPositiveCorrelation()
    {
        double[] x = [1.0, 2.0, 3.0, 4.0];
        double[] y = [2.0, 4.0, 6.0, 8.0];

        double correlation = PearsonCorrelation.Compute(x, y);

        Assert.InRange(correlation, 1.0 - 1e-12, 1.0 + 1e-12);
    }

    [Fact]
    public void Compute_ReturnsMinusOneForPerfectNegativeCorrelation()
    {
        double[] x = [1.0, 2.0, 3.0, 4.0];
        double[] y = [8.0, 6.0, 4.0, 2.0];

        double correlation = PearsonCorrelation.Compute(x, y);

        Assert.InRange(correlation, -1.0 - 1e-12, -1.0 + 1e-12);
    }

    [Fact]
    public void Compute_ReturnsZeroWhenVarianceIsZero()
    {
        double[] x = [1.0, 1.0, 1.0, 1.0];
        double[] y = [5.0, 2.0, 9.0, 4.0];

        double correlation = PearsonCorrelation.Compute(x, y);

        Assert.Equal(0.0, correlation);
    }

    [Fact]
    public void Compute_Float_ReturnsOneForPerfectPositiveCorrelation()
    {
        float[] x = [1f, 2f, 3f, 4f];
        float[] y = [2f, 4f, 6f, 8f];

        float correlation = PearsonCorrelation.Compute(x, y);

        Assert.InRange(correlation, 1f - 1e-4f, 1f + 1e-4f);
    }

    [Fact]
    public void Compute_Complex_ReturnsNearOneForLinearMapping()
    {
        Complex[] x = [new Complex(1, 1), new Complex(2, 1), new Complex(3, 1), new Complex(4, 1)];
        Complex[] y = [new Complex(2, -1), new Complex(4, -1), new Complex(6, -1), new Complex(8, -1)];

        Complex correlation = PearsonCorrelation.Compute(x, y);

        Assert.InRange(correlation.Real, 1.0 - 1e-12, 1.0 + 1e-12);
        Assert.InRange(correlation.Imaginary, -1e-12, 1e-12);
    }
}