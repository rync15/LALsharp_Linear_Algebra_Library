using LAL.TensorCore;

namespace LAL.Tests.TensorCore;

public class EinsumTests
{
    [Fact]
    public void DotProductPattern_Works()
    {
        double[] a = [1, 2, 3];
        double[] b = [4, 5, 6];
        double[] destination = new double[1];

        Einsum.Compute("i,i->", a, b, destination);

        Assert.Equal(32.0, destination[0], 10);

        double viaAlias = Einsum.Evaluate("i,i->", a, b);
        Assert.Equal(32.0, viaAlias, 10);
    }

    [Fact]
    public void MatrixMultiplyPattern_Works()
    {
        double[] a = [1, 2, 3, 4];
        double[] b = [5, 6, 7, 8];
        double[] result = new double[4];

        Einsum.Compute("ij,jk->ik", a, b, result, m: 2, n: 2, k: 2);

        Assert.Equal(new double[] { 19, 22, 43, 50 }, result);

        double[,] left2D =
        {
            { 1, 2 },
            { 3, 4 }
        };
        double[,] right2D =
        {
            { 5, 6 },
            { 7, 8 }
        };

        double[,] aliasResult = Einsum.EvaluateMatMul("ij,jk->ik", left2D, right2D);
        Assert.Equal(19, aliasResult[0, 0]);
        Assert.Equal(22, aliasResult[0, 1]);
        Assert.Equal(43, aliasResult[1, 0]);
        Assert.Equal(50, aliasResult[1, 1]);

        double[] aliasDestination = new double[4];
        Einsum.TensorContractionGemm(a, b, aliasDestination, m: 2, n: 2, k: 2);
        Assert.Equal(new double[] { 19, 22, 43, 50 }, aliasDestination);
    }

    [Fact]
    public void Kronecker_Works()
    {
        double[] left = [1, 2, 3, 4];
        double[] right = [0, 5, 6, 7];
        double[] destination = new double[16];

        Einsum.Kronecker(left, leftRows: 2, leftCols: 2, right, rightRows: 2, rightCols: 2, destination);

        Assert.Equal(
            new double[]
            {
                0, 5, 0, 10,
                6, 7, 12, 14,
                0, 15, 0, 20,
                18, 21, 24, 28
            },
            destination);
    }
}
