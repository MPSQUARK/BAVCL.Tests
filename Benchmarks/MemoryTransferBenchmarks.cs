namespace BAVCL.Tests.Benchmarks;

/// <summary>
/// Benchmarks for memory transfer operations between CPU and GPU
/// Critical for understanding performance bottlenecks
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class MemoryTransferBenchmarks
{
    private GPU _gpu = null!;
    private Vector _vector1K = null!;
    private Vector _vector10K = null!;
    private Vector _vector100K = null!;
    private Vector _vector1M = null!;

    [GlobalSetup]
    public void Setup()
    {
        _gpu = GPUManager.Default;
        _gpu.Should().NotBeNull();

        _vector1K = new Vector(_gpu, Enumerable.Range(0, 1_000).Select(i => (float)i).ToArray(), cache: true);
        _vector10K = new Vector(_gpu, Enumerable.Range(0, 10_000).Select(i => (float)i).ToArray(), cache: true);
        _vector100K = new Vector(_gpu, Enumerable.Range(0, 100_000).Select(i => (float)i).ToArray(), cache: true);
        _vector1M = new Vector(_gpu, Enumerable.Range(0, 1_000_000).Select(i => (float)i).ToArray(), cache: true);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        // GPU and Vector resources will be cleaned up automatically
    }

    [Benchmark]
    public void SyncCPU_1K() => _vector1K.SyncCPU();

    [Benchmark]
    public void SyncCPU_10K() => _vector10K.SyncCPU();

    [Benchmark]
    public void SyncCPU_100K() => _vector100K.SyncCPU();

    [Benchmark]
    public void SyncCPU_1M() => _vector1M.SyncCPU();

}
