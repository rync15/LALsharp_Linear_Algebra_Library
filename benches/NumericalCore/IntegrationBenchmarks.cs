using BenchmarkDotNet.Attributes;

namespace LAL.Benches;

[MemoryDiagnoser]
public class IntegrationBenchmarks
{
    [Benchmark]
    public int Placeholder() => 1;
}

