using BenchmarkDotNet.Attributes;

namespace LAL.Benches;

[MemoryDiagnoser]
public class BdfBenchmarks
{
    [Benchmark]
    public int Placeholder() => 1;
}

