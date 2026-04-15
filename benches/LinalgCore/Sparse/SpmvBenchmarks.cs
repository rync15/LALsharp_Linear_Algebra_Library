using BenchmarkDotNet.Attributes;

namespace LAL.Benches;

[MemoryDiagnoser]
public class SpmvBenchmarks
{
    [Benchmark]
    public int Placeholder() => 1;
}

