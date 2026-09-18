using BAVCL.Benchmarks.Infrastructure;
using BenchmarkDotNet.Attributes;

namespace BAVCL.Benchmarks;

/// <summary>
/// GPU variance group-reduce throughput per kernel implementation.
/// Production <c>VarX</c> uses the path selected once at statistics module load (typically shared scratch).
/// </summary>
[MemoryDiagnoser]
public class VarReducePathBenchmark
{
	static readonly GPU Gpu = GPUManager.Default;

	Vector _vector = null!;

	[Params(VarReduceKernelHarness.Path.SharedScratch, VarReduceKernelHarness.Path.WarpTree)]
	public VarReduceKernelHarness.Path Path { get; set; }

	[Params(BenchmarkSizes.Typical, BenchmarkSizes.Large)]
	public int N { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		var rng = new Random(41);
		float[] data = new float[N];
		for (int i = 0; i < N; i++)
			data[i] = 100_000f + (float)rng.NextDouble() * 5f;

		_vector = new Vector(Gpu, data, cache: true);
	}

	[Benchmark]
	public float VarReduceKernel() => VarReduceKernelHarness.VarFloat(_vector, Path);
}
