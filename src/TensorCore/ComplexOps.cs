using System.Numerics;

namespace LAL.TensorCore;

internal static class ComplexOps
{
    public static void Re(ReadOnlySpan<Complex> values, Span<double> destination)
    {
        Real(values, destination);
    }

    public static void Im(ReadOnlySpan<Complex> values, Span<double> destination)
    {
        Imaginary(values, destination);
    }

    public static void Real(ReadOnlySpan<Complex> values, Span<double> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = values[i].Real;
        }
    }

    public static void Imaginary(ReadOnlySpan<Complex> values, Span<double> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = values[i].Imaginary;
        }
    }

    public static void Conjugate(ReadOnlySpan<Complex> values, Span<Complex> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = Complex.Conjugate(values[i]);
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
