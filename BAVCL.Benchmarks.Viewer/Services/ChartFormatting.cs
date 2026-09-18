namespace BAVCL.Benchmarks.Viewer.Services;

public static class ChartFormatting
{
	public static string FormatNanoseconds(double ns) => BenchmarkDataStore.FormatNanoseconds(ns);

	public static string FormatNanosecondsAxis(double ns) => FormatNanoseconds(ns);

	public static (double[] Values, string Unit) ToDisplayScale(double[] nanoseconds)
	{
		if (nanoseconds.Length == 0)
			return (nanoseconds, "ns");

		double max = nanoseconds.Max(Math.Abs);
		if (max >= 1_000_000_000)
			return (nanoseconds.Select(v => v / 1_000_000_000).ToArray(), "s");
		if (max >= 1_000_000)
			return (nanoseconds.Select(v => v / 1_000_000).ToArray(), "ms");
		if (max >= 1_000)
			return (nanoseconds.Select(v => v / 1_000).ToArray(), "μs");

		return (nanoseconds, "ns");
	}
}
