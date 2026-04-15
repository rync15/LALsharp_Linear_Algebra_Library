using BenchmarkDotNet.Attributes;
using LAL.NumericalCore.RootFinding;

namespace LAL.Benches;

[MemoryDiagnoser]
public class RootFindingBenchmarks
{
    private static readonly Func<double, double> TargetFunction = static x => Math.Cos(x) - x;

    [Benchmark]
    public BrentResult BrentSolve()
    {
        return Brent.Solve(TargetFunction, 0.0, 1.0, tolerance: 1e-12, maxIterations: 100);
    }
}

