using BenchmarkDotNet.Attributes;

namespace BAVCL.Benchmarks;

[MemoryDiagnoser]
[MinWarmupCount(1), MaxWarmupCount(2)]
[MinIterationCount(3), MaxIterationCount(5)]
public class MemoryTransferBenchmarks
{
    private GPU _gpu = null!;
    private Vector _vector1K = null!;
    private Vector _vector10K = null!;
    private Vector _vector100K = null!;

    [GlobalSetup]
    public void Setup()
    {
        _gpu = GPUManager.Default;
        _vector1K = new Vector(_gpu, Data(1_000), cache: true);
        _vector10K = new Vector(_gpu, Data(10_000), cache: true);
        _vector100K = new Vector(_gpu, Data(100_000), cache: true);
    }

    private static float[] Data(int n) =>
        Enumerable.Range(0, n).Select(i => (float)i).ToArray();

    [Benchmark] public void SyncCPU_1K() => _vector1K.SyncCPU();
    [Benchmark] public void SyncCPU_10K() => _vector10K.SyncCPU();
    [Benchmark] public void SyncCPU_100K() => _vector100K.SyncCPU();
}
