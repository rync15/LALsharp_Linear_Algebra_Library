using BenchmarkDotNet.Attributes;

namespace LAL.Benches;

[MemoryDiagnoser]
public class BroadcastBenchmarks
{
    [Benchmark]
    public int Placeholder() => 1;
}

