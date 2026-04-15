using System.Numerics;
using LAL.LinalgCore;

namespace LAL.Tests.LinalgCore;

public class NormsTests
{
    [Fact]
    public void L1L2AndInfinity_ReturnExpectedValues()
    {
        double[] values = [3.0, -4.0, 12.0];

        double l1 = Norms.L1(values);
        double l2Squared = Norms.L2Squared(values);
        double l2 = Norms.L2(values);
        double inf = Norms.Infinity(values);

        Assert.Equal(19.0, l1, 10);
        Assert.Equal(169.0, l2Squared, 10);
        Assert.Equal(13.0, l2, 10);
        Assert.Equal(12.0, inf, 10);
    }

    [Fact]
    public void Norms_EmptyVector_ReturnsZero()
    {
        double[] values = [];

        Assert.Equal(0.0, Norms.L1(values), 10);
        Assert.Equal(0.0, Norms.L2Squared(values), 10);
        Assert.Equal(0.0, Norms.L2(values), 10);
        Assert.Equal(0.0, Norms.Infinity(values), 10);
    }

    [Fact]
    public void FloatNorms_ReturnExpectedValues()
    {
        float[] values = [3f, -4f, 12f];

        float l1 = Norms.L1(values);
        float l2Squared = Norms.L2Squared(values);
        float l2 = Norms.L2(values);
        float inf = Norms.Infinity(values);

        Assert.Equal(19f, l1, 5);
        Assert.Equal(169f, l2Squared, 5);
        Assert.Equal(13f, l2, 5);
        Assert.Equal(12f, inf, 5);
    }

    [Fact]
    public void ComplexNorms_ReturnExpectedValues()
    {
        Complex[] values = [new Complex(3, 4), new Complex(0, -5)];

        double l1 = Norms.L1(values);
        double l2Squared = Norms.L2Squared(values);
        double l2 = Norms.L2(values);
        double inf = Norms.Infinity(values);

        Assert.Equal(10.0, l1, 10);
        Assert.Equal(50.0, l2Squared, 10);
        Assert.Equal(Math.Sqrt(50.0), l2, 10);
        Assert.Equal(5.0, inf, 10);
    }
}
