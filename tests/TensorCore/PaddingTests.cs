using LAL.TensorCore;

namespace LAL.Tests.TensorCore;

public class PaddingTests
{
    [Fact]
    public void ZeroPad1D_Works()
    {
        double[] input = [1, 2, 3];
        double[] padded = Padding.ZeroPad1D(input, left: 2, right: 1);

        Assert.Equal(new double[] { 0, 0, 1, 2, 3, 0 }, padded);
    }

    [Fact]
    public void EdgeAndPeriodicPad1D_Work()
    {
        double[] input = [1, 2, 3];

        double[] edge = Padding.EdgePad1D(input, left: 2, right: 1);
        Assert.Equal(new double[] { 1, 1, 1, 2, 3, 3 }, edge);

        double[] periodic = Padding.PeriodicPad1D(input, left: 2, right: 2);
        Assert.Equal(new double[] { 2, 3, 1, 2, 3, 1, 2 }, periodic);
    }
}
