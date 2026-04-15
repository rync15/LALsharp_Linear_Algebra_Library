using BenchmarkDotNet.Attributes;

namespace LAL.Benches;

[MemoryDiagnoser]
public class CholeskyBenchmarks
{
    [Benchmark]
    public int Placeholder() => 1;
}

