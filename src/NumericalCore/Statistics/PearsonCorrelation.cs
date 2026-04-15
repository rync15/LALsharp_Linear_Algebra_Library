using System.Numerics;

namespace LAL.NumericalCore.Statistics;

internal static class PearsonCorrelation
{
    public static double Compute(ReadOnlySpan<double> x, ReadOnlySpan<double> y, bool allowParallel = false)
    {
        return Covariance.Correlation(x, y, allowParallel);
    }

    public static float Compute(ReadOnlySpan<float> x, ReadOnlySpan<float> y, bool allowParallel = false)
    {
        return Covariance.Correlation(x, y, allowParallel);
    }

    public static Complex Compute(ReadOnlySpan<Complex> x, ReadOnlySpan<Complex> y, bool allowParallel = false)
    {
        return Covariance.Correlation(x, y, allowParallel);
    }
}