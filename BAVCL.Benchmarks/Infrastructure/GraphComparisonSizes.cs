namespace BAVCL.Benchmarks;

/// <summary>
/// Size resolution for graph comparison benchmarks.
/// </summary>
public static class GraphComparisonSizes
{
	/// <summary>1D workloads use the benchmark <paramref name="paramN"/> directly.</summary>
	public static int VectorLength(int paramN) => paramN;

	/// <summary>
	/// Matrix workloads use <paramref name="paramN"/> for small/typical, but cap the large
	/// sweep (1M) to a feasible matrix order — a 1M×1M dense matrix cannot be allocated.
	/// </summary>
	public static int MatrixOrder(int paramN) =>
		paramN >= BenchmarkSizes.Large ? 1024 : paramN;
}
