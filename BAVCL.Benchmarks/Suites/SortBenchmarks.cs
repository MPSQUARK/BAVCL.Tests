namespace BAVCL.Benchmarks;

/// <summary>
/// User-facing sort/argsort API at small / typical / large sizes (32 methods × 3 N = 96 cases).
/// Sort and argsort are both allocating here (<c>SortAsc</c>/<c>SortAscX</c>, <c>ArgsortAsc</c>/<c>ArgsortAscX</c>),
/// so no manual clone-before-benchmark step is needed — the source vectors are never mutated.
/// </summary>
[MemoryDiagnoser]
public class SortBenchmarks
{
	static readonly GPU Gpu = GPUManager.Default;

	VectorInt _intVector = null!;
	Vector _floatVector = null!;
	VectorInt _intMatrix = null!;
	Vector _floatMatrix = null!;
	static VectorInt _intSink = null!;
	static Vector _floatSink = null!;

	[Params(BenchmarkSizes.Small, BenchmarkSizes.Typical, BenchmarkSizes.Large)]
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

	[Benchmark] public void IntSort_Cpu_Asc_1D() => Consume(_intVector.SortAsc());
	[Benchmark] public void IntSort_Cpu_Desc_1D() => Consume(_intVector.SortDesc());
	[Benchmark] public void IntSort_Gpu_Asc_1D() => Consume(_intVector.SortAscX());
	[Benchmark] public void IntSort_Gpu_Desc_1D() => Consume(_intVector.SortDescX());

	[Benchmark] public void FloatSort_Cpu_Asc_1D() => Consume(_floatVector.SortAsc());
	[Benchmark] public void FloatSort_Cpu_Desc_1D() => Consume(_floatVector.SortDesc());
	[Benchmark] public void FloatSort_Gpu_Asc_1D() => Consume(_floatVector.SortAscX());
	[Benchmark] public void FloatSort_Gpu_Desc_1D() => Consume(_floatVector.SortDescX());

	[Benchmark] public void IntSort_Cpu_Asc_2D() => Consume(_intMatrix.SortAsc());
	[Benchmark] public void IntSort_Cpu_Desc_2D() => Consume(_intMatrix.SortDesc());
	[Benchmark] public void IntSort_Gpu_Asc_2D() => Consume(_intMatrix.SortAscX());
	[Benchmark] public void IntSort_Gpu_Desc_2D() => Consume(_intMatrix.SortDescX());

	[Benchmark] public void FloatSort_Cpu_Asc_2D() => Consume(_floatMatrix.SortAsc());
	[Benchmark] public void FloatSort_Cpu_Desc_2D() => Consume(_floatMatrix.SortDesc());
	[Benchmark] public void FloatSort_Gpu_Asc_2D() => Consume(_floatMatrix.SortAscX());
	[Benchmark] public void FloatSort_Gpu_Desc_2D() => Consume(_floatMatrix.SortDescX());

	[Benchmark] public void IntArgsort_Cpu_Asc_1D() => Consume(_intVector.ArgsortAsc());
	[Benchmark] public void IntArgsort_Cpu_Desc_1D() => Consume(_intVector.ArgsortDesc());
	[Benchmark] public void IntArgsort_Gpu_Asc_1D() => Consume(_intVector.ArgsortAscX());
	[Benchmark] public void IntArgsort_Gpu_Desc_1D() => Consume(_intVector.ArgsortDescX());

	[Benchmark] public void IntArgsort_Cpu_Asc_2D() => Consume(_intMatrix.ArgsortAsc());
	[Benchmark] public void IntArgsort_Cpu_Desc_2D() => Consume(_intMatrix.ArgsortDesc());
	[Benchmark] public void IntArgsort_Gpu_Asc_2D() => Consume(_intMatrix.ArgsortAscX());
	[Benchmark] public void IntArgsort_Gpu_Desc_2D() => Consume(_intMatrix.ArgsortDescX());

	[Benchmark] public void FloatArgsort_Cpu_Asc_1D() => Consume(_floatVector.ArgsortAsc());
	[Benchmark] public void FloatArgsort_Cpu_Desc_1D() => Consume(_floatVector.ArgsortDesc());
	[Benchmark] public void FloatArgsort_Gpu_Asc_1D() => Consume(_floatVector.ArgsortAscX());
	[Benchmark] public void FloatArgsort_Gpu_Desc_1D() => Consume(_floatVector.ArgsortDescX());

	[Benchmark] public void FloatArgsort_Cpu_Asc_2D() => Consume(_floatMatrix.ArgsortAsc());
	[Benchmark] public void FloatArgsort_Cpu_Desc_2D() => Consume(_floatMatrix.ArgsortDesc());
	[Benchmark] public void FloatArgsort_Gpu_Asc_2D() => Consume(_floatMatrix.ArgsortAscX());
	[Benchmark] public void FloatArgsort_Gpu_Desc_2D() => Consume(_floatMatrix.ArgsortDescX());

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
