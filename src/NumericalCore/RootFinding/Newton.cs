using System.Numerics;

namespace LAL.NumericalCore.RootFinding;

public readonly record struct NewtonResult(double Root, int Iterations, bool Converged);
public readonly record struct NewtonResultFloat(float Root, int Iterations, bool Converged);
public readonly record struct NewtonResultComplex(Complex Root, int Iterations, bool Converged);

internal static class Newton
{
    public static NewtonResult Solve(
        Func<double, double> f,
        Func<double, double> df,
        double initialGuess,
        double tolerance = 1e-10,
        int maxIterations = 100)
    {
        if (f is null)
        {
            throw new ArgumentNullException(nameof(f));
        }

        if (df is null)
        {
            throw new ArgumentNullException(nameof(df));
        }

        if (tolerance <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be positive.");
        }

        if (maxIterations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxIterations), "Max iterations must be positive.");
        }

        double x = initialGuess;

        for (int i = 1; i <= maxIterations; i++)
        {
            double fx = f(x);
            if (Math.Abs(fx) <= tolerance)
            {
                return new NewtonResult(x, i, true);
            }

            double dfx = df(x);
            if (Math.Abs(dfx) <= double.Epsilon)
            {
                return new NewtonResult(x, i, false);
            }

            double xNext = x - (fx / dfx);
            if (Math.Abs(xNext - x) <= tolerance)
            {
                return new NewtonResult(xNext, i, true);
            }

            x = xNext;
        }

        return new NewtonResult(x, maxIterations, false);
    }

    public static NewtonResultFloat Solve(
        Func<float, float> f,
        Func<float, float> df,
        float initialGuess,
        float tolerance = 1e-6f,
        int maxIterations = 100)
    {
        if (f is null)
        {
            throw new ArgumentNullException(nameof(f));
        }

        if (df is null)
        {
            throw new ArgumentNullException(nameof(df));
        }

        if (tolerance <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be positive.");
        }

        if (maxIterations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxIterations), "Max iterations must be positive.");
        }

        float x = initialGuess;

        for (int i = 1; i <= maxIterations; i++)
        {
            float fx = f(x);
            if (MathF.Abs(fx) <= tolerance)
            {
                return new NewtonResultFloat(x, i, true);
            }

            float dfx = df(x);
            if (MathF.Abs(dfx) <= float.Epsilon)
            {
                return new NewtonResultFloat(x, i, false);
            }

            float xNext = x - (fx / dfx);
            if (MathF.Abs(xNext - x) <= tolerance)
            {
                return new NewtonResultFloat(xNext, i, true);
            }

            x = xNext;
        }

        return new NewtonResultFloat(x, maxIterations, false);
    }

    public static NewtonResultComplex Solve(
        Func<Complex, Complex> f,
        Func<Complex, Complex> df,
        Complex initialGuess,
        double tolerance = 1e-10,
        int maxIterations = 100)
    {
        if (f is null)
        {
            throw new ArgumentNullException(nameof(f));
        }

        if (df is null)
        {
            throw new ArgumentNullException(nameof(df));
        }

        if (tolerance <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be positive.");
        }

        if (maxIterations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxIterations), "Max iterations must be positive.");
        }

        Complex x = initialGuess;

        for (int i = 1; i <= maxIterations; i++)
        {
            Complex fx = f(x);
            if (Complex.Abs(fx) <= tolerance)
            {
                return new NewtonResultComplex(x, i, true);
            }

            Complex dfx = df(x);
            if (Complex.Abs(dfx) <= double.Epsilon)
            {
                return new NewtonResultComplex(x, i, false);
            }

            Complex xNext = x - (fx / dfx);
            if (Complex.Abs(xNext - x) <= tolerance)
            {
                return new NewtonResultComplex(xNext, i, true);
            }

            x = xNext;
        }

        return new NewtonResultComplex(x, maxIterations, false);
    }
}

