using System.Numerics;

namespace LAL.NumericalCore.Optimization;

public readonly record struct LbfgsResult(double X, double Fx, int Iterations, bool Converged);
public readonly record struct LbfgsResultFloat(float X, float Fx, int Iterations, bool Converged);
public readonly record struct LbfgsComplexResult(Complex X, double Fx, int Iterations, bool Converged);

internal static class Lbfgs
{
    public static LbfgsResult SolveScalar(
        Func<double, double> f,
        Func<double, double> gradient,
        double initialX,
        double tolerance = 1e-8,
        int maxIterations = 200,
        int historySize = 5)
    {
        if (f is null)
        {
            throw new ArgumentNullException(nameof(f));
        }

        if (gradient is null)
        {
            throw new ArgumentNullException(nameof(gradient));
        }

        if (tolerance <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be positive.");
        }

        if (maxIterations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxIterations), "Max iterations must be positive.");
        }

        if (historySize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(historySize), "History size must be positive.");
        }

        double x = initialX;
        double fx = f(x);
        double g = gradient(x);
        double inverseHessianScale = 1d;

        for (int iter = 1; iter <= maxIterations; iter++)
        {
            if (Math.Abs(g) <= tolerance)
            {
                return new LbfgsResult(x, fx, iter, true);
            }

            double direction = -inverseHessianScale * g;
            double step = 1d;
            double directionalDerivative = g * direction;

            for (int ls = 0; ls < 20; ls++)
            {
                double candidateX = x + (step * direction);
                double candidateFx = f(candidateX);

                if (candidateFx <= fx + (1e-4 * step * directionalDerivative))
                {
                    double candidateG = gradient(candidateX);

                    double s = candidateX - x;
                    double y = candidateG - g;
                    if (Math.Abs(y) > 1e-14)
                    {
                        inverseHessianScale = s / y;
                    }

                    x = candidateX;
                    fx = candidateFx;
                    g = candidateG;
                    break;
                }

                step *= 0.5;
            }
        }

        return new LbfgsResult(x, fx, maxIterations, false);
    }

    public static LbfgsResultFloat SolveScalar(
        Func<float, float> f,
        Func<float, float> gradient,
        float initialX,
        float tolerance = 1e-5f,
        int maxIterations = 200,
        int historySize = 5)
    {
        if (f is null)
        {
            throw new ArgumentNullException(nameof(f));
        }

        if (gradient is null)
        {
            throw new ArgumentNullException(nameof(gradient));
        }

        if (tolerance <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be positive.");
        }

        if (maxIterations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxIterations), "Max iterations must be positive.");
        }

        if (historySize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(historySize), "History size must be positive.");
        }

        float x = initialX;
        float fx = f(x);
        float g = gradient(x);
        float inverseHessianScale = 1f;

        for (int iter = 1; iter <= maxIterations; iter++)
        {
            if (MathF.Abs(g) <= tolerance)
            {
                return new LbfgsResultFloat(x, fx, iter, true);
            }

            float direction = -inverseHessianScale * g;
            float step = 1f;
            float directionalDerivative = g * direction;

            for (int ls = 0; ls < 20; ls++)
            {
                float candidateX = x + (step * direction);
                float candidateFx = f(candidateX);

                if (candidateFx <= fx + (1e-4f * step * directionalDerivative))
                {
                    float candidateG = gradient(candidateX);

                    float s = candidateX - x;
                    float y = candidateG - g;
                    if (MathF.Abs(y) > 1e-7f)
                    {
                        inverseHessianScale = s / y;
                    }

                    x = candidateX;
                    fx = candidateFx;
                    g = candidateG;
                    break;
                }

                step *= 0.5f;
            }
        }

        return new LbfgsResultFloat(x, fx, maxIterations, false);
    }

    public static LbfgsComplexResult SolveScalar(
        Func<Complex, double> f,
        Func<Complex, Complex> gradient,
        Complex initialX,
        double tolerance = 1e-8,
        int maxIterations = 200,
        int historySize = 5)
    {
        if (f is null)
        {
            throw new ArgumentNullException(nameof(f));
        }

        if (gradient is null)
        {
            throw new ArgumentNullException(nameof(gradient));
        }

        if (tolerance <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be positive.");
        }

        if (maxIterations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxIterations), "Max iterations must be positive.");
        }

        if (historySize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(historySize), "History size must be positive.");
        }

        Complex x = initialX;
        double fx = f(x);
        Complex g = gradient(x);
        double inverseHessianScale = 1d;

        for (int iter = 1; iter <= maxIterations; iter++)
        {
            if (Complex.Abs(g) <= tolerance)
            {
                return new LbfgsComplexResult(x, fx, iter, true);
            }

            Complex direction = -inverseHessianScale * g;
            double step = 1d;
            double directionalDerivative = (Complex.Conjugate(g) * direction).Real;

            for (int ls = 0; ls < 20; ls++)
            {
                Complex candidateX = x + (step * direction);
                double candidateFx = f(candidateX);

                if (candidateFx <= fx + (1e-4d * step * directionalDerivative))
                {
                    Complex candidateG = gradient(candidateX);

                    Complex s = candidateX - x;
                    Complex y = candidateG - g;
                    double yy = (Complex.Conjugate(y) * y).Real;
                    if (yy > 1e-14)
                    {
                        double sy = (Complex.Conjugate(s) * y).Real;
                        inverseHessianScale = sy / yy;
                    }

                    x = candidateX;
                    fx = candidateFx;
                    g = candidateG;
                    break;
                }

                step *= 0.5d;
            }
        }

        return new LbfgsComplexResult(x, fx, maxIterations, false);
    }
}

