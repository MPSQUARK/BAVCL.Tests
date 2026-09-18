using BAVCL.Modules.Arithmetic;
using BAVCL.Modules.Statistics;
using BenchmarkDotNet.Attributes;

namespace BAVCL.Benchmarks;

[MemoryDiagnoser]
public class ReduceBenchmark
{
	static readonly GPU Gpu = GPUManager.Default;

	static ReduceBenchmark()
	{
		KernelModuleLoader.Load<float>(Gpu, KernelWorkloads.Statistics);
		KernelModuleLoader.Load<int>(Gpu, KernelWorkloads.Statistics);
	}

	Vector _vector = null!;
	VectorInt _intVector = null!;

	[Params(BenchmarkSizes.Small, BenchmarkSizes.Typical, BenchmarkSizes.Large)]
	public int N { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		var rng = new Random(11);
		float[] floatData = new float[N];
		int[] intData = new int[N];
		for (int i = 0; i < N; i++)
		{
			floatData[i] = (float)rng.NextDouble() * 1000f;
			intData[i] = rng.Next();
		}

		_vector = new Vector(Gpu, floatData, cache: true);
		_intVector = new VectorInt(Gpu, intData, cache: true);
	}

	[Benchmark(Baseline = true)]
	public float FloatSum_Cpu() => _vector.Sum();

	[Benchmark]
	public float FloatSum_Gpu() => _vector.SumX();

	[Benchmark]
	public float FloatMin_Cpu() => _vector.Min();

	[Benchmark]
	public float FloatMin_Gpu() => _vector.MinX();

	[Benchmark]
	public float FloatMax_Cpu() => _vector.Max();

	[Benchmark]
	public float FloatMax_Gpu() => _vector.MaxX();

	[Benchmark]
	public float IntSum_Cpu() => _intVector.Sum();

	[Benchmark]
	public float IntSum_Gpu() => _intVector.SumX();
}
