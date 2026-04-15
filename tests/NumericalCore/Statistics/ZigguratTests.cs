using System.Numerics;
using LAL.NumericalCore.Statistics;

namespace LAL.Tests.NumericalCore.Statistics;

public class ZigguratTests
{
    [Fact]
    public void NextU01_IsDeterministicForSameSeed()
    {
        uint stateA = 12345u;
        uint stateB = 12345u;

        double a = Ziggurat.NextU01(ref stateA);
        double b = Ziggurat.NextU01(ref stateB);

        Assert.Equal(a, b);
    }

    [Fact]
    public void NextU01_StaysInsideOpenUnitInterval()
    {
        uint state = 2463534242u;

        for (int i = 0; i < 10_000; i++)
        {
            double value = Ziggurat.NextU01(ref state);
            Assert.True(value > 0d);
            Assert.True(value < 1d);
        }
    }

    [Fact]
    public void NextUniform_StaysInsideRequestedRange()
    {
        uint state = 987654321u;

        for (int i = 0; i < 10_000; i++)
        {
            double value = Ziggurat.NextUniform(ref state, minInclusive: -2.0, maxExclusive: 5.0);
            Assert.True(value >= -2.0);
            Assert.True(value < 5.0);
        }
    }

    [Fact]
    public void NextNormal_HasReasonableMeanAndVarianceOverLargeSample()
    {
        uint state = 2463534242u;
        const int sampleCount = 20_000;

        double sum = 0d;
        double sumSquares = 0d;

        for (int i = 0; i < sampleCount; i++)
        {
            double value = Ziggurat.NextNormal(ref state);
            sum += value;
            sumSquares += value * value;
        }

        double mean = sum / sampleCount;
        double variance = (sumSquares / sampleCount) - (mean * mean);

        Assert.InRange(mean, -0.06, 0.06);
        Assert.InRange(variance, 0.85, 1.15);
    }

    [Fact]
    public void FloatAndComplex_APIs_Work()
    {
        uint state = 424242u;

        float u = Ziggurat.NextU01Float(ref state);
        float uf = Ziggurat.NextUniform(ref state, -1f, 2f);
        float nf = Ziggurat.NextNormalFloat(ref state);
        Complex cu = Ziggurat.NextComplexU01(ref state);
        Complex cuni = Ziggurat.NextComplexUniform(ref state, -2.0, 3.0, -4.0, 5.0);
        Complex cn = Ziggurat.NextComplexNormal(ref state);

        Assert.True(u > 0f && u < 1f);
        Assert.True(uf >= -1f && uf < 2f);
        Assert.False(float.IsNaN(nf));

        Assert.True(cu.Real > 0d && cu.Real < 1d);
        Assert.True(cu.Imaginary > 0d && cu.Imaginary < 1d);
        Assert.True(cuni.Real >= -2d && cuni.Real < 3d);
        Assert.True(cuni.Imaginary >= -4d && cuni.Imaginary < 5d);
        Assert.False(double.IsNaN(cn.Real));
        Assert.False(double.IsNaN(cn.Imaginary));
    }
}