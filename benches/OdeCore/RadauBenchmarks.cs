using BenchmarkDotNet.Attributes;

namespace LAL.Benches;

[MemoryDiagnoser]
public class RadauBenchmarks
{
    [Benchmark]
    public int Placeholder() => 1;
}

