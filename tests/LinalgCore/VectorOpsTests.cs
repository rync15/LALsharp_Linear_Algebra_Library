using System.Numerics;
using LAL.LinalgCore;

namespace LAL.Tests.LinalgCore;

public class VectorOpsTests
{
    [Fact]
    public void AddAndSubtract_Work()
    {
        double[] left = [1.0, 2.0, 3.0];
        double[] right = [4.0, 5.0, 6.0];
        double[] sum = new double[3];
        double[] diff = new double[3];

        VectorOps.Add(left, right, sum);
        VectorOps.Subtract(right, left, diff);

        Assert.Equal(new[] { 5.0, 7.0, 9.0 }, sum);
        Assert.Equal(new[] { 3.0, 3.0, 3.0 }, diff);
    }

    [Fact]
    public void DotAndInnerProduct_AreEquivalent()
    {
        double[] left = [1.0, 2.0, 3.0];
        double[] right = [4.0, 5.0, 6.0];

        double dot = VectorOps.Dot(left, right);
        double inner = VectorOps.InnerProduct(left, right);

        Assert.InRange(dot, 32.0 - 1e-12, 32.0 + 1e-12);
        Assert.InRange(inner, 32.0 - 1e-12, 32.0 + 1e-12);
    }

    [Fact]
    public void OuterProduct_Works()
    {
        double[] left = [1.0, 2.0];
        double[] right = [3.0, 4.0, 5.0];
        double[] output = new double[6];

        VectorOps.OuterProduct(left, right, output);

        Assert.Equal(new[]
        {
            3.0, 4.0, 5.0,
            6.0, 8.0, 10.0
        }, output);
    }

    [Fact]
    public void Float_Operations_Work()
    {
        float[] left = [1f, 2f, 3f];
        float[] right = [4f, 5f, 6f];
        float[] sum = new float[3];
        float[] diff = new float[3];
        float[] outer = new float[6];

        VectorOps.Add(left, right, sum);
        VectorOps.Subtract(right, left, diff);
        float dot = VectorOps.Dot(left, right);
        VectorOps.OuterProduct([1f, 2f], [3f, 4f, 5f], outer);

        Assert.Equal([5f, 7f, 9f], sum);
        Assert.Equal([3f, 3f, 3f], diff);
        Assert.Equal(32f, dot);
        Assert.Equal([3f, 4f, 5f, 6f, 8f, 10f], outer);
    }

    [Fact]
    public void Complex_Operations_Work()
    {
        Complex[] left = [new Complex(1, 1), new Complex(2, -1)];
        Complex[] right = [new Complex(3, 0), new Complex(1, 2)];
        Complex[] sum = new Complex[2];
        Complex[] diff = new Complex[2];
        Complex[] outer = new Complex[4];

        VectorOps.Add(left, right, sum);
        VectorOps.Subtract(left, right, diff);
        Complex dot = VectorOps.Dot(left, right);
        VectorOps.OuterProduct(left, right, outer);

        Assert.Equal(new Complex(4, 1), sum[0]);
        Assert.Equal(new Complex(3, 1), sum[1]);
        Assert.Equal(new Complex(-2, 1), diff[0]);
        Assert.Equal(new Complex(1, -3), diff[1]);
        Assert.Equal(new Complex(7, 6), dot);
        Assert.Equal(new Complex(3, 3), outer[0]);
        Assert.Equal(new Complex(-1, 3), outer[1]);
        Assert.Equal(new Complex(6, -3), outer[2]);
        Assert.Equal(new Complex(4, 3), outer[3]);
    }
}
