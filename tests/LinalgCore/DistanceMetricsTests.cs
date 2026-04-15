using System.Numerics;
using LAL.LinalgCore;

namespace LAL.Tests.LinalgCore;

public class DistanceMetricsTests
{
    [Fact]
    public void PairwiseEuclidean_ComputesExpectedMatrix()
    {
        double[] points =
        [
            0.0, 0.0,
            3.0, 4.0,
            6.0, 8.0
        ];

        double[] distances = new double[9];
        DistanceMetrics.PairwiseEuclidean(points, pointCount: 3, dimension: 2, distances);

        Assert.InRange(distances[(0 * 3) + 1], 5.0 - 1e-12, 5.0 + 1e-12);
        Assert.InRange(distances[(1 * 3) + 2], 5.0 - 1e-12, 5.0 + 1e-12);
        Assert.InRange(distances[(0 * 3) + 2], 10.0 - 1e-12, 10.0 + 1e-12);
    }

    [Fact]
    public void PairwiseCosine_ComputesExpectedDistances()
    {
        double[] points =
        [
            1.0, 0.0,
            0.0, 1.0,
            1.0, 1.0
        ];

        double[] distances = new double[9];
        DistanceMetrics.PairwiseCosine(points, pointCount: 3, dimension: 2, distances);

        Assert.InRange(distances[(0 * 3) + 1], 1.0 - 1e-12, 1.0 + 1e-12);

        double expected = 1.0 - (1.0 / Math.Sqrt(2.0));
        Assert.InRange(distances[(0 * 3) + 2], expected - 1e-12, expected + 1e-12);
        Assert.InRange(distances[(1 * 3) + 2], expected - 1e-12, expected + 1e-12);
    }

    [Fact]
    public void PairwiseMahalanobis_WithIdentityInverseCovariance_EqualsEuclidean()
    {
        double[] points =
        [
            0.0, 0.0,
            3.0, 4.0
        ];

        double[] inverseCovariance =
        [
            1.0, 0.0,
            0.0, 1.0
        ];

        double[] distances = new double[4];
        DistanceMetrics.PairwiseMahalanobis(points, pointCount: 2, dimension: 2, inverseCovariance, distances);

        Assert.InRange(distances[(0 * 2) + 1], 5.0 - 1e-12, 5.0 + 1e-12);
        Assert.InRange(distances[(1 * 2) + 0], 5.0 - 1e-12, 5.0 + 1e-12);
    }

    [Fact]
    public void PairwiseEuclidean_Complex_ComputesExpectedDistance()
    {
        Complex[] points =
        [
            Complex.Zero,
            new Complex(3.0, 4.0)
        ];

        double[] distances = new double[4];
        DistanceMetrics.PairwiseEuclidean(points, pointCount: 2, dimension: 1, distances);

        Assert.InRange(distances[(0 * 2) + 1], 5.0 - 1e-12, 5.0 + 1e-12);
        Assert.InRange(distances[(1 * 2) + 0], 5.0 - 1e-12, 5.0 + 1e-12);
    }

    [Fact]
    public void PairwiseCosine_Complex_UsesMagnitudeNormalizedInnerProduct()
    {
        Complex[] points =
        [
            new Complex(1.0, 0.0),
            new Complex(0.0, 1.0)
        ];

        double[] distances = new double[4];
        DistanceMetrics.PairwiseCosine(points, pointCount: 2, dimension: 1, distances);

        Assert.InRange(distances[(0 * 2) + 1], 0.0 - 1e-12, 0.0 + 1e-12);
        Assert.InRange(distances[(1 * 2) + 0], 0.0 - 1e-12, 0.0 + 1e-12);
    }

    [Fact]
    public void PairwiseMahalanobis_Complex_WithIdentityInverseCovariance_EqualsEuclidean()
    {
        Complex[] points =
        [
            Complex.Zero,
            new Complex(3.0, 4.0)
        ];

        Complex[] inverseCovariance =
        [
            Complex.One
        ];

        double[] distances = new double[4];
        DistanceMetrics.PairwiseMahalanobis(points, pointCount: 2, dimension: 1, inverseCovariance, distances);

        Assert.InRange(distances[(0 * 2) + 1], 5.0 - 1e-12, 5.0 + 1e-12);
        Assert.InRange(distances[(1 * 2) + 0], 5.0 - 1e-12, 5.0 + 1e-12);
    }
}
