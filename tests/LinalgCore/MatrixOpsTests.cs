using System.Numerics;
using LAL.LinalgCore;

namespace LAL.Tests.LinalgCore;

public class MatrixOpsTests
{
    [Fact]
    public void AddSubtractAndDot_Work()
    {
        double[] a =
        [
            1.0, 2.0,
            3.0, 4.0
        ];

        double[] b =
        [
            4.0, 3.0,
            2.0, 1.0
        ];

        double[] sum = new double[4];
        double[] diff = new double[4];

        MatrixOps.Add(a, b, sum, rows: 2, cols: 2);
        MatrixOps.Subtract(a, b, diff, rows: 2, cols: 2);

        Assert.Equal(new[] { 5.0, 5.0, 5.0, 5.0 }, sum);
        Assert.Equal(new[] { -3.0, -1.0, 1.0, 3.0 }, diff);

        double dot = MatrixOps.Dot(a, b, rows: 2, cols: 2);
        Assert.InRange(dot, 20.0 - 1e-12, 20.0 + 1e-12);
    }

    [Fact]
    public void MultiplyMatrixVector_Works()
    {
        double[] matrix =
        [
            1.0, 2.0, 3.0,
            4.0, 5.0, 6.0
        ];

        double[] vector = [1.0, 0.0, -1.0];
        double[] result = new double[2];

        MatrixOps.Multiply(matrix, rows: 2, cols: 3, vector, result);

        Assert.InRange(result[0], -2.0 - 1e-12, -2.0 + 1e-12);
        Assert.InRange(result[1], -2.0 - 1e-12, -2.0 + 1e-12);
    }

    [Fact]
    public void MultiplyMatrixMatrix_Works()
    {
        double[] left =
        [
            1.0, 2.0,
            3.0, 4.0
        ];

        double[] right =
        [
            5.0, 6.0,
            7.0, 8.0
        ];

        double[] result = new double[4];

        MatrixOps.Multiply(left, leftRows: 2, sharedDim: 2, right, rightCols: 2, result);

        Assert.Equal(new[]
        {
            19.0, 22.0,
            43.0, 50.0
        }, result);
    }

    [Fact]
    public void Float_Operations_Work()
    {
        float[] a =
        [
            1f, 2f,
            3f, 4f
        ];

        float[] b =
        [
            4f, 3f,
            2f, 1f
        ];

        float[] sum = new float[4];
        float[] diff = new float[4];
        float[] mv = new float[2];
        float[] mm = new float[4];

        MatrixOps.Add(a, b, sum, rows: 2, cols: 2);
        MatrixOps.Subtract(a, b, diff, rows: 2, cols: 2);
        float dot = MatrixOps.Dot(a, b, rows: 2, cols: 2);
        MatrixOps.Multiply([1f, 2f, 3f, 4f], rows: 2, cols: 2, [1f, -1f], mv);
        MatrixOps.Multiply([1f, 2f, 3f, 4f], leftRows: 2, sharedDim: 2, [2f, 0f, 1f, 2f], rightCols: 2, mm);

        Assert.Equal([5f, 5f, 5f, 5f], sum);
        Assert.Equal([-3f, -1f, 1f, 3f], diff);
        Assert.Equal(20f, dot);
        Assert.Equal([-1f, -1f], mv);
        Assert.Equal([4f, 4f, 10f, 8f], mm);
    }

    [Fact]
    public void Complex_Operations_Work()
    {
        Complex[] a =
        [
            new Complex(1, 0), new Complex(0, 1),
            new Complex(2, -1), new Complex(1, 0)
        ];
        Complex[] b =
        [
            new Complex(1, 0), new Complex(2, 0),
            new Complex(0, -1), new Complex(1, 1)
        ];
        Complex[] sum = new Complex[4];
        Complex[] diff = new Complex[4];
        Complex[] mv = new Complex[2];
        Complex[] mm = new Complex[4];

        MatrixOps.Add(a, b, sum, rows: 2, cols: 2);
        MatrixOps.Subtract(a, b, diff, rows: 2, cols: 2);
        Complex dot = MatrixOps.Dot(a, b, rows: 2, cols: 2);
        MatrixOps.Multiply(a, rows: 2, cols: 2, [new Complex(1, 1), new Complex(2, 0)], mv);
        MatrixOps.Multiply(a, leftRows: 2, sharedDim: 2, b, rightCols: 2, mm);

        Assert.Equal(new Complex(2, 0), sum[0]);
        Assert.Equal(new Complex(2, 1), sum[1]);
        Assert.Equal(new Complex(2, -2), sum[2]);
        Assert.Equal(new Complex(2, 1), sum[3]);

        Assert.Equal(Complex.Zero, diff[0]);
        Assert.Equal(new Complex(-2, 1), diff[1]);
        Assert.Equal(new Complex(2, 0), diff[2]);
        Assert.Equal(new Complex(0, -1), diff[3]);

        Assert.Equal(new Complex(1, 1), dot);
        Assert.Equal(new Complex(1, 3), mv[0]);
        Assert.Equal(new Complex(5, 1), mv[1]);
        Assert.Equal(new Complex(2, 0), mm[0]);
        Assert.Equal(new Complex(1, 1), mm[1]);
        Assert.Equal(new Complex(2, -2), mm[2]);
        Assert.Equal(new Complex(5, -1), mm[3]);
    }
}
