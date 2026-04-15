using LAL.TensorCore;

namespace LAL.Tests.TensorCore;

public class MaskOpsTests
{
    [Fact]
    public void GreaterThanAndWhere_Work()
    {
        double[] values = [1.0, 3.0, -2.0, 4.0];
        bool[] mask = new bool[values.Length];
        double[] filtered = new double[values.Length];

        MaskOps.GreaterThan(values, 1.5, mask);
        MaskOps.Where(values, mask, filtered, fallback: 0.0);

        Assert.Equal(new[] { false, true, false, true }, mask);
        Assert.Equal(new double[] { 0.0, 3.0, 0.0, 4.0 }, filtered);
    }
}
