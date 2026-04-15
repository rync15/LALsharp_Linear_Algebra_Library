using BenchmarkDotNet.Attributes;
using LAL.LinalgCore;

namespace LAL.Benches;

[MemoryDiagnoser]
public class MatrixAnalysisBenchmarks
{
    [Params(128, 512)]
    public int Size { get; set; }

    private double[] _input = Array.Empty<double>();
    private double[] _output = Array.Empty<double>();

    [GlobalSetup]
    public void GlobalSetup()
    {
        var rng = new Random(47);
        int elements = Size * Size;
        _input = new double[elements];
        _output = new double[elements];
        for (int i = 0; i < elements; i++)
        {
            _input[i] = rng.NextDouble();
        }
    }

    [Benchmark]
    public void TransposeMatrix()
    {
        Transpose.Matrix(_input, Size, Size, _output);
    }
}

