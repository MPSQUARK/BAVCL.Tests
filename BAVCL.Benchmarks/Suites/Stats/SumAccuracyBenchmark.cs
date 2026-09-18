using BAVCL.Modules.Arithmetic;
using BenchmarkDotNet.Attributes;

namespace BAVCL.Benchmarks;

[MemoryDiagnoser]
public class SumAccuracyBenchmark
{
	const int N = 10_000_000;
	const float Value = 0.1f;
	const float Expected = N * Value;

	static readonly GPU Gpu = GPUManager.Default;

	Vector _vector = null!;

	[GlobalSetup]
	public void Setup()
	{
		KernelModuleLoader.Load<float>(Gpu, KernelWorkloads.Statistics);
		_vector = new Vector(Gpu, Enumerable.Repeat(Value, N).ToArray(), cache: true);
	}

	[Benchmark(Baseline = true)]
	public float CpuErrorCorrectedSum() => _vector.Sum();

	[Benchmark]
	public float GpuSumX() => _vector.SumX();
}
