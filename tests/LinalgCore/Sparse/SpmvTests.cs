using System.Numerics;
using LAL.LinalgCore.Sparse;

namespace LAL.Tests.LinalgCore.Sparse;

public class SpmvTests
{
    [Fact]
    public void CsrMultiply_ComputesRowProducts()
    {
        double[] values = [10.0, 20.0, 30.0];
        int[] colIndices = [0, 2, 1];
        int[] rowPointers = [0, 2, 3];
        double[] x = [1.0, 2.0, 3.0];
        double[] y = new double[2];

        Spmv.CsrMultiply(values, colIndices, rowPointers, x, y);

        Assert.Equal(new double[] { 70.0, 60.0 }, y);
    }

    [Fact]
    public void CscMultiply_ComputesRowProducts()
    {
        double[] values = [10.0, 30.0, 20.0];
        int[] rowIndices = [0, 1, 0];
        int[] colPointers = [0, 1, 2, 3];
        double[] x = [1.0, 2.0, 3.0];
        double[] y = new double[2];

        Spmv.CscMultiply(values, rowIndices, colPointers, x, y);

        Assert.Equal(new double[] { 70.0, 60.0 }, y);
    }

    [Fact]
    public void CsrMultiply_Complex_ComputesRowProducts()
    {
        Complex[] values = [new Complex(10.0, 0.0), new Complex(20.0, 0.0), new Complex(30.0, 0.0)];
        int[] colIndices = [0, 2, 1];
        int[] rowPointers = [0, 2, 3];
        Complex[] x = [new Complex(1.0, 0.0), new Complex(2.0, 0.0), new Complex(3.0, 0.0)];
        Complex[] y = new Complex[2];

        Spmv.CsrMultiply(values, colIndices, rowPointers, x, y);

        Assert.InRange(Complex.Abs(y[0] - new Complex(70.0, 0.0)), 0d, 1e-12);
        Assert.InRange(Complex.Abs(y[1] - new Complex(60.0, 0.0)), 0d, 1e-12);
    }

    [Fact]
    public void CscMultiply_Complex_ComputesRowProducts()
    {
        Complex[] values = [new Complex(10.0, 0.0), new Complex(30.0, 0.0), new Complex(20.0, 0.0)];
        int[] rowIndices = [0, 1, 0];
        int[] colPointers = [0, 1, 2, 3];
        Complex[] x = [new Complex(1.0, 0.0), new Complex(2.0, 0.0), new Complex(3.0, 0.0)];
        Complex[] y = new Complex[2];

        Spmv.CscMultiply(values, rowIndices, colPointers, x, y);

        Assert.InRange(Complex.Abs(y[0] - new Complex(70.0, 0.0)), 0d, 1e-12);
        Assert.InRange(Complex.Abs(y[1] - new Complex(60.0, 0.0)), 0d, 1e-12);
    }
}
