using BenchmarkDotNet.Attributes;

namespace LAL.Benches;

[MemoryDiagnoser]
public class SchurBenchmarks
{
    [Benchmark]
    public int Placeholder() => 1;
}

