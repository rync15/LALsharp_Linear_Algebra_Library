using System.Numerics;

namespace LAL.TensorCore;

internal static class Cumulative
{
    public static void CumSum(ReadOnlySpan<double> values, Span<double> destination)
    {
        Validate(values.Length, destination.Length);

        double sum = 0d;
        double c = 0d;
        for (int i = 0; i < values.Length; i++)
        {
            double y = values[i] - c;
            double t = sum + y;
            c = (t - sum) - y;
            sum = t;
            destination[i] = sum;
        }
    }

    public static void CumSum(ReadOnlySpan<float> values, Span<float> destination)
    {
        Validate(values.Length, destination.Length);

        float sum = 0f;
        float c = 0f;
        for (int i = 0; i < values.Length; i++)
        {
            float y = values[i] - c;
            float t = sum + y;
            c = (t - sum) - y;
            sum = t;
            destination[i] = sum;
        }
    }

    public static void CumSum(ReadOnlySpan<Complex> values, Span<Complex> destination)
    {
        Validate(values.Length, destination.Length);

        Complex sum = Complex.Zero;
        for (int i = 0; i < values.Length; i++)
        {
            sum += values[i];
            destination[i] = sum;
        }
    }

    public static void CumProd(ReadOnlySpan<double> values, Span<double> destination)
    {
        Validate(values.Length, destination.Length);

        double product = 1d;
        for (int i = 0; i < values.Length; i++)
        {
            product *= values[i];
            destination[i] = product;
        }
    }

    public static void CumProd(ReadOnlySpan<float> values, Span<float> destination)
    {
        Validate(values.Length, destination.Length);

        float product = 1f;
        for (int i = 0; i < values.Length; i++)
        {
            product *= values[i];
            destination[i] = product;
        }
    }

    public static void CumProd(ReadOnlySpan<Complex> values, Span<Complex> destination)
    {
        Validate(values.Length, destination.Length);

        Complex product = Complex.One;
        for (int i = 0; i < values.Length; i++)
        {
            product *= values[i];
            destination[i] = product;
        }
    }

    private static void Validate(int inputLength, int outputLength)
    {
        if (inputLength != outputLength)
        {
            throw new ArgumentException("Input and output lengths must match.");
        }
    }
}
