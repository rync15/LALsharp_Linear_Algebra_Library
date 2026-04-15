namespace LAL.OdeCore;

internal static class DenseOutput
{
    public static void InterpolateLinear(ReadOnlySpan<double> y0, ReadOnlySpan<double> y1, double theta, Span<double> yOut)
    {
        if (theta < 0d || theta > 1d)
        {
            throw new ArgumentOutOfRangeException(nameof(theta), "Interpolation fraction must be in [0,1].");
        }

        if (y0.Length != y1.Length || y0.Length != yOut.Length)
        {
            throw new ArgumentException("State vectors must have matching lengths.");
        }

        Interpolate(y0, y1, theta, yOut);
    }

    public static void InterpolateLinear(ReadOnlySpan<float> y0, ReadOnlySpan<float> y1, float theta, Span<float> yOut)
    {
        if (theta < 0f || theta > 1f)
        {
            throw new ArgumentOutOfRangeException(nameof(theta), "Interpolation fraction must be in [0,1].");
        }

        if (y0.Length != y1.Length || y0.Length != yOut.Length)
        {
            throw new ArgumentException("State vectors must have matching lengths.");
        }

        Interpolate(y0, y1, theta, yOut);
    }

    public static void InterpolateLinear(
        ReadOnlySpan<System.Numerics.Complex> y0,
        ReadOnlySpan<System.Numerics.Complex> y1,
        double theta,
        Span<System.Numerics.Complex> yOut)
    {
        if (theta < 0d || theta > 1d)
        {
            throw new ArgumentOutOfRangeException(nameof(theta), "Interpolation fraction must be in [0,1].");
        }

        if (y0.Length != y1.Length || y0.Length != yOut.Length)
        {
            throw new ArgumentException("State vectors must have matching lengths.");
        }

        for (int i = 0; i < yOut.Length; i++)
        {
            yOut[i] = y0[i] + (theta * (y1[i] - y0[i]));
        }
    }

    private static void Interpolate(ReadOnlySpan<double> y0, ReadOnlySpan<double> y1, double theta, Span<double> yOut)
    {
        int i = 0;
        if (System.Numerics.Vector.IsHardwareAccelerated && y0.Length >= (System.Numerics.Vector<double>.Count * 2))
        {
            int width = System.Numerics.Vector<double>.Count;
            int end = y0.Length - width;
            System.Numerics.Vector<double> thetaVec = new(theta);

            for (; i <= end; i += width)
            {
                System.Numerics.Vector<double> y0Vec = new(y0.Slice(i, width));
                System.Numerics.Vector<double> y1Vec = new(y1.Slice(i, width));
                (y0Vec + (thetaVec * (y1Vec - y0Vec))).CopyTo(yOut.Slice(i, width));
            }
        }

        for (; i < yOut.Length; i++)
        {
            yOut[i] = y0[i] + (theta * (y1[i] - y0[i]));
        }
    }

    private static void Interpolate(ReadOnlySpan<float> y0, ReadOnlySpan<float> y1, float theta, Span<float> yOut)
    {
        int i = 0;
        if (System.Numerics.Vector.IsHardwareAccelerated && y0.Length >= (System.Numerics.Vector<float>.Count * 2))
        {
            int width = System.Numerics.Vector<float>.Count;
            int end = y0.Length - width;
            System.Numerics.Vector<float> thetaVec = new(theta);

            for (; i <= end; i += width)
            {
                System.Numerics.Vector<float> y0Vec = new(y0.Slice(i, width));
                System.Numerics.Vector<float> y1Vec = new(y1.Slice(i, width));
                (y0Vec + (thetaVec * (y1Vec - y0Vec))).CopyTo(yOut.Slice(i, width));
            }
        }

        for (; i < yOut.Length; i++)
        {
            yOut[i] = y0[i] + (theta * (y1[i] - y0[i]));
        }
    }
}

