using System.Numerics;

namespace LAL.NumericalCore.Integration;

internal static class GaussianQuadrature
{
    public static double Integrate(Func<double, double> f, double a, double b, int order = 2)
    {
        if (f is null)
        {
            throw new ArgumentNullException(nameof(f));
        }

        if (order != 2 && order != 3)
        {
            throw new ArgumentOutOfRangeException(nameof(order), "Only order 2 or 3 Gauss-Legendre quadrature is supported in baseline.");
        }

        double half = 0.5 * (b - a);
        double center = 0.5 * (a + b);

        if (order == 2)
        {
            double t = 1d / Math.Sqrt(3d);
            double x1 = center - (half * t);
            double x2 = center + (half * t);
            return half * (f(x1) + f(x2));
        }

        double t3 = Math.Sqrt(3d / 5d);
        double sum = (5d / 9d) * f(center - (half * t3))
                   + (8d / 9d) * f(center)
                   + (5d / 9d) * f(center + (half * t3));
        return half * sum;
    }

    public static float Integrate(Func<float, float> f, float a, float b, int order = 2)
    {
        if (f is null)
        {
            throw new ArgumentNullException(nameof(f));
        }

        if (order != 2 && order != 3)
        {
            throw new ArgumentOutOfRangeException(nameof(order), "Only order 2 or 3 Gauss-Legendre quadrature is supported in baseline.");
        }

        float half = 0.5f * (b - a);
        float center = 0.5f * (a + b);

        if (order == 2)
        {
            float t = 1f / MathF.Sqrt(3f);
            float x1 = center - (half * t);
            float x2 = center + (half * t);
            return half * (f(x1) + f(x2));
        }

        float t3 = MathF.Sqrt(3f / 5f);
        float sum = (5f / 9f) * f(center - (half * t3))
                  + (8f / 9f) * f(center)
                  + (5f / 9f) * f(center + (half * t3));
        return half * sum;
    }

    public static Complex Integrate(Func<double, Complex> f, double a, double b, int order = 2)
    {
        if (f is null)
        {
            throw new ArgumentNullException(nameof(f));
        }

        if (order != 2 && order != 3)
        {
            throw new ArgumentOutOfRangeException(nameof(order), "Only order 2 or 3 Gauss-Legendre quadrature is supported in baseline.");
        }

        double half = 0.5d * (b - a);
        double center = 0.5d * (a + b);

        if (order == 2)
        {
            double t = 1d / Math.Sqrt(3d);
            double x1 = center - (half * t);
            double x2 = center + (half * t);
            return half * (f(x1) + f(x2));
        }

        double t3 = Math.Sqrt(3d / 5d);
        Complex sum = (5d / 9d) * f(center - (half * t3))
                    + (8d / 9d) * f(center)
                    + (5d / 9d) * f(center + (half * t3));
        return half * sum;
    }
}

