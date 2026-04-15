using BenchmarkDotNet.Attributes;
using LAL.LinalgCore;

namespace LAL.Benches;

[MemoryDiagnoser]
public class NormsBenchmarks
{
    [Params(4_096, 65_536)]
    public int N { get; set; }

    private double[] _values = Array.Empty<double>();

    [GlobalSetup]
    public void GlobalSetup()
    {
        var rng = new Random(46);
        _values = new double[N];
        for (int i = 0; i < N; i++)
        {
            _values[i] = rng.NextDouble() - 0.5;
        }
    }

    [Benchmark]
    public double L2Norm()
    {
        return Norms.L2(_values);
    }
}

