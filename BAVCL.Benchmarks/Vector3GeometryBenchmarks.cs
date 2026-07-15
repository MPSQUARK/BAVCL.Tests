using BenchmarkDotNet.Attributes;

namespace BAVCL.Benchmarks;

[MemoryDiagnoser]
[MinWarmupCount(1), MaxWarmupCount(2)]
[MinIterationCount(3), MaxIterationCount(5)]
public class Vector3GeometryBenchmarks
{
    private GPU _gpu = null!;
    private Vector3 _a = null!;
    private Vector3 _b = null!;

    [GlobalSetup]
    public void Setup()
    {
        _gpu = GPUManager.Default;
        var values = Enumerable.Range(0, 3000).Select(i => (float)i).ToArray();
        _a = new Vector3(_gpu, values, cache: true);
        _b = new Vector3(_gpu, values.Select(v => v + 1f).ToArray(), cache: true);
    }

    [Benchmark] public Vector3 Cross_1K() => Vector3.Cross(_a, _b);
    [Benchmark] public Vector Magnitude_1K() => Vector3.Magnitude(_a);
    [Benchmark] public Vector Distance_1K() => Vector3.Distance(_a, _b);
}
