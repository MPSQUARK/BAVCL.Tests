using BAVCL.Modules.Arithmetic;
using BenchmarkDotNet.Attributes;

namespace BAVCL.Benchmarks;

[MemoryDiagnoser]
public class DotBenchmark
{
	static readonly GPU Gpu = GPUManager.Default;

	static DotBenchmark()
	{
		KernelModuleLoader.Load<float>(Gpu, KernelWorkloads.Statistics);
	}

	Vector _left = null!;
	Vector _right = null!;

	[Params(BenchmarkSizes.Small, BenchmarkSizes.Typical, BenchmarkSizes.Large)]
	public int N { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		var rng = new Random(37);
		float[] left = new float[N];
		float[] right = new float[N];
		for (int i = 0; i < N; i++)
		{
			left[i] = (float)rng.NextDouble();
			right[i] = (float)rng.NextDouble();
		}

		_left = new Vector(Gpu, left, cache: true);
		_right = new Vector(Gpu, right, cache: true);
	}

	[Benchmark(Baseline = true)]
	public float Dot_Cpu() => _left.Dot(_right);

	[Benchmark]
	public float Dot_Gpu() => _left.DotX(_right);
}
