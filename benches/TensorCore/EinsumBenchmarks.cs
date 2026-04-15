using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using LAL.TensorCore;

namespace LAL.Benches;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, launchCount: 1, warmupCount: 1, iterationCount: 5)]
public class EinsumDotBenchmarks
{
    [Params(256, 1024, 4096)]
    public int Length { get; set; }

    private double[] _left = [];
    private double[] _right = [];

    [GlobalSetup]
    public void GlobalSetup()
    {
        _left = new double[Length];
        _right = new double[Length];

        for (int i = 0; i < Length; i++)
        {
            _left[i] = ((i * 13) % 17) - 8;
            _right[i] = ((i * 7) % 23) - 11;
        }
    }

    [Benchmark]
    public double DotEvaluateDouble()
    {
        return Einsum.Evaluate("i,i->", _left, _right);
    }
}

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, launchCount: 1, warmupCount: 1, iterationCount: 5)]
public class EinsumMatMulBenchmarks
{
    [Params(32, 64)]
    public int Size { get; set; }

    private double[,] _left = new double[1, 1];
    private double[,] _right = new double[1, 1];

    [GlobalSetup]
    public void GlobalSetup()
    {
        _left = new double[Size, Size];
        _right = new double[Size, Size];

        for (int row = 0; row < Size; row++)
        {
            for (int col = 0; col < Size; col++)
            {
                _left[row, col] = (((row + 1) * (col + 3)) % 31) * 0.05;
                _right[row, col] = (((row + 2) * (col + 5)) % 29) * 0.04;
            }
        }
    }

    [Benchmark]
    public double MatMulEvaluateDouble_CenterValue()
    {
        double[,] result = Einsum.EvaluateMatMul("ij,jk->ik", _left, _right);
        return result[Size / 2, Size / 2];
    }
}

