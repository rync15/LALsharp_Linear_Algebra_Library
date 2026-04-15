using System.Numerics;

namespace LAL.NumericalCore.RootFinding;

public readonly record struct SecantResult(double Root, int Iterations, bool Converged);
public readonly record struct SecantResultFloat(float Root, int Iterations, bool Converged);
public readonly record struct SecantResultComplex(Complex Root, int Iterations, bool Converged);

internal static class Secant
{
    public static SecantResult Solve(
        Func<double, double> f,
        double x0,
        double x1,
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

        double prev = x0;
        double current = x1;
        double fPrev = f(prev);
        double fCurrent = f(current);

        for (int i = 1; i <= maxIterations; i++)
        {
            double denominator = fCurrent - fPrev;
            if (Math.Abs(denominator) <= double.Epsilon)
            {
                return new SecantResult(current, i, false);
            }

            double next = current - (fCurrent * (current - prev) / denominator);
            double fNext = f(next);

            if (Math.Abs(next - current) <= tolerance || Math.Abs(fNext) <= tolerance)
            {
                return new SecantResult(next, i, true);
            }

            prev = current;
            current = next;
            fPrev = fCurrent;
            fCurrent = fNext;
        }

        return new SecantResult(current, maxIterations, false);
    }

    public static SecantResultFloat Solve(
        Func<float, float> f,
        float x0,
        float x1,
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

        float prev = x0;
        float current = x1;
        float fPrev = f(prev);
        float fCurrent = f(current);

        for (int i = 1; i <= maxIterations; i++)
        {
            float denominator = fCurrent - fPrev;
            if (MathF.Abs(denominator) <= float.Epsilon)
            {
                return new SecantResultFloat(current, i, false);
            }

            float next = current - (fCurrent * (current - prev) / denominator);
            float fNext = f(next);

            if (MathF.Abs(next - current) <= tolerance || MathF.Abs(fNext) <= tolerance)
            {
                return new SecantResultFloat(next, i, true);
            }

            prev = current;
            current = next;
            fPrev = fCurrent;
            fCurrent = fNext;
        }

        return new SecantResultFloat(current, maxIterations, false);
    }

    public static SecantResultComplex Solve(
        Func<Complex, Complex> f,
        Complex x0,
        Complex x1,
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

        Complex prev = x0;
        Complex current = x1;
        Complex fPrev = f(prev);
        Complex fCurrent = f(current);

        for (int i = 1; i <= maxIterations; i++)
        {
            Complex denominator = fCurrent - fPrev;
            if (Complex.Abs(denominator) <= double.Epsilon)
            {
                return new SecantResultComplex(current, i, false);
            }

            Complex next = current - (fCurrent * (current - prev) / denominator);
            Complex fNext = f(next);

            if (Complex.Abs(next - current) <= tolerance || Complex.Abs(fNext) <= tolerance)
            {
                return new SecantResultComplex(next, i, true);
            }

            prev = current;
            current = next;
            fPrev = fCurrent;
            fCurrent = fNext;
        }

        return new SecantResultComplex(current, maxIterations, false);
    }
}

