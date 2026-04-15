using BenchmarkDotNet.Attributes;
using LAL.LinalgCore;

namespace LAL.Benches;

[MemoryDiagnoser]
public class GemmBenchmarks
{
    [Params(64, 128)]
    public int Size { get; set; }

    private double[] _a = Array.Empty<double>();
    private double[] _b = Array.Empty<double>();
    private double[] _cSeed = Array.Empty<double>();
    private double[] _c = Array.Empty<double>();

    [GlobalSetup]
    public void GlobalSetup()
    {
        var rng = new Random(45);
        int elementCount = Size * Size;

        _a = new double[elementCount];
        _b = new double[elementCount];
        _cSeed = new double[elementCount];
        _c = new double[elementCount];

        for (int i = 0; i < elementCount; i++)
        {
            _a[i] = rng.NextDouble();
            _b[i] = rng.NextDouble();
            _cSeed[i] = rng.NextDouble();
        }
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _cSeed.AsSpan().CopyTo(_c);
    }

    [Benchmark]
    public void GemmMultiply()
    {
        Gemm.Multiply(_a, _b, _c, Size, Size, Size, alpha: 1.0, beta: 1.0);
    }
}

