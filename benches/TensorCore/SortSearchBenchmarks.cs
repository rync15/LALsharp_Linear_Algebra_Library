using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using LAL.TensorCore;

namespace LAL.Benches;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, launchCount: 1, warmupCount: 1, iterationCount: 5)]
public class SortSearchBenchmarks
{
    [Params(256, 1024, 4096)]
    public int Length { get; set; }

    private double[] _values = [];
    private double[] _primary = [];
    private double[] _secondary = [];
    private int[] _indices = [];
    private bool[] _mask = [];

    [GlobalSetup]
    public void GlobalSetup()
    {
        _values = new double[Length];
        _primary = new double[Length];
        _secondary = new double[Length];
        _indices = new int[Length];
        _mask = new bool[Length];

        for (int i = 0; i < Length; i++)
        {
            double v = ((i * 17) % 101) - 50;
            _values[i] = v;
            _primary[i] = ((i * 31) % 127) * 0.1;
            _secondary[i] = ((i * 19) % 113) * 0.1;
            _mask[i] = (i & 3) == 0;
        }
    }

    [Benchmark]
    public int ArgsortDouble_FirstIndex()
    {
        SortSearch.Argsort(_values, _indices, ascending: true);
        return _indices[0];
    }

    [Benchmark]
    public int LexsortDouble_FirstIndex()
    {
        SortSearch.Lexsort(_primary, _secondary, _indices);
        return _indices[0];
    }

    [Benchmark]
    public int NonZeroDouble_IntoSpan()
    {
        return SortSearch.NonZero(_values, _indices);
    }

    [Benchmark]
    public int Where_IntoSpan()
    {
        return SortSearch.Where(_mask, _indices);
    }
}

