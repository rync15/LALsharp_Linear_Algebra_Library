using BenchmarkDotNet.Attributes;

namespace LAL.Benches;

[MemoryDiagnoser]
public class LuBenchmarks
{
    [Benchmark]
    public int Placeholder() => 1;
}

