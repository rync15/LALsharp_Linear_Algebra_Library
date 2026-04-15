using System.Numerics;

namespace LAL.TensorCore;

internal static class UFuncTranscendental
{
    public static void Exp(ReadOnlySpan<double> values, Span<double> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = Math.Exp(values[i]);
        }
    }

    public static void Log(ReadOnlySpan<double> values, Span<double> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = Math.Log(values[i]);
        }
    }

    public static void Sin(ReadOnlySpan<double> values, Span<double> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = Math.Sin(values[i]);
        }
    }

    public static void Cos(ReadOnlySpan<double> values, Span<double> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = Math.Cos(values[i]);
        }
    }

    public static void Tan(ReadOnlySpan<double> values, Span<double> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = Math.Tan(values[i]);
        }
    }

    public static void Asin(ReadOnlySpan<double> values, Span<double> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = Math.Asin(values[i]);
        }
    }

    public static void Acos(ReadOnlySpan<double> values, Span<double> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = Math.Acos(values[i]);
        }
    }

    public static void Atan(ReadOnlySpan<double> values, Span<double> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = Math.Atan(values[i]);
        }
    }

    public static void Ln(ReadOnlySpan<double> values, Span<double> destination)
    {
        Log(values, destination);
    }

    public static void Sinh(ReadOnlySpan<double> values, Span<double> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = Math.Sinh(values[i]);
        }
    }

    public static void Cosh(ReadOnlySpan<double> values, Span<double> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = Math.Cosh(values[i]);
        }
    }

    public static void Tanh(ReadOnlySpan<double> values, Span<double> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = Math.Tanh(values[i]);
        }
    }

    public static void Exp(ReadOnlySpan<float> values, Span<float> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = MathF.Exp(values[i]);
        }
    }

    public static void Log(ReadOnlySpan<float> values, Span<float> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = MathF.Log(values[i]);
        }
    }

    public static void Sin(ReadOnlySpan<float> values, Span<float> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = MathF.Sin(values[i]);
        }
    }

    public static void Cos(ReadOnlySpan<float> values, Span<float> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = MathF.Cos(values[i]);
        }
    }

    public static void Tan(ReadOnlySpan<float> values, Span<float> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = MathF.Tan(values[i]);
        }
    }

    public static void Asin(ReadOnlySpan<float> values, Span<float> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = MathF.Asin(values[i]);
        }
    }

    public static void Acos(ReadOnlySpan<float> values, Span<float> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = MathF.Acos(values[i]);
        }
    }

    public static void Atan(ReadOnlySpan<float> values, Span<float> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = MathF.Atan(values[i]);
        }
    }

    public static void Ln(ReadOnlySpan<float> values, Span<float> destination)
    {
        Log(values, destination);
    }

    public static void Sinh(ReadOnlySpan<float> values, Span<float> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = MathF.Sinh(values[i]);
        }
    }

    public static void Cosh(ReadOnlySpan<float> values, Span<float> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = MathF.Cosh(values[i]);
        }
    }

    public static void Tanh(ReadOnlySpan<float> values, Span<float> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = MathF.Tanh(values[i]);
        }
    }

    public static void Exp(ReadOnlySpan<Complex> values, Span<Complex> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = Complex.Exp(values[i]);
        }
    }

    public static void Log(ReadOnlySpan<Complex> values, Span<Complex> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = Complex.Log(values[i]);
        }
    }

    public static void Sin(ReadOnlySpan<Complex> values, Span<Complex> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = Complex.Sin(values[i]);
        }
    }

    public static void Cos(ReadOnlySpan<Complex> values, Span<Complex> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = Complex.Cos(values[i]);
        }
    }

    public static void Tan(ReadOnlySpan<Complex> values, Span<Complex> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = Complex.Tan(values[i]);
        }
    }

    public static void Asin(ReadOnlySpan<Complex> values, Span<Complex> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = Complex.Asin(values[i]);
        }
    }

    public static void Acos(ReadOnlySpan<Complex> values, Span<Complex> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = Complex.Acos(values[i]);
        }
    }

    public static void Atan(ReadOnlySpan<Complex> values, Span<Complex> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = Complex.Atan(values[i]);
        }
    }

    public static void Ln(ReadOnlySpan<Complex> values, Span<Complex> destination)
    {
        Log(values, destination);
    }

    public static void Sinh(ReadOnlySpan<Complex> values, Span<Complex> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = Complex.Sinh(values[i]);
        }
    }

    public static void Cosh(ReadOnlySpan<Complex> values, Span<Complex> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = Complex.Cosh(values[i]);
        }
    }

    public static void Tanh(ReadOnlySpan<Complex> values, Span<Complex> destination)
    {
        Validate(values.Length, destination.Length);
        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = Complex.Tanh(values[i]);
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

