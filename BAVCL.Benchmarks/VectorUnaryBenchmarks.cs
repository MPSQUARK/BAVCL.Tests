using BenchmarkDotNet.Attributes;

namespace BAVCL.Benchmarks;

[MemoryDiagnoser]
[MinWarmupCount(1), MaxWarmupCount(2)]
[MinIterationCount(3), MaxIterationCount(5)]
public class VectorUnaryBenchmarks
{
    private GPU _gpu = null!;
    private Vector _data10K = null!;

    [GlobalSetup]
    public void Setup()
    {
        _gpu = GPUManager.Default;
        var values = Enumerable.Range(0, 10_000).Select(i => (float)(i % 100 - 50)).ToArray();
        _data10K = new Vector(_gpu, values, cache: true);
    }

    [Benchmark] public Vector AbsX_10K() => Vector.AbsX(_data10K);
    [Benchmark] public Vector ReverseX_10K() => Vector.ReverseX(_data10K);
    [Benchmark] public Vector Diff_10K() => Vector.Diff(_data10K);
    [Benchmark] public Vector NanToNum_10K() => Vector.Nan_to_num(_data10K, 0f);
}
