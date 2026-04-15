using BenchmarkDotNet.Attributes;

namespace LAL.Benches;

[MemoryDiagnoser]
public class ConcatStackBenchmarks
{
    [Benchmark]
    public int Placeholder() => 1;
}

