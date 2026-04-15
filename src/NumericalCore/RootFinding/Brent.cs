using System.Numerics;

namespace LAL.NumericalCore.RootFinding;

public readonly record struct BrentResult(double Root, int Iterations, bool Converged);
public readonly record struct BrentResultFloat(float Root, int Iterations, bool Converged);
public readonly record struct BrentResultComplex(Complex Root, int Iterations, bool Converged);

internal static class Brent
{
    public static BrentResult Solve(
        Func<double, double> f,
        double lower,
        double upper,
        double tolerance = 1e-10,
        int maxIterations = 100)
    {
        if (f is null)
        {
            throw new ArgumentNullException(nameof(f));
        }

        if (tolerance <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be positive.");
        }

        if (maxIterations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxIterations), "Max iterations must be positive.");
        }

        double a = lower;
        double b = upper;
        double fa = f(a);
        double fb = f(b);

        if (fa * fb > 0)
        {
            throw new ArgumentException("Function values at bounds must have opposite signs.");
        }

        if (Math.Abs(fa) < Math.Abs(fb))
        {
            Swap(ref a, ref b);
            Swap(ref fa, ref fb);
        }

        double c = a;
        double fc = fa;
        bool mflag = true;
        double d = 0;

        for (int iter = 1; iter <= maxIterations; iter++)
        {
            if (Math.Abs(fb) <= tolerance)
            {
                return new BrentResult(b, iter, true);
            }

            double s;
            if (Math.Abs(fa - fc) > double.Epsilon && Math.Abs(fb - fc) > double.Epsilon)
            {
                s = (a * fb * fc) / ((fa - fb) * (fa - fc))
                  + (b * fa * fc) / ((fb - fa) * (fb - fc))
                  + (c * fa * fb) / ((fc - fa) * (fc - fb));
            }
            else
            {
                s = b - fb * (b - a) / (fb - fa);
            }

            double cond1 = (3 * a + b) / 4;
            bool condition = (s < Math.Min(cond1, b) || s > Math.Max(cond1, b))
                             || (mflag && Math.Abs(s - b) >= Math.Abs(b - c) / 2)
                             || (!mflag && Math.Abs(s - b) >= Math.Abs(c - d) / 2)
                             || (mflag && Math.Abs(b - c) < tolerance)
                             || (!mflag && Math.Abs(c - d) < tolerance);

            if (condition)
            {
                s = (a + b) / 2;
                mflag = true;
            }
            else
            {
                mflag = false;
            }

            double fs = f(s);
            d = c;
            c = b;
            fc = fb;

            if (fa * fs < 0)
            {
                b = s;
                fb = fs;
            }
            else
            {
                a = s;
                fa = fs;
            }

            if (Math.Abs(fa) < Math.Abs(fb))
            {
                Swap(ref a, ref b);
                Swap(ref fa, ref fb);
            }

            if (Math.Abs(b - a) <= tolerance)
            {
                return new BrentResult(b, iter, true);
            }
        }

        return new BrentResult(b, maxIterations, false);
    }

    private static void Swap(ref double x, ref double y)
    {
        (x, y) = (y, x);
    }

    public static BrentResultFloat Solve(
        Func<float, float> f,
        float lower,
        float upper,
        float tolerance = 1e-6f,
        int maxIterations = 100)
    {
        if (f is null)
        {
            throw new ArgumentNullException(nameof(f));
        }

        if (tolerance <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be positive.");
        }

        if (maxIterations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxIterations), "Max iterations must be positive.");
        }

        float a = lower;
        float b = upper;
        float fa = f(a);
        float fb = f(b);

        if (fa * fb > 0)
        {
            throw new ArgumentException("Function values at bounds must have opposite signs.");
        }

        if (MathF.Abs(fa) < MathF.Abs(fb))
        {
            Swap(ref a, ref b);
            Swap(ref fa, ref fb);
        }

        float c = a;
        float fc = fa;
        bool mflag = true;
        float d = 0;

        for (int iter = 1; iter <= maxIterations; iter++)
        {
            if (MathF.Abs(fb) <= tolerance)
            {
                return new BrentResultFloat(b, iter, true);
            }

            float s;
            if (MathF.Abs(fa - fc) > float.Epsilon && MathF.Abs(fb - fc) > float.Epsilon)
            {
                s = (a * fb * fc) / ((fa - fb) * (fa - fc))
                  + (b * fa * fc) / ((fb - fa) * (fb - fc))
                  + (c * fa * fb) / ((fc - fa) * (fc - fb));
            }
            else
            {
                s = b - (fb * (b - a) / (fb - fa));
            }

            float cond1 = (3f * a + b) / 4f;
            bool condition = (s < MathF.Min(cond1, b) || s > MathF.Max(cond1, b))
                             || (mflag && MathF.Abs(s - b) >= MathF.Abs(b - c) / 2f)
                             || (!mflag && MathF.Abs(s - b) >= MathF.Abs(c - d) / 2f)
                             || (mflag && MathF.Abs(b - c) < tolerance)
                             || (!mflag && MathF.Abs(c - d) < tolerance);

            if (condition)
            {
                s = (a + b) / 2f;
                mflag = true;
            }
            else
            {
                mflag = false;
            }

            float fs = f(s);
            d = c;
            c = b;
            fc = fb;

            if (fa * fs < 0)
            {
                b = s;
                fb = fs;
            }
            else
            {
                a = s;
                fa = fs;
            }

            if (MathF.Abs(fa) < MathF.Abs(fb))
            {
                Swap(ref a, ref b);
                Swap(ref fa, ref fb);
            }

            if (MathF.Abs(b - a) <= tolerance)
            {
                return new BrentResultFloat(b, iter, true);
            }
        }

        return new BrentResultFloat(b, maxIterations, false);
    }

    private static void Swap(ref float x, ref float y)
    {
        (x, y) = (y, x);
    }

    public static BrentResultComplex Solve(
        Func<Complex, Complex> f,
        Complex x0,
        Complex x1,
        double tolerance = 1e-10,
        int maxIterations = 100)
    {
        SecantResultComplex fallback = Secant.Solve(f, x0, x1, tolerance, maxIterations);
        return new BrentResultComplex(fallback.Root, fallback.Iterations, fallback.Converged);
    }
}

