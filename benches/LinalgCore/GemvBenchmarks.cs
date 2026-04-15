using BenchmarkDotNet.Attributes;
using LAL.LinalgCore;

namespace LAL.Benches;

[MemoryDiagnoser]
public class GemvBenchmarks
{
    [Params(256, 1024)]
    public int Rows { get; set; }

    [Params(256)]
    public int Cols { get; set; }

    private double[] _matrix = Array.Empty<double>();
    private double[] _x = Array.Empty<double>();
    private double[] _ySeed = Array.Empty<double>();
    private double[] _y = Array.Empty<double>();

    [GlobalSetup]
    public void GlobalSetup()
    {
        var rng = new Random(44);
        _matrix = new double[Rows * Cols];
        _x = new double[Cols];
        _ySeed = new double[Rows];
        _y = new double[Rows];

        for (int i = 0; i < _matrix.Length; i++)
        {
            _matrix[i] = rng.NextDouble();
        }

        for (int i = 0; i < _x.Length; i++)
        {
            _x[i] = rng.NextDouble();
        }

        for (int i = 0; i < _ySeed.Length; i++)
        {
            _ySeed[i] = rng.NextDouble();
        }
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _ySeed.AsSpan().CopyTo(_y);
    }

    [Benchmark]
    public void GemvMultiply()
    {
        Gemv.Multiply(_matrix, Rows, Cols, _x, _y, alpha: 1.0, beta: 0.5);
    }
}

