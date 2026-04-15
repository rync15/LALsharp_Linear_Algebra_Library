using LAL.TensorCore;

namespace LAL.Tests.TensorCore;

public class SortSearchTests
{
    [Fact]
    public void Argsort_SearchSorted_NonZero_Work()
    {
        double[] values = [3, 1, 2];
        int[] indices = new int[values.Length];

        SortSearch.Argsort(values, indices);
        Assert.Equal(new[] { 1, 2, 0 }, indices);

        int pos = SortSearch.SearchSorted(new double[] { 1, 3, 5, 7 }, 4);
        Assert.Equal(2, pos);

        int[] nonZero = SortSearch.NonZero(new double[] { 0, -2, 0, 4 });
        Assert.Equal(new[] { 1, 3 }, nonZero);

        int[] descending = new int[values.Length];
        SortSearch.Argsort(values, descending, ascending: false);
        Assert.Equal(new[] { 0, 2, 1 }, descending);

        int[] nonZeroSpan = new int[4];
        int count = SortSearch.NonZero(new double[] { 0, -2, 0, 4 }, nonZeroSpan);
        Assert.Equal(2, count);
        Assert.Equal(1, nonZeroSpan[0]);
        Assert.Equal(3, nonZeroSpan[1]);
    }

    [Fact]
    public void ArgMaxArgMinAndLexsort_Work()
    {
        double[] values = [3.0, -1.0, 2.0];
        Assert.Equal(0, SortSearch.ArgMax(values));
        Assert.Equal(1, SortSearch.ArgMin(values));

        double[] primary = [1.0, 1.0, 0.0, 0.0];
        double[] secondary = [2.0, 1.0, 2.0, 1.0];

        int[] lex = SortSearch.Lexsort(primary, secondary);
        Assert.Equal(new[] { 3, 2, 1, 0 }, lex);

        int[] destination = new int[4];
        SortSearch.Lexsort(primary, secondary, destination);
        Assert.Equal(new[] { 3, 2, 1, 0 }, destination);

        bool[] mask = [false, true, false, true, true];
        int[] whereIndices = SortSearch.Where(mask);
        Assert.Equal(new[] { 1, 3, 4 }, whereIndices);

        int[] whereDestination = new int[5];
        int whereCount = SortSearch.Where(mask, whereDestination);
        Assert.Equal(3, whereCount);
        Assert.Equal(1, whereDestination[0]);
        Assert.Equal(3, whereDestination[1]);
        Assert.Equal(4, whereDestination[2]);
    }
}
