using LAL.TensorCore;

namespace LAL.Tests.TensorCore;

public class TensorShapeTests
{
    [Fact]
    public void ComputeRowMajorStrides_ReturnsExpectedValues()
    {
        int[] shape = [2, 3, 4];

        int[] strides = TensorShape.ComputeRowMajorStrides(shape);

        Assert.Equal([12, 4, 1], strides);
    }

    [Fact]
    public void GetOffset_ReturnsExpectedOffset()
    {
        int[] shape = [2, 3, 4];
        int[] strides = TensorShape.ComputeRowMajorStrides(shape);

        int offset = TensorShape.GetOffset([1, 2, 3], shape, strides);

        Assert.Equal(23, offset);
    }
}
