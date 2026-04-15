using BenchmarkDotNet.Attributes;
using LAL.LinalgCore;

namespace LAL.Benches;

[MemoryDiagnoser]
public class DotBenchmarks
{
    [Params(4_096, 65_536)]
    public int N { get; set; }

    private double[] _left = Array.Empty<double>();
    private double[] _right = Array.Empty<double>();

    [GlobalSetup]
    public void GlobalSetup()
    {
        var rng = new Random(43);
        _left = new double[N];
        _right = new double[N];

        for (int i = 0; i < N; i++)
        {
            _left[i] = rng.NextDouble();
            _right[i] = rng.NextDouble();
        }
    }

    [Benchmark]
    public double DotuDouble()
    {
        return Dot.Dotu(_left, _right);
    }
}

