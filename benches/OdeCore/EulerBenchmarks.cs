using BenchmarkDotNet.Attributes;

namespace LAL.Benches;

[MemoryDiagnoser]
public class EulerBenchmarks
{
    [Benchmark]
    public int Placeholder() => 1;
}

