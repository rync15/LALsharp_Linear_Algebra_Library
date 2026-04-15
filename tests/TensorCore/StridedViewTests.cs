using LAL.TensorCore;

namespace LAL.Tests.TensorCore;

public class StridedViewTests
{
    [Fact]
    public void PermuteShape_ReordersAxes()
    {
        int[] result = StridedView.PermuteShape([2, 3, 4], [2, 0, 1]);

        Assert.Equal([4, 2, 3], result);
    }

    [Fact]
    public void ExpandAndSqueeze_RoundTripShape()
    {
        int[] expanded = StridedView.ExpandDims([2, 3], 1);
        int[] squeezed = StridedView.Squeeze(expanded, 1);

        Assert.Equal([2, 1, 3], expanded);
        Assert.Equal([2, 3], squeezed);
    }

    [Fact]
    public void Slice1D_WithStep_Works()
    {
        int[] values = [0, 1, 2, 3, 4, 5];
        int[] sliced = new int[3];

        int count = StridedView.Slice1D<int>(values, start: 1, stop: 6, step: 2, destination: sliced);

        Assert.Equal(3, count);
        Assert.Equal(new[] { 1, 3, 5 }, sliced);
    }

    [Fact]
    public void Slice1D_WithNegativeStep_Works()
    {
        int[] values = [0, 1, 2, 3, 4, 5];
        int[] sliced = new int[3];

        int count = StridedView.Slice1D<int>(values, start: 5, stop: 0, step: -2, destination: sliced);

        Assert.Equal(3, count);
        Assert.Equal(new[] { 5, 3, 1 }, sliced);
    }
}
