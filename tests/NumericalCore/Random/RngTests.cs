using System.Numerics;
using LAL.NumericalCore.Random;

namespace LAL.Tests.NumericalCore.Random;

public class RngTests
{
    [Fact]
    public void NextUniform_IsDeterministicForSameSeed()
    {
        uint a = 12345;
        uint b = 12345;

        double a1 = Rng.NextUniform(ref a);
        double b1 = Rng.NextUniform(ref b);

        Assert.Equal(a1, b1);

        double normal = Rng.NextNormal(ref a);
        Assert.False(double.IsNaN(normal));
        Assert.False(double.IsInfinity(normal));
    }

    [Fact]
    public void NextUInt_NormalizesZeroSeed()
    {
        uint state = 0;

        uint next = Rng.NextUInt(ref state);

        Assert.NotEqual(0u, state);
        Assert.NotEqual(0u, next);
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
            double value = Rng.NextNormal(ref state);
            sum += value;
            sumSquares += value * value;
        }

        double mean = sum / sampleCount;
        double variance = (sumSquares / sampleCount) - (mean * mean);

        Assert.InRange(mean, -0.06, 0.06);
        Assert.InRange(variance, 0.85, 1.15);
    }

    [Fact]
    public void FloatAndComplex_APIs_ReturnFiniteValues()
    {
        uint state = 13579u;

        float u = Rng.NextUniformFloat(ref state);
        float n = Rng.NextNormalFloat(ref state);
        Complex cu = Rng.NextComplexUniform(ref state);
        Complex cn = Rng.NextComplexNormal(ref state);

        Assert.InRange(u, 0f, 1f);
        Assert.False(float.IsNaN(n));
        Assert.False(float.IsInfinity(n));

        Assert.False(double.IsNaN(cu.Real));
        Assert.False(double.IsNaN(cu.Imaginary));
        Assert.False(double.IsNaN(cn.Real));
        Assert.False(double.IsNaN(cn.Imaginary));
    }
}
