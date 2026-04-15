using LAL.TensorCore;

namespace LAL.Tests.TensorCore;

public class ShapeOpsTests
{
    [Fact]
    public void ReshapeCopy_AndTranspose2D_Work()
    {
        double[] flat = [1, 2, 3, 4, 5, 6];
        double[] reshaped = new double[6];
        ShapeOps.Reshape(flat, reshaped);
        Assert.Equal(flat, reshaped);

        double[] reshapedByAlias = new double[6];
        ShapeOps.ReshapeCopy(flat, reshapedByAlias);
        Assert.Equal(flat, reshapedByAlias);

        double[] input = [1, 2, 3, 4, 5, 6];
        double[] output = new double[6];
        ShapeOps.Transpose2D(input, rows: 2, cols: 3, output);

        Assert.Equal(new double[] { 1, 4, 2, 5, 3, 6 }, output);
    }

    [Fact]
    public void SwapAxes2D_Work()
    {
        double[] input = [1, 2, 3, 4, 5, 6];
        double[] output = new double[6];

        ShapeOps.SwapAxes2D(input, rows: 3, cols: 2, output);

        Assert.Equal(new double[] { 1, 3, 5, 2, 4, 6 }, output);
    }

    [Fact]
    public void FlattenAndShapeTransforms_Work()
    {
        double[] input = [1, 2, 3, 4];
        double[] flattened = new double[4];
        ShapeOps.Flatten(input, flattened);
        Assert.Equal(input, flattened);

        int[] expanded = ShapeOps.ExpandDims([2, 3], 0);
        Assert.Equal([1, 2, 3], expanded);

        int[] squeezed = ShapeOps.Squeeze([1, 2, 1, 3]);
        Assert.Equal([2, 3], squeezed);

        int[] swapped = ShapeOps.SwapAxes([2, 3, 4], 0, 2);
        Assert.Equal([4, 3, 2], swapped);

        int[] transposed = ShapeOps.TransposeShape([2, 3, 4], [1, 2, 0]);
        Assert.Equal([3, 4, 2], transposed);
    }
}
