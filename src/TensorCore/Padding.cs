using System.Numerics;

namespace LAL.TensorCore;

internal static class Padding
{
    public static double[] ZeroPad1D(ReadOnlySpan<double> input, int left, int right)
    {
        return ZeroPad1DCore(input, left, right);
    }

    public static float[] ZeroPad1D(ReadOnlySpan<float> input, int left, int right)
    {
        return ZeroPad1DCore(input, left, right);
    }

    public static Complex[] ZeroPad1D(ReadOnlySpan<Complex> input, int left, int right)
    {
        return ZeroPad1DCore(input, left, right);
    }

    public static double[] EdgePad1D(ReadOnlySpan<double> input, int left, int right)
    {
        return EdgePad1DCore(input, left, right);
    }

    public static float[] EdgePad1D(ReadOnlySpan<float> input, int left, int right)
    {
        return EdgePad1DCore(input, left, right);
    }

    public static Complex[] EdgePad1D(ReadOnlySpan<Complex> input, int left, int right)
    {
        return EdgePad1DCore(input, left, right);
    }

    public static double[] PeriodicPad1D(ReadOnlySpan<double> input, int left, int right)
    {
        return PeriodicPad1DCore(input, left, right);
    }

    public static float[] PeriodicPad1D(ReadOnlySpan<float> input, int left, int right)
    {
        return PeriodicPad1DCore(input, left, right);
    }

    public static Complex[] PeriodicPad1D(ReadOnlySpan<Complex> input, int left, int right)
    {
        return PeriodicPad1DCore(input, left, right);
    }

    private static T[] ZeroPad1DCore<T>(ReadOnlySpan<T> input, int left, int right)
    {
        ValidatePadding(left, right);
        T[] result = new T[left + input.Length + right];
        input.CopyTo(result.AsSpan(left));
        return result;
    }

    private static T[] EdgePad1DCore<T>(ReadOnlySpan<T> input, int left, int right)
    {
        ValidatePadding(left, right);
        if (input.IsEmpty)
        {
            throw new ArgumentException("Input must not be empty for edge padding.", nameof(input));
        }

        T[] result = new T[left + input.Length + right];
        input.CopyTo(result.AsSpan(left));

        for (int i = 0; i < left; i++)
        {
            result[i] = input[0];
        }

        for (int i = 0; i < right; i++)
        {
            result[left + input.Length + i] = input[input.Length - 1];
        }

        return result;
    }

    private static T[] PeriodicPad1DCore<T>(ReadOnlySpan<T> input, int left, int right)
    {
        ValidatePadding(left, right);
        if (input.IsEmpty)
        {
            throw new ArgumentException("Input must not be empty for periodic padding.", nameof(input));
        }

        T[] result = new T[left + input.Length + right];
        input.CopyTo(result.AsSpan(left));

        for (int i = 0; i < left; i++)
        {
            int src = Mod(input.Length - left + i, input.Length);
            result[i] = input[src];
        }

        for (int i = 0; i < right; i++)
        {
            result[left + input.Length + i] = input[i % input.Length];
        }

        return result;
    }

    private static int Mod(int value, int modulo)
    {
        int r = value % modulo;
        return r < 0 ? r + modulo : r;
    }

    private static void ValidatePadding(int left, int right)
    {
        if (left < 0 || right < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(left), "Padding values must be non-negative.");
        }
    }
}

