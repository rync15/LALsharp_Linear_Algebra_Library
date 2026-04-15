using BenchmarkDotNet.Attributes;
using LAL.LinalgCore;

namespace LAL.Benches;

[MemoryDiagnoser]
public class AxpyBenchmarks
{
    [Params(4_096, 65_536)]
    public int N { get; set; }

    private double[] _x = Array.Empty<double>();
    private double[] _ySeed = Array.Empty<double>();
    private double[] _y = Array.Empty<double>();

    [GlobalSetup]
    public void GlobalSetup()
    {
        var rng = new Random(42);
        _x = new double[N];
        _ySeed = new double[N];
        _y = new double[N];

        for (int i = 0; i < N; i++)
        {
            _x[i] = rng.NextDouble();
            _ySeed[i] = rng.NextDouble();
        }
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _ySeed.AsSpan().CopyTo(_y);
    }

    [Benchmark]
    public void AxpyDouble()
    {
        Axpy.Compute(0.75d, _x, _y);
    }
}

