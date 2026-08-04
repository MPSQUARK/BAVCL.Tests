namespace BAVCL.Benchmarks;

/// <summary>
/// User-facing sort/argsort API at small / typical / large sizes (28 methods × 3 N = 84 cases).
/// </summary>
[MemoryDiagnoser]
public class SortBenchmarks
{
	static readonly GPU Gpu = GPUManager.Default;

	VectorInt _intVector = null!;
	Vector _floatVector = null!;
	VectorInt _intMatrix = null!;
	Vector _floatMatrix = null!;
	static VectorInt _argsortSink = null!;

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
		_argsortSink = new VectorInt(Gpu, 1);
	}

	[Benchmark] public void IntSort_Cpu_Asc_1D() => CloneAndSortIntCpuAsc(_intVector);
	[Benchmark] public void IntSort_Cpu_Desc_1D() => CloneAndSortIntCpuDesc(_intVector);
	[Benchmark] public void IntSort_Gpu_Asc_1D() => CloneAndSortIntGpuAsc(_intVector);
	[Benchmark] public void IntSort_Gpu_Desc_1D() => CloneAndSortIntGpuDesc(_intVector);

	[Benchmark] public void FloatSort_Cpu_Asc_1D() => CloneAndSortFloatCpuAsc(_floatVector);
	[Benchmark] public void FloatSort_Cpu_Desc_1D() => CloneAndSortFloatCpuDesc(_floatVector);
	[Benchmark] public void FloatSort_Gpu_Asc_1D() => CloneAndSortFloatGpuAsc(_floatVector);
	[Benchmark] public void FloatSort_Gpu_Desc_1D() => CloneAndSortFloatGpuDesc(_floatVector);

	[Benchmark] public void IntSort_Cpu_Asc_2D() => CloneAndSortIntCpuAsc(_intMatrix);
	[Benchmark] public void IntSort_Cpu_Desc_2D() => CloneAndSortIntCpuDesc(_intMatrix);
	[Benchmark] public void IntSort_Gpu_Asc_2D() => CloneAndSortIntGpuAsc(_intMatrix);
	[Benchmark] public void IntSort_Gpu_Desc_2D() => CloneAndSortIntGpuDesc(_intMatrix);

	[Benchmark] public void FloatSort_Cpu_Asc_2D() => CloneAndSortFloatCpuAsc(_floatMatrix);
	[Benchmark] public void FloatSort_Cpu_Desc_2D() => CloneAndSortFloatCpuDesc(_floatMatrix);
	[Benchmark] public void FloatSort_Gpu_Asc_2D() => CloneAndSortFloatGpuAsc(_floatMatrix);
	[Benchmark] public void FloatSort_Gpu_Desc_2D() => CloneAndSortFloatGpuDesc(_floatMatrix);

	[Benchmark] public void IntArgsort_Gpu_Asc_1D() => CloneAndArgsortIntGpuAsc(_intVector);
	[Benchmark] public void IntArgsort_Gpu_Desc_1D() => CloneAndArgsortIntGpuDesc(_intVector);
	[Benchmark] public void IntArgsort_Gpu_Asc_2D() => CloneAndArgsortIntGpuAsc(_intMatrix);
	[Benchmark] public void IntArgsort_Gpu_Desc_2D() => CloneAndArgsortIntGpuDesc(_intMatrix);

	[Benchmark] public void FloatArgsort_Gpu_Asc_1D() => CloneAndArgsortFloatGpuAsc(_floatVector);
	[Benchmark] public void FloatArgsort_Gpu_Desc_1D() => CloneAndArgsortFloatGpuDesc(_floatVector);
	[Benchmark] public void FloatArgsort_Gpu_Asc_2D() => CloneAndArgsortFloatGpuAsc(_floatMatrix);
	[Benchmark] public void FloatArgsort_Gpu_Desc_2D() => CloneAndArgsortFloatGpuDesc(_floatMatrix);

	static void CloneAndSortIntCpuAsc(VectorInt source)
	{
		var copy = new VectorInt(Gpu, source.ToArray(), source.Columns, cache: true);
		copy.SortAscending();
	}

	static void CloneAndSortIntCpuDesc(VectorInt source)
	{
		var copy = new VectorInt(Gpu, source.ToArray(), source.Columns, cache: true);
		copy.SortDescending();
	}

	static void CloneAndSortIntGpuAsc(VectorInt source)
	{
		var copy = new VectorInt(Gpu, source.ToArray(), source.Columns, cache: true);
		copy.SortAscendingX();
	}

	static void CloneAndSortIntGpuDesc(VectorInt source)
	{
		var copy = new VectorInt(Gpu, source.ToArray(), source.Columns, cache: true);
		copy.SortDescendingX();
	}

	static void CloneAndSortFloatCpuAsc(Vector source)
	{
		var copy = new Vector(Gpu, source.ToArray(), source.Columns, cache: true);
		copy.SortAscending();
	}

	static void CloneAndSortFloatCpuDesc(Vector source)
	{
		var copy = new Vector(Gpu, source.ToArray(), source.Columns, cache: true);
		copy.SortDescending();
	}

	static void CloneAndSortFloatGpuAsc(Vector source)
	{
		var copy = new Vector(Gpu, source.ToArray(), source.Columns, cache: true);
		copy.SortAscendingX();
	}

	static void CloneAndSortFloatGpuDesc(Vector source)
	{
		var copy = new Vector(Gpu, source.ToArray(), source.Columns, cache: true);
		copy.SortDescendingX();
	}

	static void CloneAndArgsortIntGpuAsc(VectorInt source)
	{
		var copy = new VectorInt(Gpu, source.ToArray(), source.Columns, cache: true);
		Consume(copy.ArgsortAscendingX());
	}

	static void CloneAndArgsortIntGpuDesc(VectorInt source)
	{
		var copy = new VectorInt(Gpu, source.ToArray(), source.Columns, cache: true);
		Consume(copy.ArgsortDescendingX());
	}

	static void CloneAndArgsortFloatGpuAsc(Vector source)
	{
		var copy = new Vector(Gpu, source.ToArray(), source.Columns, cache: true);
		Consume(copy.ArgsortAscendingX());
	}

	static void CloneAndArgsortFloatGpuDesc(Vector source)
	{
		var copy = new Vector(Gpu, source.ToArray(), source.Columns, cache: true);
		Consume(copy.ArgsortDescendingX());
	}

	static void Consume(VectorInt indices)
	{
		if (indices.Length > 0)
			_argsortSink = indices;
	}
}
