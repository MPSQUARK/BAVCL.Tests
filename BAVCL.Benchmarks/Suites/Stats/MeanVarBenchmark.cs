using BAVCL.Modules.Statistics;
using BenchmarkDotNet.Attributes;

namespace BAVCL.Benchmarks;

[MemoryDiagnoser]
public class MeanVarBenchmark
{
	static readonly GPU Gpu = GPUManager.Default;

	static MeanVarBenchmark()
	{
		KernelModuleLoader.Load<float>(Gpu, KernelWorkloads.Statistics);
	}

	Vector _vector = null!;

	[Params(BenchmarkSizes.Small, BenchmarkSizes.Typical, BenchmarkSizes.Large)]
	public int N { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		var rng = new Random(23);
		float[] data = new float[N];
		for (int i = 0; i < N; i++)
			data[i] = (float)rng.NextDouble() * 100f;
		_vector = new Vector(Gpu, data, cache: true);
	}

	[Benchmark(Baseline = true)]
	public float Mean_Cpu() => _vector.Mean();

	[Benchmark]
	public float Mean_Gpu() => _vector.MeanX();

	[Benchmark]
	public float Var_Cpu() => _vector.Var();

	[Benchmark]
	public float Var_Gpu() => _vector.VarX();
}
