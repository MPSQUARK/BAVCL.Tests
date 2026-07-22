using BAVCL.Core;
using BenchmarkDotNet.Attributes;

namespace BAVCL.Benchmarks;

/// <summary>
/// CPU/GPU sync and cache upload paths.
/// Pair <c>SyncCPU_Cold_FromGpu_*</c> (GPU authority reset each iteration) with
/// <c>SyncCPU_NoOp_InSync_*</c> (already in sync; fast-path return) for the full picture.
/// </summary>
[MemoryDiagnoser]
[MinWarmupCount(1), MaxWarmupCount(2)]
[MinIterationCount(3), MaxIterationCount(5)]
public class MemoryTransferBenchmarks
{
    private GPU _gpu = null!;
    private Vector _inSync1K = null!;
    private Vector _inSync10K = null!;
    private Vector _inSync100K = null!;
    private Vector _gpuResident1K = null!;
    private Vector _gpuResident10K = null!;
    private Vector _gpuResident100K = null!;

    [GlobalSetup]
    public void Setup()
    {
        _gpu = GPUManager.Default;
        _inSync1K = CreateInSync(1_000);
        _inSync10K = CreateInSync(10_000);
        _inSync100K = CreateInSync(100_000);

        _gpuResident1K = CreateGpuResident(1_000);
        _gpuResident10K = CreateGpuResident(10_000);
        _gpuResident100K = CreateGpuResident(100_000);
    }

    static Vector CreateInSync(int length) =>
        new(GPUManager.Default, Data(length), cache: true);

    static Vector CreateGpuResident(int length)
    {
        var vector = new Vector(GPUManager.Default, Data(length), cache: true);
        vector.Residence = Residence.Gpu;
        return vector;
    }

    private static float[] Data(int n) =>
        Enumerable.Range(0, n).Select(i => (float)i).ToArray();

    static void MarkGpuAuthority(Vector vector) => vector.Residence = Residence.Gpu;

    // --- 1K: cold pull vs in-sync no-op ---

    [Benchmark]
    [InvocationCount(1)]
    public void SyncCPU_Cold_FromGpu_1K() => _gpuResident1K.SyncCPU();

    [Benchmark]
    public void SyncCPU_NoOp_InSync_1K() => _inSync1K.SyncCPU();

    [IterationSetup(Target = nameof(SyncCPU_Cold_FromGpu_1K))]
    public void ResetGpuResident1K() => MarkGpuAuthority(_gpuResident1K);

    // --- 10K ---

    [Benchmark]
    [InvocationCount(1)]
    public void SyncCPU_Cold_FromGpu_10K() => _gpuResident10K.SyncCPU();

    [Benchmark]
    public void SyncCPU_NoOp_InSync_10K() => _inSync10K.SyncCPU();

    [IterationSetup(Target = nameof(SyncCPU_Cold_FromGpu_10K))]
    public void ResetGpuResident10K() => MarkGpuAuthority(_gpuResident10K);

    // --- 100K ---

    [Benchmark]
    [InvocationCount(1)]
    public void SyncCPU_Cold_FromGpu_100K() => _gpuResident100K.SyncCPU();

    [Benchmark]
    public void SyncCPU_NoOp_InSync_100K() => _inSync100K.SyncCPU();

    [IterationSetup(Target = nameof(SyncCPU_Cold_FromGpu_100K))]
    public void ResetGpuResident100K() => MarkGpuAuthority(_gpuResident100K);

    // --- cache upload ---

    [Benchmark]
    public void UpdateCache_Warm_InSync_10K() => _inSync10K.UpdateCache();

    [Benchmark]
    public void AllocateAndUpload_10K()
    {
        var vector = new Vector(_gpu, Data(10_000), cache: false);
        vector.UpdateCache();
    }
}
