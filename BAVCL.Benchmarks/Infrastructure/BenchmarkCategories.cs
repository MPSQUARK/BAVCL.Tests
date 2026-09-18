namespace BAVCL.Benchmarks;

public static class BenchmarkCategories
{
	public static readonly string[] All =
	[
		"Sort", "Vector", "Stats", "Memory", "Mask", "Graph", "Probe", "Other",
	];

	public static bool IsProbe(string suite) =>
		suite.Contains("Probe", StringComparison.OrdinalIgnoreCase);

	public static string GetCategory(string suite)
	{
		if (IsProbe(suite))
			return "Probe";

		if (Matches(suite, "Sort", "Argsort", "SortShape"))
			return "Sort";

		if (Matches(suite, "Vector", "SpanRead", "AllX", "DotBenchmark", "Dot"))
			return "Vector";

		if (Matches(suite, "Sum", "Var", "RowReduce", "MeanVar", "Reduce", "StatsRead", "DiffViaCumSum"))
			return "Stats";

		if (Matches(suite, "Cache", "MemoryTransfer"))
			return "Memory";

		if (Matches(suite, "MaskCompaction"))
			return "Mask";

		if (Matches(suite, "GraphComparison"))
			return "Graph";

		return "Other";
	}

	public static string NormalizeFolder(string folder)
	{
		foreach (string category in All)
		{
			if (category.Equals(folder, StringComparison.OrdinalIgnoreCase))
				return category;
		}

		return folder;
	}

	static bool Matches(string suite, params string[] tokens) =>
		tokens.Any(token => suite.Contains(token, StringComparison.OrdinalIgnoreCase));
}
