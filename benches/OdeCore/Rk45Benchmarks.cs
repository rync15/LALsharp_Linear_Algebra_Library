using BenchmarkDotNet.Attributes;
using LAL.OdeCore;

namespace LAL.Benches;

[MemoryDiagnoser]
public class Rk45Benchmarks
{
    [Params(8, 32)]
    public int Dimension { get; set; }

    private double[] _state = Array.Empty<double>();
    private double[] _next = Array.Empty<double>();
    private double[] _jacobian = Array.Empty<double>();

    private static readonly OdeSystem SystemModel = static (t, y, dydt) =>
    {
        for (int i = 0; i < y.Length; i++)
        {
            dydt[i] = -0.15 * y[i] + Math.Sin(t + (0.01 * i));
        }
    };

    [GlobalSetup]
    public void GlobalSetup()
    {
        var rng = new Random(48);
        _state = new double[Dimension];
        _next = new double[Dimension];
        _jacobian = new double[Dimension * Dimension];

        for (int i = 0; i < _state.Length; i++)
        {
            _state[i] = rng.NextDouble();
        }
    }

    [Benchmark]
    public Rk45StepResult Rk45Step()
    {
        return Rk45.Step(0.0, 1e-3, _state, _next, SystemModel);
    }

    [Benchmark]
    public void EstimateJacobianForwardDifference()
    {
        JacobianEstimator.EstimateForwardDifference(0.0, _state, _jacobian, SystemModel);
    }
}

