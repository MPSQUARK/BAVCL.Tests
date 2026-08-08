namespace BAVCL.Benchmarks;

/// <summary>
/// Slim GPU sort/argsort probe (10 methods × N=10K/1M) for scratch-pool comparisons.
/// </summary>
[MemoryDiagnoser]
public class SortGpuPoolProbeBenchmarks
{
	static readonly GPU Gpu = GPUManager.Default;

	static SortGpuPoolProbeBenchmarks()
	{
		KernelModuleLoader.Load<float>(Gpu, KernelWorkloads.Sorting);
		KernelModuleLoader.Load<int>(Gpu, KernelWorkloads.Sorting);
	}

	VectorInt _intVector = null!;
	Vector _floatVector = null!;
	VectorInt _intMatrix = null!;
	Vector _floatMatrix = null!;
	static VectorInt _intSink = null!;
	static Vector _floatSink = null!;

	[Params(BenchmarkSizes.Typical, BenchmarkSizes.Large)]
	public int N { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		var rng = new Random(42);
		int[] intData = new int[N];
		float[] floatData = new float[N];
		for (int i = 0; i < N; i++)
		{
			intData[i] = rng.Next();
			floatData[i] = (float)rng.NextDouble() * 1000f;
		}

		_intVector = new VectorInt(Gpu, intData, cache: true);
		_floatVector = new Vector(Gpu, floatData, cache: true);

		int rows = Math.Max(4, (int)Math.Sqrt(N));
		int cols = Math.Max(1, N / rows);
		int matrixLength = rows * cols;
		int[] intMatrixData = new int[matrixLength];
		float[] floatMatrixData = new float[matrixLength];
		for (int i = 0; i < matrixLength; i++)
		{
			intMatrixData[i] = rng.Next();
			floatMatrixData[i] = (float)rng.NextDouble() * 1000f;
		}

		_intMatrix = new VectorInt(Gpu, intMatrixData, columns: cols, cache: true);
		_floatMatrix = new Vector(Gpu, floatMatrixData, columns: cols, cache: true);
		_intSink = new VectorInt(Gpu, 1);
		_floatSink = new Vector(Gpu, 1);
	}

	[Benchmark] public void IntSort_Gpu_Asc_1D() => Consume(_intVector.SortAscX());
	[Benchmark] public void IntSort_Gpu_Desc_1D() => Consume(_intVector.SortDescX());
	[Benchmark] public void FloatSort_Gpu_Asc_1D() => Consume(_floatVector.SortAscX());
	[Benchmark] public void FloatSort_Gpu_Desc_1D() => Consume(_floatVector.SortDescX());

	[Benchmark] public void IntSort_Gpu_Asc_2D() => Consume(_intMatrix.SortAscX());
	[Benchmark] public void IntSort_Gpu_Desc_2D() => Consume(_intMatrix.SortDescX());
	[Benchmark] public void FloatSort_Gpu_Asc_2D() => Consume(_floatMatrix.SortAscX());
	[Benchmark] public void FloatSort_Gpu_Desc_2D() => Consume(_floatMatrix.SortDescX());

	[Benchmark] public void IntArgsort_Gpu_Asc_1D() => Consume(_intVector.ArgsortAscX());
	[Benchmark] public void FloatArgsort_Gpu_Asc_1D() => Consume(_floatVector.ArgsortAscX());

	static void Consume(VectorInt result)
	{
		if (result.Length > 0)
			_intSink = result;
	}

	static void Consume(Vector result)
	{
		if (result.Length > 0)
			_floatSink = result;
	}
}
