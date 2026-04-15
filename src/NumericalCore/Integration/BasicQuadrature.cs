using System.Numerics;

namespace LAL.NumericalCore.Integration;

internal static class BasicQuadrature
{
    public static double Trapezoidal(Func<double, double> f, double a, double b, int intervals)
    {
        if (f is null)
        {
            throw new ArgumentNullException(nameof(f));
        }

        if (intervals <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(intervals), "Intervals must be positive.");
        }

        double h = (b - a) / intervals;
        double sum = 0.5 * (f(a) + f(b));

        for (int i = 1; i < intervals; i++)
        {
            sum += f(a + (i * h));
        }

        return h * sum;
    }

    public static double Simpson(Func<double, double> f, double a, double b, int intervals)
    {
        if (f is null)
        {
            throw new ArgumentNullException(nameof(f));
        }

        if (intervals <= 0 || (intervals % 2) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(intervals), "Intervals must be a positive even number for Simpson's rule.");
        }

        double h = (b - a) / intervals;
        double sum = f(a) + f(b);

        for (int i = 1; i < intervals; i++)
        {
            double weight = (i % 2) == 0 ? 2.0 : 4.0;
            sum += weight * f(a + (i * h));
        }

        return (h / 3.0) * sum;
    }

    public static float Trapezoidal(Func<float, float> f, float a, float b, int intervals)
    {
        if (f is null)
        {
            throw new ArgumentNullException(nameof(f));
        }

        if (intervals <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(intervals), "Intervals must be positive.");
        }

        float h = (b - a) / intervals;
        float sum = 0.5f * (f(a) + f(b));

        for (int i = 1; i < intervals; i++)
        {
            sum += f(a + (i * h));
        }

        return h * sum;
    }

    public static float Simpson(Func<float, float> f, float a, float b, int intervals)
    {
        if (f is null)
        {
            throw new ArgumentNullException(nameof(f));
        }

        if (intervals <= 0 || (intervals % 2) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(intervals), "Intervals must be a positive even number for Simpson's rule.");
        }

        float h = (b - a) / intervals;
        float sum = f(a) + f(b);

        for (int i = 1; i < intervals; i++)
        {
            float weight = (i % 2) == 0 ? 2f : 4f;
            sum += weight * f(a + (i * h));
        }

        return (h / 3f) * sum;
    }

    public static Complex Trapezoidal(Func<double, Complex> f, double a, double b, int intervals)
    {
        if (f is null)
        {
            throw new ArgumentNullException(nameof(f));
        }

        if (intervals <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(intervals), "Intervals must be positive.");
        }

        double h = (b - a) / intervals;
        Complex sum = 0.5d * (f(a) + f(b));

        for (int i = 1; i < intervals; i++)
        {
            sum += f(a + (i * h));
        }

        return h * sum;
    }

    public static Complex Simpson(Func<double, Complex> f, double a, double b, int intervals)
    {
        if (f is null)
        {
            throw new ArgumentNullException(nameof(f));
        }

        if (intervals <= 0 || (intervals % 2) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(intervals), "Intervals must be a positive even number for Simpson's rule.");
        }

        double h = (b - a) / intervals;
        Complex sum = f(a) + f(b);

        for (int i = 1; i < intervals; i++)
        {
            double weight = (i % 2) == 0 ? 2d : 4d;
            sum += weight * f(a + (i * h));
        }

        return (h / 3d) * sum;
    }
}

