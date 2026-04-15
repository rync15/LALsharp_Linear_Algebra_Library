using System.Buffers;
using System.Numerics;

namespace LAL.NumericalCore.Interpolation;

internal static class Spline
{
    public static void ComputeNaturalSecondDerivatives(ReadOnlySpan<double> xs, ReadOnlySpan<double> ys, Span<double> secondDerivatives)
    {
        if (xs.Length != ys.Length || xs.Length != secondDerivatives.Length)
        {
            throw new ArgumentException("Input/output lengths must match.");
        }

        int n = xs.Length;
        if (n < 2)
        {
            throw new ArgumentException("At least two points are required.", nameof(xs));
        }

        ValidateStrictlyIncreasing(xs);

        double[]? rentedU = null;
        Span<double> u = n <= 256
            ? stackalloc double[n - 1]
            : (rentedU = ArrayPool<double>.Shared.Rent(n - 1)).AsSpan(0, n - 1);

        try
        {
            secondDerivatives[0] = 0d;
            u[0] = 0d;

            for (int i = 1; i < n - 1; i++)
            {
                double sig = (xs[i] - xs[i - 1]) / (xs[i + 1] - xs[i - 1]);
                double p = (sig * secondDerivatives[i - 1]) + 2d;
                secondDerivatives[i] = (sig - 1d) / p;

                double slopeNext = (ys[i + 1] - ys[i]) / (xs[i + 1] - xs[i]);
                double slopePrev = (ys[i] - ys[i - 1]) / (xs[i] - xs[i - 1]);
                double rhs = (6d * (slopeNext - slopePrev) / (xs[i + 1] - xs[i - 1])) - (sig * u[i - 1]);
                u[i] = rhs / p;
            }

            secondDerivatives[n - 1] = 0d;
            for (int k = n - 2; k >= 0; k--)
            {
                secondDerivatives[k] = (secondDerivatives[k] * secondDerivatives[k + 1]) + u[k];
            }
        }
        finally
        {
            if (rentedU is not null)
            {
                ArrayPool<double>.Shared.Return(rentedU, clearArray: false);
            }
        }
    }

    public static double EvaluateNaturalCubic(
        ReadOnlySpan<double> xs,
        ReadOnlySpan<double> ys,
        ReadOnlySpan<double> secondDerivatives,
        double x)
    {
        if (xs.Length != ys.Length || xs.Length != secondDerivatives.Length)
        {
            throw new ArgumentException("Input lengths must match.");
        }

        int n = xs.Length;
        if (n < 2)
        {
            throw new ArgumentException("At least two points are required.", nameof(xs));
        }

        if (x < xs[0] || x > xs[n - 1])
        {
            throw new ArgumentOutOfRangeException(nameof(x), "Evaluation point must be within the spline domain.");
        }

        int lo = 0;
        int hi = n - 1;
        while (hi - lo > 1)
        {
            int mid = (lo + hi) / 2;
            if (xs[mid] > x)
            {
                hi = mid;
            }
            else
            {
                lo = mid;
            }
        }

        double h = xs[hi] - xs[lo];
        if (h <= 0)
        {
            throw new ArgumentException("x values must be strictly increasing.", nameof(xs));
        }

        double a = (xs[hi] - x) / h;
        double b = (x - xs[lo]) / h;

        return (a * ys[lo])
             + (b * ys[hi])
             + ((((a * a * a) - a) * secondDerivatives[lo] + ((b * b * b) - b) * secondDerivatives[hi]) * (h * h) / 6d);
    }

    public static void ComputeNaturalSecondDerivatives(ReadOnlySpan<float> xs, ReadOnlySpan<float> ys, Span<float> secondDerivatives)
    {
        if (xs.Length != ys.Length || xs.Length != secondDerivatives.Length)
        {
            throw new ArgumentException("Input/output lengths must match.");
        }

        int n = xs.Length;
        if (n < 2)
        {
            throw new ArgumentException("At least two points are required.", nameof(xs));
        }

        ValidateStrictlyIncreasing(xs);

        float[]? rentedU = null;
        Span<float> u = n <= 256
            ? stackalloc float[n - 1]
            : (rentedU = ArrayPool<float>.Shared.Rent(n - 1)).AsSpan(0, n - 1);

        try
        {
            secondDerivatives[0] = 0f;
            u[0] = 0f;

            for (int i = 1; i < n - 1; i++)
            {
                float sig = (xs[i] - xs[i - 1]) / (xs[i + 1] - xs[i - 1]);
                float p = (sig * secondDerivatives[i - 1]) + 2f;
                secondDerivatives[i] = (sig - 1f) / p;

                float slopeNext = (ys[i + 1] - ys[i]) / (xs[i + 1] - xs[i]);
                float slopePrev = (ys[i] - ys[i - 1]) / (xs[i] - xs[i - 1]);
                float rhs = (6f * (slopeNext - slopePrev) / (xs[i + 1] - xs[i - 1])) - (sig * u[i - 1]);
                u[i] = rhs / p;
            }

            secondDerivatives[n - 1] = 0f;
            for (int k = n - 2; k >= 0; k--)
            {
                secondDerivatives[k] = (secondDerivatives[k] * secondDerivatives[k + 1]) + u[k];
            }
        }
        finally
        {
            if (rentedU is not null)
            {
                ArrayPool<float>.Shared.Return(rentedU, clearArray: false);
            }
        }
    }

    public static float EvaluateNaturalCubic(
        ReadOnlySpan<float> xs,
        ReadOnlySpan<float> ys,
        ReadOnlySpan<float> secondDerivatives,
        float x)
    {
        if (xs.Length != ys.Length || xs.Length != secondDerivatives.Length)
        {
            throw new ArgumentException("Input lengths must match.");
        }

        int n = xs.Length;
        if (n < 2)
        {
            throw new ArgumentException("At least two points are required.", nameof(xs));
        }

        if (x < xs[0] || x > xs[n - 1])
        {
            throw new ArgumentOutOfRangeException(nameof(x), "Evaluation point must be within the spline domain.");
        }

        int lo = 0;
        int hi = n - 1;
        while (hi - lo > 1)
        {
            int mid = (lo + hi) / 2;
            if (xs[mid] > x)
            {
                hi = mid;
            }
            else
            {
                lo = mid;
            }
        }

        float h = xs[hi] - xs[lo];
        if (h <= 0)
        {
            throw new ArgumentException("x values must be strictly increasing.", nameof(xs));
        }

        float a = (xs[hi] - x) / h;
        float b = (x - xs[lo]) / h;

        return (a * ys[lo])
             + (b * ys[hi])
             + ((((a * a * a) - a) * secondDerivatives[lo] + ((b * b * b) - b) * secondDerivatives[hi]) * (h * h) / 6f);
    }

    public static void ComputeNaturalSecondDerivatives(ReadOnlySpan<double> xs, ReadOnlySpan<Complex> ys, Span<Complex> secondDerivatives)
    {
        if (xs.Length != ys.Length || xs.Length != secondDerivatives.Length)
        {
            throw new ArgumentException("Input/output lengths must match.");
        }

        int n = xs.Length;
        if (n < 2)
        {
            throw new ArgumentException("At least two points are required.", nameof(xs));
        }

        ValidateStrictlyIncreasing(xs);

        Complex[]? rentedU = null;
        Span<Complex> u = n <= 128
            ? stackalloc Complex[n - 1]
            : (rentedU = ArrayPool<Complex>.Shared.Rent(n - 1)).AsSpan(0, n - 1);

        try
        {
            secondDerivatives[0] = Complex.Zero;
            u[0] = Complex.Zero;

            for (int i = 1; i < n - 1; i++)
            {
                double sig = (xs[i] - xs[i - 1]) / (xs[i + 1] - xs[i - 1]);
                Complex p = (sig * secondDerivatives[i - 1]) + 2d;
                secondDerivatives[i] = (sig - 1d) / p;

                Complex slopeNext = (ys[i + 1] - ys[i]) / (xs[i + 1] - xs[i]);
                Complex slopePrev = (ys[i] - ys[i - 1]) / (xs[i] - xs[i - 1]);
                Complex rhs = (6d * (slopeNext - slopePrev) / (xs[i + 1] - xs[i - 1])) - (sig * u[i - 1]);
                u[i] = rhs / p;
            }

            secondDerivatives[n - 1] = Complex.Zero;
            for (int k = n - 2; k >= 0; k--)
            {
                secondDerivatives[k] = (secondDerivatives[k] * secondDerivatives[k + 1]) + u[k];
            }
        }
        finally
        {
            if (rentedU is not null)
            {
                ArrayPool<Complex>.Shared.Return(rentedU, clearArray: false);
            }
        }
    }

    public static Complex EvaluateNaturalCubic(
        ReadOnlySpan<double> xs,
        ReadOnlySpan<Complex> ys,
        ReadOnlySpan<Complex> secondDerivatives,
        double x)
    {
        if (xs.Length != ys.Length || xs.Length != secondDerivatives.Length)
        {
            throw new ArgumentException("Input lengths must match.");
        }

        int n = xs.Length;
        if (n < 2)
        {
            throw new ArgumentException("At least two points are required.", nameof(xs));
        }

        if (x < xs[0] || x > xs[n - 1])
        {
            throw new ArgumentOutOfRangeException(nameof(x), "Evaluation point must be within the spline domain.");
        }

        int lo = 0;
        int hi = n - 1;
        while (hi - lo > 1)
        {
            int mid = (lo + hi) / 2;
            if (xs[mid] > x)
            {
                hi = mid;
            }
            else
            {
                lo = mid;
            }
        }

        double h = xs[hi] - xs[lo];
        if (h <= 0)
        {
            throw new ArgumentException("x values must be strictly increasing.", nameof(xs));
        }

        double a = (xs[hi] - x) / h;
        double b = (x - xs[lo]) / h;

        return (a * ys[lo])
             + (b * ys[hi])
             + ((((a * a * a) - a) * secondDerivatives[lo] + ((b * b * b) - b) * secondDerivatives[hi]) * (h * h) / 6d);
    }

    private static void ValidateStrictlyIncreasing(ReadOnlySpan<double> xs)
    {
        for (int i = 1; i < xs.Length; i++)
        {
            if (xs[i] <= xs[i - 1])
            {
                throw new ArgumentException("x values must be strictly increasing.", nameof(xs));
            }
        }
    }

    private static void ValidateStrictlyIncreasing(ReadOnlySpan<float> xs)
    {
        for (int i = 1; i < xs.Length; i++)
        {
            if (xs[i] <= xs[i - 1])
            {
                throw new ArgumentException("x values must be strictly increasing.", nameof(xs));
            }
        }
    }
}

