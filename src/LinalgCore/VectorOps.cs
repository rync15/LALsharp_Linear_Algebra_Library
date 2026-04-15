using System.Numerics;

namespace LAL.LinalgCore;

internal static class VectorOps
{
    public static void Add(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Span<double> destination)
    {
        ValidateBinary(left, right, destination);

        for (int i = 0; i < destination.Length; i++)
        {
            destination[i] = left[i] + right[i];
        }
    }

    public static void Subtract(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Span<double> destination)
    {
        ValidateBinary(left, right, destination);

        for (int i = 0; i < destination.Length; i++)
        {
            destination[i] = left[i] - right[i];
        }
    }

    public static double Dot(ReadOnlySpan<double> left, ReadOnlySpan<double> right)
    {
        return global::LAL.LinalgCore.Dot.Dotu(left, right);
    }

    public static double InnerProduct(ReadOnlySpan<double> left, ReadOnlySpan<double> right)
    {
        return Dot(left, right);
    }

    public static void Add(ReadOnlySpan<float> left, ReadOnlySpan<float> right, Span<float> destination)
    {
        ValidateBinary(left, right, destination);

        for (int i = 0; i < destination.Length; i++)
        {
            destination[i] = left[i] + right[i];
        }
    }

    public static void Subtract(ReadOnlySpan<float> left, ReadOnlySpan<float> right, Span<float> destination)
    {
        ValidateBinary(left, right, destination);

        for (int i = 0; i < destination.Length; i++)
        {
            destination[i] = left[i] - right[i];
        }
    }

    public static float Dot(ReadOnlySpan<float> left, ReadOnlySpan<float> right)
    {
        return global::LAL.LinalgCore.Dot.Dotu(left, right);
    }

    public static float InnerProduct(ReadOnlySpan<float> left, ReadOnlySpan<float> right)
    {
        return Dot(left, right);
    }

    public static void Add(ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right, Span<Complex> destination)
    {
        ValidateBinary(left, right, destination);

        for (int i = 0; i < destination.Length; i++)
        {
            destination[i] = left[i] + right[i];
        }
    }

    public static void Subtract(ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right, Span<Complex> destination)
    {
        ValidateBinary(left, right, destination);

        for (int i = 0; i < destination.Length; i++)
        {
            destination[i] = left[i] - right[i];
        }
    }

    public static Complex Dot(ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right)
    {
        return global::LAL.LinalgCore.Dot.Dotu(left, right);
    }

    public static Complex InnerProduct(ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right)
    {
        return Dot(left, right);
    }

    public static void OuterProduct(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Span<double> destination)
    {
        if (destination.Length != left.Length * right.Length)
        {
            throw new ArgumentException("Destination length must be left.Length * right.Length.", nameof(destination));
        }

        for (int i = 0; i < left.Length; i++)
        {
            int rowOffset = i * right.Length;
            for (int j = 0; j < right.Length; j++)
            {
                destination[rowOffset + j] = left[i] * right[j];
            }
        }
    }

    public static void OuterProduct(ReadOnlySpan<float> left, ReadOnlySpan<float> right, Span<float> destination)
    {
        if (destination.Length != left.Length * right.Length)
        {
            throw new ArgumentException("Destination length must be left.Length * right.Length.", nameof(destination));
        }

        for (int i = 0; i < left.Length; i++)
        {
            int rowOffset = i * right.Length;
            for (int j = 0; j < right.Length; j++)
            {
                destination[rowOffset + j] = left[i] * right[j];
            }
        }
    }

    public static void OuterProduct(ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right, Span<Complex> destination)
    {
        if (destination.Length != left.Length * right.Length)
        {
            throw new ArgumentException("Destination length must be left.Length * right.Length.", nameof(destination));
        }

        for (int i = 0; i < left.Length; i++)
        {
            int rowOffset = i * right.Length;
            for (int j = 0; j < right.Length; j++)
            {
                destination[rowOffset + j] = left[i] * right[j];
            }
        }
    }

    private static void ValidateBinary(ReadOnlySpan<float> left, ReadOnlySpan<float> right, Span<float> destination)
    {
        if (left.Length != right.Length || destination.Length != left.Length)
        {
            throw new ArgumentException("Left, right, and destination lengths must match.");
        }
    }

    private static void ValidateBinary(ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right, Span<Complex> destination)
    {
        if (left.Length != right.Length || destination.Length != left.Length)
        {
            throw new ArgumentException("Left, right, and destination lengths must match.");
        }
    }

    private static void ValidateBinary(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Span<double> destination)
    {
        if (left.Length != right.Length || destination.Length != left.Length)
        {
            throw new ArgumentException("Left, right, and destination lengths must match.");
        }
    }
}
