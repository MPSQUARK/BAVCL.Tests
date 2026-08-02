using System.Text.RegularExpressions;

namespace BAVCL.Benchmarks;

public static class BenchmarkConfig
{
	public static string GetArtifactsPath() =>
		Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Results"));

	public static string CreateRunReportPath(string scope)
	{
		string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
		string safeScope = SanitizeScope(scope);
		string suiteDir = Path.Combine(GetArtifactsPath(), safeScope);
		Directory.CreateDirectory(suiteDir);
		return Path.Combine(suiteDir, $"{timestamp}-baseline.md");
	}

	public static string DeriveScopeFromArgs(string[] args)
	{
		for (int i = 0; i < args.Length; i++)
		{
			if (args[i] is "--filter" or "-f" && i + 1 < args.Length)
				return ExtractScopeFromFilter(args[i + 1]);
		}

		return "All";
	}

	static string ExtractScopeFromFilter(string filter)
	{
		string trimmed = filter.Trim('*', '?', ' ');

		if (string.IsNullOrWhiteSpace(trimmed))
			return "Filtered";

		foreach (string segment in trimmed.Split('*'))
		{
			if (segment.EndsWith("Benchmarks", StringComparison.Ordinal))
				return segment;
		}

		// SortBenchmarks.IntSort_Gpu_Asc_1D -> SortBenchmarks
		int dot = trimmed.IndexOf('.');
		if (dot > 0)
			return trimmed[..dot].Split('*')[0];

		// *IntSort_Gpu_Asc_1D* -> IntSort_Gpu_Asc_1D
		int star = trimmed.IndexOf('*');
		if (star > 0)
			return trimmed[..star];

		return trimmed;
	}

	static string SanitizeScope(string scope) =>
		Regex.Replace(scope, @"[^\w.-]", "_");
}
