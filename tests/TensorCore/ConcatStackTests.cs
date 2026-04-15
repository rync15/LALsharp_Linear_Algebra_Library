using LAL.TensorCore;

namespace LAL.Tests.TensorCore;

public class ConcatStackTests
{
    [Fact]
    public void Concatenate1D_AndStackRows_Work()
    {
        double[] a = [1, 2];
        double[] b = [3, 4, 5];
        double[] concat = new double[a.Length + b.Length];

        ConcatStack.Concatenate1D(a, b, concat);
        Assert.Equal(new double[] { 1, 2, 3, 4, 5 }, concat);

        double[] row0 = [1, 2, 3];
        double[] row1 = [4, 5, 6];
        double[] stacked = new double[6];

        ConcatStack.StackRows(row0, row1, cols: 3, stacked);
        Assert.Equal(new double[] { 1, 2, 3, 4, 5, 6 }, stacked);

        double[] concatAlias = new double[a.Length + b.Length];
        ConcatStack.Concatenate(a, b, concatAlias);
        Assert.Equal(new double[] { 1, 2, 3, 4, 5 }, concatAlias);

        double[] stackedAlias = new double[6];
        ConcatStack.Stack(row0, row1, cols: 3, stackedAlias);
        Assert.Equal(new double[] { 1, 2, 3, 4, 5, 6 }, stackedAlias);
    }
}
