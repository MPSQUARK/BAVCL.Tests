using ILGPU.Runtime;

namespace BAVCL.Benchmarks;

/// <summary>Single-case probe: GPU in-place int sort at N=1M (scratch-pool path).</summary>
[MemoryDiagnoser]
public class SortXip1MProbeBenchmarks
{
	static readonly GPU Gpu = GPUManager.Default;

	const int N = BenchmarkSizes.Large;

	VectorInt _source = null!;
	VectorInt _work = null!;

	[GlobalSetup]
	public void Setup()
	{
		KernelModuleLoader.Load<int>(Gpu, KernelWorkloads.Sorting);

		var rng = new Random(42);
		int[] data = new int[N];
		for (int i = 0; i < N; i++)
			data[i] = rng.Next();

		_source = new VectorInt(Gpu, data, cache: true);
		_work = new VectorInt(Gpu, data, cache: true);
	}

	[IterationSetup]
	public void ResetWorkCopy()
	{
		using (GpuScope.Begin(_work, _source))
			_source.GetBuffer().View.CopyTo(Gpu.accelerator.DefaultStream, _work.GetBuffer().View);

		Gpu.accelerator.Synchronize();
	}

	[Benchmark]
	public void IntSort_Gpu_Asc_XIP_1D() => _work.SortAscXIP();
}
