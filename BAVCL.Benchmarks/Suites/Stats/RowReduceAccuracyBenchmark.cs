using BAVCL.Core;
using BAVCL.GpuAlgorithms;
using BAVCL.Modules.Statistics;
using BenchmarkDotNet.Attributes;

namespace BAVCL.Benchmarks;

[MemoryDiagnoser]
public class RowReduceAccuracyBenchmark
{
	const int Rows = 1024;
	const int Cols = 10_000;

	static readonly GPU Gpu = GPUManager.Default;

	Vector _coeff = null!;
	Vector _matrix = null!;

	[GlobalSetup]
	public void Setup()
	{
		KernelModuleLoader.Load<float>(Gpu, KernelWorkloads.Default);
		float[] coeff = Enumerable.Repeat(0.1f, Cols).ToArray();
		float[] matrix = Enumerable.Repeat(0.1f, Rows * Cols).ToArray();
		_coeff = new Vector(Gpu, coeff, cache: true);
		_matrix = new Vector(Gpu, matrix, Cols, cache: true);
	}

	[Benchmark(Baseline = true)]
	public float[] CpuErrorCorrectedReference() =>
		CpuSimdReduce.RowReduceFloatCompensated(
			_coeff.RetrieveReadOnlySpan(),
			_matrix.RetrieveReadOnlySpan(),
			Rows,
			Cols,
			Operations.multiply);

	[Benchmark]
	public Vector GpuReduce() =>
		RowReduceAlgorithms.Reduce(_coeff, _matrix, Operations.multiply);
}
