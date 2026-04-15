using LAL.Core;
using System.Numerics;

namespace LAL.TensorCore;

internal static class UFuncArithmetic
{
    public static void Add(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Span<double> destination)
    {
        ValidateLengths(left.Length, right.Length, destination.Length);

        DataStructurePerformanceSettings settings = DataStructureCompatibility.GetPerformanceSettings();
        PerformancePrimitives.Add(left, right, destination, settings);
    }

    public static void Subtract(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Span<double> destination)
    {
        ValidateLengths(left.Length, right.Length, destination.Length);

        DataStructurePerformanceSettings settings = DataStructureCompatibility.GetPerformanceSettings();
        PerformancePrimitives.Subtract(left, right, destination, settings);
    }

    public static void Multiply(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Span<double> destination)
    {
        ValidateLengths(left.Length, right.Length, destination.Length);

        DataStructurePerformanceSettings settings = DataStructureCompatibility.GetPerformanceSettings();
        PerformancePrimitives.Multiply(left, right, destination, settings);
    }

    public static void Divide(ReadOnlySpan<double> numerator, ReadOnlySpan<double> denominator, Span<double> destination)
    {
        ValidateLengths(numerator.Length, denominator.Length, destination.Length);

        DataStructurePerformanceSettings settings = DataStructureCompatibility.GetPerformanceSettings();
        PerformancePrimitives.Divide(numerator, denominator, destination, settings);
    }

    public static void Power(ReadOnlySpan<double> basis, double exponent, Span<double> destination)
    {
        ValidateLengths(basis.Length, basis.Length, destination.Length);

        for (int i = 0; i < destination.Length; i++)
        {
            destination[i] = Math.Pow(basis[i], exponent);
        }
    }

    public static void Power(ReadOnlySpan<double> basis, ReadOnlySpan<double> exponent, Span<double> destination)
    {
        ValidateLengths(basis.Length, exponent.Length, destination.Length);

        for (int i = 0; i < destination.Length; i++)
        {
            destination[i] = Math.Pow(basis[i], exponent[i]);
        }
    }

    public static void Add(ReadOnlySpan<float> left, ReadOnlySpan<float> right, Span<float> destination)
    {
        ValidateLengths(left.Length, right.Length, destination.Length);

        DataStructurePerformanceSettings settings = DataStructureCompatibility.GetPerformanceSettings();
        PerformancePrimitives.Add(left, right, destination, settings);
    }

    public static void Subtract(ReadOnlySpan<float> left, ReadOnlySpan<float> right, Span<float> destination)
    {
        ValidateLengths(left.Length, right.Length, destination.Length);

        DataStructurePerformanceSettings settings = DataStructureCompatibility.GetPerformanceSettings();
        PerformancePrimitives.Subtract(left, right, destination, settings);
    }

    public static void Multiply(ReadOnlySpan<float> left, ReadOnlySpan<float> right, Span<float> destination)
    {
        ValidateLengths(left.Length, right.Length, destination.Length);

        DataStructurePerformanceSettings settings = DataStructureCompatibility.GetPerformanceSettings();
        PerformancePrimitives.Multiply(left, right, destination, settings);
    }

    public static void Divide(ReadOnlySpan<float> numerator, ReadOnlySpan<float> denominator, Span<float> destination)
    {
        ValidateLengths(numerator.Length, denominator.Length, destination.Length);

        DataStructurePerformanceSettings settings = DataStructureCompatibility.GetPerformanceSettings();
        PerformancePrimitives.Divide(numerator, denominator, destination, settings);
    }

    public static void Power(ReadOnlySpan<float> basis, float exponent, Span<float> destination)
    {
        ValidateLengths(basis.Length, basis.Length, destination.Length);

        for (int i = 0; i < destination.Length; i++)
        {
            destination[i] = MathF.Pow(basis[i], exponent);
        }
    }

    public static void Power(ReadOnlySpan<float> basis, ReadOnlySpan<float> exponent, Span<float> destination)
    {
        ValidateLengths(basis.Length, exponent.Length, destination.Length);

        for (int i = 0; i < destination.Length; i++)
        {
            destination[i] = MathF.Pow(basis[i], exponent[i]);
        }
    }

    public static void Add(ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right, Span<Complex> destination)
    {
        ValidateLengths(left.Length, right.Length, destination.Length);
        for (int i = 0; i < destination.Length; i++)
        {
            destination[i] = left[i] + right[i];
        }
    }

    public static void Subtract(ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right, Span<Complex> destination)
    {
        ValidateLengths(left.Length, right.Length, destination.Length);
        for (int i = 0; i < destination.Length; i++)
        {
            destination[i] = left[i] - right[i];
        }
    }

    public static void Multiply(ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right, Span<Complex> destination)
    {
        ValidateLengths(left.Length, right.Length, destination.Length);
        for (int i = 0; i < destination.Length; i++)
        {
            destination[i] = left[i] * right[i];
        }
    }

    public static void Divide(ReadOnlySpan<Complex> numerator, ReadOnlySpan<Complex> denominator, Span<Complex> destination)
    {
        ValidateLengths(numerator.Length, denominator.Length, destination.Length);
        for (int i = 0; i < destination.Length; i++)
        {
            destination[i] = numerator[i] / denominator[i];
        }
    }

    public static void Power(ReadOnlySpan<Complex> basis, double exponent, Span<Complex> destination)
    {
        ValidateLengths(basis.Length, basis.Length, destination.Length);
        for (int i = 0; i < destination.Length; i++)
        {
            destination[i] = Complex.Pow(basis[i], exponent);
        }
    }

    public static void Power(ReadOnlySpan<Complex> basis, ReadOnlySpan<Complex> exponent, Span<Complex> destination)
    {
        ValidateLengths(basis.Length, exponent.Length, destination.Length);
        for (int i = 0; i < destination.Length; i++)
        {
            destination[i] = Complex.Pow(basis[i], exponent[i]);
        }
    }

    private static void ValidateLengths(int leftLength, int rightLength, int destinationLength)
    {
        if (leftLength != rightLength || leftLength != destinationLength)
        {
            throw new ArgumentException("Input and destination lengths must match.");
        }
    }
}

