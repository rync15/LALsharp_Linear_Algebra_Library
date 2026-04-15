using BenchmarkDotNet.Attributes;
using LAL.NumericalCore.Optimization;

namespace LAL.Benches;

[MemoryDiagnoser]
public class OptimizationBenchmarks
{
    private static readonly Func<double, double> Objective = static x => ((x - 3.0) * (x - 3.0)) + 0.5;
    private static readonly Func<double, double> Gradient = static x => 2.0 * (x - 3.0);

    [Benchmark]
    public GradientDescentResult SolveScalar()
    {
        return GradientDescent.SolveScalar(
            Objective,
            Gradient,
            initialX: 20.0,
            learningRate: 0.1,
            tolerance: 1e-10,
            maxIterations: 5_000);
    }
}

