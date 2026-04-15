using BenchmarkDotNet.Attributes;

namespace LAL.Benches;

[MemoryDiagnoser]
public class SvdBenchmarks
{
    [Benchmark]
    public int Placeholder() => 1;
}

