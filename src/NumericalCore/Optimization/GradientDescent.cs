using System.Numerics;

namespace LAL.NumericalCore.Optimization;

public readonly record struct GradientDescentResult(double X, double Fx, int Iterations, bool Converged);
public readonly record struct GradientDescentResultFloat(float X, float Fx, int Iterations, bool Converged);
public readonly record struct GradientDescentComplexResult(Complex X, double Fx, int Iterations, bool Converged);

internal static class GradientDescent
{
    public static GradientDescentResult SolveScalar(
        Func<double, double> f,
        Func<double, double> gradient,
        double initialX,
        double learningRate = 0.05,
        double tolerance = 1e-8,
        int maxIterations = 10_000)
    {
        if (f is null)
        {
            throw new ArgumentNullException(nameof(f));
        }

        if (gradient is null)
        {
            throw new ArgumentNullException(nameof(gradient));
        }

        if (learningRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(learningRate), "Learning rate must be positive.");
        }

        if (tolerance <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be positive.");
        }

        if (maxIterations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxIterations), "Max iterations must be positive.");
        }

        double x = initialX;

        for (int i = 1; i <= maxIterations; i++)
        {
            double g = gradient(x);
            if (Math.Abs(g) <= tolerance)
            {
                return new GradientDescentResult(x, f(x), i, true);
            }

            x -= learningRate * g;
        }

        return new GradientDescentResult(x, f(x), maxIterations, false);
    }

    public static GradientDescentResultFloat SolveScalar(
        Func<float, float> f,
        Func<float, float> gradient,
        float initialX,
        float learningRate = 0.05f,
        float tolerance = 1e-5f,
        int maxIterations = 10_000)
    {
        if (f is null)
        {
            throw new ArgumentNullException(nameof(f));
        }

        if (gradient is null)
        {
            throw new ArgumentNullException(nameof(gradient));
        }

        if (learningRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(learningRate), "Learning rate must be positive.");
        }

        if (tolerance <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be positive.");
        }

        if (maxIterations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxIterations), "Max iterations must be positive.");
        }

        float x = initialX;

        for (int i = 1; i <= maxIterations; i++)
        {
            float g = gradient(x);
            if (MathF.Abs(g) <= tolerance)
            {
                return new GradientDescentResultFloat(x, f(x), i, true);
            }

            x -= learningRate * g;
        }

        return new GradientDescentResultFloat(x, f(x), maxIterations, false);
    }

    public static GradientDescentComplexResult SolveScalar(
        Func<Complex, double> f,
        Func<Complex, Complex> gradient,
        Complex initialX,
        double learningRate = 0.05,
        double tolerance = 1e-8,
        int maxIterations = 10_000)
    {
        if (f is null)
        {
            throw new ArgumentNullException(nameof(f));
        }

        if (gradient is null)
        {
            throw new ArgumentNullException(nameof(gradient));
        }

        if (learningRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(learningRate), "Learning rate must be positive.");
        }

        if (tolerance <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be positive.");
        }

        if (maxIterations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxIterations), "Max iterations must be positive.");
        }

        Complex x = initialX;

        for (int i = 1; i <= maxIterations; i++)
        {
            Complex g = gradient(x);
            if (Complex.Abs(g) <= tolerance)
            {
                return new GradientDescentComplexResult(x, f(x), i, true);
            }

            x -= learningRate * g;
        }

        return new GradientDescentComplexResult(x, f(x), maxIterations, false);
    }
}

