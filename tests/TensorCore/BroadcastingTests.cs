using LAL.TensorCore;

namespace LAL.Tests.TensorCore;

public class BroadcastingTests
{
    [Fact]
    public void BroadcastShapes_ReturnsExpectedShape()
    {
        int[] output = Broadcasting.BroadcastShapes([3, 1], [1, 4]);

        Assert.Equal([3, 4], output);
    }

    [Fact]
    public void ComputeBroadcastStrides_SetsBroadcastAxisStrideToZero()
    {
        int[] sourceShape = [3, 1];
        int[] sourceStrides = TensorShape.ComputeRowMajorStrides(sourceShape);

        int[] result = Broadcasting.ComputeBroadcastStrides(sourceShape, sourceStrides, [3, 4]);

        Assert.Equal([1, 0], result);
    }
}
