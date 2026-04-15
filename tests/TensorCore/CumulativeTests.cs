using LAL.TensorCore;

namespace LAL.Tests.TensorCore;

public class CumulativeTests
{
    [Fact]
    public void CumSum_Works()
    {
        double[] values = [1.0, 2.0, 3.0, 4.0];
        double[] destination = new double[values.Length];

        Cumulative.CumSum(values, destination);

        Assert.Equal(new[] { 1.0, 3.0, 6.0, 10.0 }, destination);
    }

    [Fact]
    public void CumProd_Works()
    {
        double[] values = [1.0, 2.0, 3.0, 4.0];
        double[] destination = new double[values.Length];

        Cumulative.CumProd(values, destination);

        Assert.Equal(new[] { 1.0, 2.0, 6.0, 24.0 }, destination);
    }
}
