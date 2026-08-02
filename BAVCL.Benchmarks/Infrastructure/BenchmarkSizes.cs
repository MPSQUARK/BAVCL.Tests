namespace BAVCL.Benchmarks;

/// <summary>
/// Standard benchmark sizes: small (100), typical (10K), large (1M).
/// </summary>
public static class BenchmarkSizes
{
	public const int Small = 100;
	public const int Typical = 10_000;
	public const int Large = 1_000_000;

	public static readonly int[] All = [Small, Typical, Large];
}
