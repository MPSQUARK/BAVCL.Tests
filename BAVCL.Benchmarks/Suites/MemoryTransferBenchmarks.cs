namespace BAVCL.Benchmarks;

/// <summary>
/// CPU/GPU sync and cache upload paths.
/// </summary>
public class MemoryTransferBenchmarks
{
	GPU _gpu = null!;
	Vector _inSync = null!;
	Vector _gpuResident = null!;

	[Params(BenchmarkSizes.Small, BenchmarkSizes.Typical, BenchmarkSizes.Large)]
	public int N { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		_gpu = GPUManager.Default;
		_inSync = new Vector(_gpu, Data(N), cache: true);
		_gpuResident = CreateGpuResident(N);
	}

	static Vector CreateGpuResident(int length)
	{
		var vector = new Vector(GPUManager.Default, Data(length), cache: true);
		vector.Residence = Residence.Gpu;
		return vector;
	}

	static float[] Data(int n) =>
		Enumerable.Range(0, n).Select(i => (float)i).ToArray();

	static void MarkGpuAuthority(Vector vector) => vector.Residence = Residence.Gpu;

	[Benchmark]
	[InvocationCount(1)]
	public void SyncCPU_Cold_FromGpu() => _gpuResident.SyncCPU();

	[Benchmark]
	public void SyncCPU_NoOp_InSync() => _inSync.SyncCPU();

	[IterationSetup(Target = nameof(SyncCPU_Cold_FromGpu))]
	public void ResetGpuResident() => MarkGpuAuthority(_gpuResident);

	[Benchmark]
	public void UpdateCache_Warm_InSync() => _inSync.UpdateCache();

	[Benchmark]
	public void AllocateAndUpload()
	{
		var vector = new Vector(_gpu, Data(N), cache: false);
		vector.UpdateCache();
	}
}
