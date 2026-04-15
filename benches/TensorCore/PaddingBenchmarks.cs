using BenchmarkDotNet.Attributes;

namespace LAL.Benches;

[MemoryDiagnoser]
public class PaddingBenchmarks
{
    [Benchmark]
    public int Placeholder() => 1;
}

