using System.Numerics;

namespace LAL.NumericalCore.Differentiation;

internal static class FiniteDifference
{
    public static double Forward(Func<double, double> f, double x, double h = 1e-6)
    {
        Validate(f, h);
        return (f(x + h) - f(x)) / h;
    }

    public static double Backward(Func<double, double> f, double x, double h = 1e-6)
    {
        Validate(f, h);
        return (f(x) - f(x - h)) / h;
    }

    public static double Central(Func<double, double> f, double x, double h = 1e-6)
    {
        Validate(f, h);
        return (f(x + h) - f(x - h)) / (2d * h);
    }

    public static float Forward(Func<float, float> f, float x, float h = 1e-4f)
    {
        Validate(f, h);
        return (f(x + h) - f(x)) / h;
    }

    public static float Backward(Func<float, float> f, float x, float h = 1e-4f)
    {
        Validate(f, h);
        return (f(x) - f(x - h)) / h;
    }

    public static float Central(Func<float, float> f, float x, float h = 1e-4f)
    {
        Validate(f, h);
        return (f(x + h) - f(x - h)) / (2f * h);
    }

    public static Complex Forward(Func<double, Complex> f, double x, double h = 1e-6)
    {
        Validate(f, h);
        return (f(x + h) - f(x)) / h;
    }

    public static Complex Backward(Func<double, Complex> f, double x, double h = 1e-6)
    {
        Validate(f, h);
        return (f(x) - f(x - h)) / h;
    }

    public static Complex Central(Func<double, Complex> f, double x, double h = 1e-6)
    {
        Validate(f, h);
        return (f(x + h) - f(x - h)) / (2d * h);
    }

    private static void Validate(Func<double, double> f, double h)
    {
        if (f is null)
        {
            throw new ArgumentNullException(nameof(f));
        }

        if (h <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(h), "Step size must be positive.");
        }
    }

    private static void Validate(Func<float, float> f, float h)
    {
        if (f is null)
        {
            throw new ArgumentNullException(nameof(f));
        }

        if (h <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(h), "Step size must be positive.");
        }
    }

    private static void Validate(Func<double, Complex> f, double h)
    {
        if (f is null)
        {
            throw new ArgumentNullException(nameof(f));
        }

        if (h <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(h), "Step size must be positive.");
        }
    }
}

