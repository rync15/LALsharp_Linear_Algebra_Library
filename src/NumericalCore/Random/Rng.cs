using System.Numerics;

namespace LAL.NumericalCore.Random;

internal static class Rng
{
    private const uint DefaultSeed = 2463534242u;

    public static uint NormalizeSeed(uint seed)
    {
        return seed == 0 ? DefaultSeed : seed;
    }

    public static uint NextUInt(ref uint state)
    {
        state = NormalizeSeed(state);

        uint x = state;
        x ^= x << 13;
        x ^= x >> 17;
        x ^= x << 5;
        state = x;
        return x;
    }

    public static double NextUniform(ref uint state)
    {
        uint value = NextUInt(ref state);
        return (value + 0.5d) / (uint.MaxValue + 1d);
    }

    public static double NextNormal(ref uint state)
    {
        double u1 = NextUniform(ref state);
        double u2 = NextUniform(ref state);
        double radius = Math.Sqrt(-2d * Math.Log(u1));
        double theta = 2d * Math.PI * u2;
        return radius * Math.Cos(theta);
    }

    public static float NextUniformFloat(ref uint state)
    {
        return (float)NextUniform(ref state);
    }

    public static float NextNormalFloat(ref uint state)
    {
        return (float)NextNormal(ref state);
    }

    public static Complex NextComplexUniform(ref uint state)
    {
        return new Complex(NextUniform(ref state), NextUniform(ref state));
    }

    public static Complex NextComplexNormal(ref uint state)
    {
        return new Complex(NextNormal(ref state), NextNormal(ref state));
    }
}

