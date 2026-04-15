using System.Numerics;
using LAL.NumericalCore.Random;

namespace LAL.NumericalCore.Statistics;

internal static class Ziggurat
{
    private const int BlockCount = 128;
    private const int BlockMask = BlockCount - 1;
    private const double R = 3.442619855899;
    private const double M1 = 2147483648.0;
    private const double Vn = 9.91256303526217e-3;

    private static readonly uint[] Kn = new uint[BlockCount];
    private static readonly double[] Wn = new double[BlockCount];
    private static readonly double[] Fn = new double[BlockCount];

    static Ziggurat()
    {
        InitializeTables();
    }

    public static double NextU01(ref uint state)
    {
        return Rng.NextUniform(ref state);
    }

    public static float NextU01Float(ref uint state)
    {
        return (float)NextU01(ref state);
    }

    public static double NextUniform(ref uint state, double minInclusive, double maxExclusive)
    {
        if (!(maxExclusive > minInclusive))
        {
            throw new ArgumentException("maxExclusive must be greater than minInclusive.");
        }

        return minInclusive + ((maxExclusive - minInclusive) * NextU01(ref state));
    }

    public static float NextUniform(ref uint state, float minInclusive, float maxExclusive)
    {
        if (!(maxExclusive > minInclusive))
        {
            throw new ArgumentException("maxExclusive must be greater than minInclusive.");
        }

        return minInclusive + ((maxExclusive - minInclusive) * NextU01Float(ref state));
    }

    public static double NextNormal(ref uint state)
    {
        int hz = unchecked((int)Rng.NextUInt(ref state));
        int iz = hz & BlockMask;

        if (AbsAsUnsigned(hz) < Kn[iz])
        {
            return hz * Wn[iz];
        }

        return SampleNormalTail(ref state, hz, iz);
    }

    public static float NextNormalFloat(ref uint state)
    {
        return (float)NextNormal(ref state);
    }

    public static Complex NextComplexU01(ref uint state)
    {
        return new Complex(NextU01(ref state), NextU01(ref state));
    }

    public static Complex NextComplexUniform(ref uint state)
    {
        return new Complex(NextU01(ref state), NextU01(ref state));
    }

    public static Complex NextComplexUniform(
        ref uint state,
        double minRealInclusive,
        double maxRealExclusive,
        double minImagInclusive,
        double maxImagExclusive)
    {
        if (!(maxRealExclusive > minRealInclusive))
        {
            throw new ArgumentException("maxRealExclusive must be greater than minRealInclusive.");
        }

        if (!(maxImagExclusive > minImagInclusive))
        {
            throw new ArgumentException("maxImagExclusive must be greater than minImagInclusive.");
        }

        double real = NextUniform(ref state, minRealInclusive, maxRealExclusive);
        double imag = NextUniform(ref state, minImagInclusive, maxImagExclusive);
        return new Complex(real, imag);
    }

    public static Complex NextComplexNormal(ref uint state)
    {
        return new Complex(NextNormal(ref state), NextNormal(ref state));
    }

    private static double SampleNormalTail(ref uint state, int hz, int iz)
    {
        while (true)
        {
            if (iz == 0)
            {
                double x;
                double y;

                do
                {
                    x = -Math.Log(NextU01(ref state)) / R;
                    y = -Math.Log(NextU01(ref state));
                }
                while ((x * x) > (2d * y));

                return hz > 0 ? R + x : -R - x;
            }

            double xCandidate = hz * Wn[iz];
            double lower = Fn[iz];
            double upper = Fn[iz - 1];
            double u = NextU01(ref state);

            if (lower + (u * (upper - lower)) < Math.Exp(-0.5d * xCandidate * xCandidate))
            {
                return xCandidate;
            }

            hz = unchecked((int)Rng.NextUInt(ref state));
            iz = hz & BlockMask;

            if (AbsAsUnsigned(hz) < Kn[iz])
            {
                return hz * Wn[iz];
            }
        }
    }

    private static uint AbsAsUnsigned(int value)
    {
        int mask = value >> 31;
        return (uint)((value + mask) ^ mask);
    }

    private static void InitializeTables()
    {
        double dn = R;
        double tn = R;
        double q = Vn / Math.Exp(-0.5d * dn * dn);

        Kn[0] = (uint)((dn / q) * M1);
        Kn[1] = 0;

        Wn[0] = q / M1;
        Wn[BlockCount - 1] = dn / M1;

        Fn[0] = 1d;
        Fn[BlockCount - 1] = Math.Exp(-0.5d * dn * dn);

        for (int i = BlockCount - 2; i >= 1; i--)
        {
            dn = Math.Sqrt(-2d * Math.Log((Vn / dn) + Math.Exp(-0.5d * dn * dn)));
            Kn[i + 1] = (uint)((dn / tn) * M1);
            tn = dn;
            Fn[i] = Math.Exp(-0.5d * dn * dn);
            Wn[i] = dn / M1;
        }
    }
}