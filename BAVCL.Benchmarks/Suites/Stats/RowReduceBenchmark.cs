using BAVCL.Core;
using BAVCL.GpuAlgorithms;
using BenchmarkDotNet.Attributes;

namespace BAVCL.Benchmarks;

[MemoryDiagnoser]
public class RowReduceBenchmark
{
	static readonly GPU Gpu = GPUManager.Default;

	Vector _coeff = null!;
	Vector _matrix = null!;
	VectorInt _coeffInt = null!;
	VectorInt _matrixInt = null!;

	[Params(64, 1024, 4096)]
	public int Rows { get; set; }

	[Params(64, 1024, 10000)]
	public int Cols { get; set; }

	[Params(Operations.multiply, Operations.add)]
	public Operations Op { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		KernelModuleLoader.Load<float>(Gpu, KernelWorkloads.Default);
		KernelModuleLoader.Load<int>(Gpu, KernelWorkloads.Default);

		var rng = new Random(17);
		float[] coeff = new float[Cols];
		float[] matrix = new float[Rows * Cols];
		int[] coeffInt = new int[Cols];
		int[] matrixInt = new int[Rows * Cols];
		for (int i = 0; i < Cols; i++)
		{
			coeff[i] = (float)rng.NextDouble();
			coeffInt[i] = rng.Next(1, 10);
		}

		for (int i = 0; i < matrix.Length; i++)
		{
			matrix[i] = (float)rng.NextDouble();
			matrixInt[i] = rng.Next(1, 10);
		}

		_coeff = new Vector(Gpu, coeff, cache: true);
		_matrix = new Vector(Gpu, matrix, Cols, cache: true);
		_coeffInt = new VectorInt(Gpu, coeffInt, cache: true);
		_matrixInt = new VectorInt(Gpu, matrixInt, Cols, cache: true);
	}

	[Benchmark(Baseline = true)]
	public Vector Float_Reduce() => RowReduceAlgorithms.Reduce(_coeff, _matrix, Op);

	[Benchmark]
	public VectorInt Int_Reduce() => RowReduceAlgorithms.Reduce(_coeffInt, _matrixInt, Op);
}
