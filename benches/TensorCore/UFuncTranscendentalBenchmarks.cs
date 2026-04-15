using BenchmarkDotNet.Attributes;

namespace LAL.Benches;

[MemoryDiagnoser]
public class UFuncTranscendentalBenchmarks
{
    [Benchmark]
    public int Placeholder() => 1;
}

