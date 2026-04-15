using LAL.TensorCore;

namespace LAL.Tests.TensorCore;

public class ReductionsTests
{
    [Fact]
    public void Sum_AndMean_ReturnExpectedValues()
    {
        double[] values = [1.0, 2.0, 3.0, 4.0];

        double sum = Reductions.Sum(values);
        double mean = Reductions.Mean(values);

        Assert.Equal(10.0, sum, 10);
        Assert.Equal(2.5, mean, 10);
    }

    [Fact]
    public void Variance_ReturnsPopulationVariance()
    {
        double[] values = [1.0, 2.0, 3.0];

        double variance = Reductions.Variance(values);

        Assert.Equal(2.0 / 3.0, variance, 10);
    }

    [Fact]
    public void MaxMinQuantileMedian_Work()
    {
        double[] values = [4.0, 1.0, 2.0, 3.0];

        Assert.Equal(4.0, Reductions.Max(values), 10);
        Assert.Equal(1.0, Reductions.Min(values), 10);
        Assert.Equal(2.5, Reductions.Median(values), 10);
        Assert.Equal(1.75, Reductions.Quantile(values, 0.25), 10);
    }

    [Fact]
    public void NanSafeReductions_IgnoreNaNValues()
    {
        double[] values = [1.0, double.NaN, 3.0];

        Assert.Equal(4.0, Reductions.SumNanSafe(values), 10);
        Assert.Equal(2.0, Reductions.MeanNanSafe(values), 10);
        Assert.Equal(1.0, Reductions.VarianceNanSafe(values), 10);
        Assert.Equal(3.0, Reductions.MaxNanSafe(values), 10);
        Assert.Equal(1.0, Reductions.MinNanSafe(values), 10);
    }
}
