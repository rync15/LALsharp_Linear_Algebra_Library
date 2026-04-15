using BenchmarkDotNet.Attributes;

namespace LAL.Benches;

[MemoryDiagnoser]
public class FftBenchmarks
{
    [Benchmark]
    public int Placeholder() => 1;
}

