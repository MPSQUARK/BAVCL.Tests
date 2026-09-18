using System.Text;
using System.Text.RegularExpressions;

namespace BAVCL.Benchmarks;

public static class BenchmarkConfig
{

	public static string GetProjectRoot() =>
		Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));

	public static string GetBenchmarksPath() =>
		Path.Combine(GetProjectRoot(), "Benchmarks");

	public static string GetProbesPath() =>
		Path.Combine(GetProjectRoot(), "Probes");

	[Obsolete("Use GetBenchmarksPath.")]
	public static string GetRunsPath() => GetBenchmarksPath();

	public static string GetReportsPath() =>
		Path.Combine(GetProjectRoot(), "Reports");

	public static string GetLegacyResultsPath() =>
		Path.Combine(GetProjectRoot(), "Results");

	[Obsolete("Use GetBenchmarksPath or GetProbesPath.")]
	public static string GetDataRunsPath() => GetBenchmarksPath();

	[Obsolete("Use GetProbesPath.")]
	public static string GetDataProbesPath() => GetProbesPath();

	public static string GetCategoryDirectory(string scope, bool probe = false)
	{
		string category = BenchmarkCategories.GetCategory(scope);
		return Path.Combine(probe ? GetProbesPath() : GetBenchmarksPath(), category);
	}

	public static (string Path, string RunId, string Timestamp, string Suite, string Category) CreateRunReportPath(string scope)
	{
		string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
		string suite = SanitizeScope(scope);
		bool probe = BenchmarkCategories.IsProbe(scope);
		string category = BenchmarkCategories.GetCategory(scope);
		string directory = GetCategoryDirectory(scope, probe);
		Directory.CreateDirectory(directory);
		string filePath = Path.Combine(directory, $"{timestamp}.csv");
		string runId = $"{category}/{suite}/{timestamp}";
		return (filePath, runId, timestamp, suite, category);
	}

	public static string CreateReportPath(string name)
	{
		string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
		string directory = GetReportsPath();
		Directory.CreateDirectory(directory);
		return Path.Combine(directory, $"{timestamp}-{SanitizeScope(name)}.md");
	}

	public static int RelocateExistingRuns()
	{
		string projectRoot = GetProjectRoot();
		string benchmarksPath = GetBenchmarksPath();
		var sources = new List<string>
		{
			Path.Combine(projectRoot, "data", "runs"),
			Path.Combine(projectRoot, "runs"),
			benchmarksPath,
			projectRoot,
		};

		int moved = 0;
		foreach (string sourceRoot in sources.Distinct(StringComparer.OrdinalIgnoreCase))
		{
			if (!Directory.Exists(sourceRoot))
				continue;

			foreach (string file in Directory.EnumerateFiles(sourceRoot, "*.csv", SearchOption.AllDirectories))
			{
				if (TryRelocateCsv(file, benchmarksPath))
					moved++;
			}
		}

		CleanupEmptyDirectories(benchmarksPath);
		if (Directory.Exists(Path.Combine(projectRoot, "runs")))
			CleanupEmptyDirectories(Path.Combine(projectRoot, "runs"));

		string legacyDataRuns = Path.Combine(projectRoot, "data", "runs");
		if (Directory.Exists(legacyDataRuns))
			CleanupEmptyDirectories(legacyDataRuns);

		string legacyData = Path.Combine(projectRoot, "data");
		if (Directory.Exists(legacyData) && !Directory.EnumerateFileSystemEntries(legacyData).Any())
			Directory.Delete(legacyData, recursive: true);

		Console.WriteLine($"Relocated {moved} CSV file(s) into Benchmarks/<category>/{{timestamp}}.csv");
		return moved;
	}

	static bool TryRelocateCsv(string filePath, string benchmarksPath)
	{
		string? folderFromPath = null;
		string relative = Path.GetRelativePath(benchmarksPath, filePath);
		string[] parts = relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

		if (parts.Length >= 2 && IsValidFolderName(parts[0]))
			folderFromPath = parts[0];

		string stem = Path.GetFileNameWithoutExtension(filePath);

		if (!TryParseTimestampAndSuite(stem, folderFromPath, filePath, out string timestamp, out string suite, out string category))
			return false;

		string destDir = Path.Combine(benchmarksPath, category);
		string destPath = Path.Combine(destDir, $"{timestamp}.csv");

		if (Path.GetFullPath(filePath).Equals(Path.GetFullPath(destPath), StringComparison.OrdinalIgnoreCase))
			return false;

		Directory.CreateDirectory(destDir);
		if (File.Exists(destPath))
			destPath = Path.Combine(destDir, $"{timestamp}-{Guid.NewGuid():N}.csv");

		File.Move(filePath, destPath);
		return true;
	}

	internal static bool TryParseTimestampAndSuite(
		string stem,
		string? folderFromPath,
		string filePath,
		out string timestamp,
		out string suite,
		out string category)
	{
		timestamp = "";
		suite = "";
		category = "";

		var match = Regex.Match(stem, @"^(\d{4}-\d{2}-\d{2}_\d{6})(?:[-_](.+))?$");
		if (!match.Success)
			return false;

		timestamp = match.Groups[1].Value;
		string suffix = match.Groups[2].Success ? match.Groups[2].Value : "";

		if (folderFromPath is not null)
			category = BenchmarkCategories.NormalizeFolder(folderFromPath);
		else if (folderFromPath is not null)
			suite = folderFromPath;

		if (string.IsNullOrWhiteSpace(suite) && !string.IsNullOrWhiteSpace(suffix) && !IsGenericTag(suffix))
			suite = suffix;

		if (string.IsNullOrWhiteSpace(suite))
			suite = TryReadSuiteFromCsv(filePath) ?? "Unknown";

		suite = SanitizeScope(suite);

		if (string.IsNullOrWhiteSpace(category))
			category = BenchmarkCategories.GetCategory(suite);
		else
			category = BenchmarkCategories.NormalizeFolder(category);

		return !string.IsNullOrWhiteSpace(timestamp);
	}

	static bool IsValidFolderName(string name) =>
		!string.IsNullOrWhiteSpace(name) && name is not "." and not "..";

	static bool IsGenericTag(string suffix) =>
		suffix.Equals("baseline", StringComparison.OrdinalIgnoreCase)
		|| suffix.Contains("gate-report", StringComparison.OrdinalIgnoreCase)
		|| suffix.Contains("decision-report", StringComparison.OrdinalIgnoreCase)
		|| suffix.Contains("july-2026", StringComparison.OrdinalIgnoreCase);

	static void CleanupEmptyDirectories(string root)
	{
		if (!Directory.Exists(root))
			return;

		foreach (string dir in Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories).OrderByDescending(d => d.Length))
		{
			if (!Directory.EnumerateFileSystemEntries(dir).Any())
				Directory.Delete(dir);
		}
	}

	static string? TryReadSuiteFromCsv(string filePath)
	{
		try
		{
			using var reader = new StreamReader(filePath, Encoding.UTF8);
			string? header = reader.ReadLine();
			if (header is null)
				return null;

			string[] headers = ParseCsvLine(header);
			int suiteIndex = Array.FindIndex(headers, h => h.Trim().Equals("Suite", StringComparison.OrdinalIgnoreCase));
			if (suiteIndex < 0)
				return null;

			string? row = reader.ReadLine();
			if (row is null)
				return null;

			string[] cells = ParseCsvLine(row);
			return suiteIndex < cells.Length ? cells[suiteIndex].Trim() : null;
		}
		catch
		{
			return null;
		}
	}

	static string[] ParseCsvLine(string line)
	{
		var fields = new List<string>();
		var current = new StringBuilder();
		bool inQuotes = false;

		for (int i = 0; i < line.Length; i++)
		{
			char c = line[i];
			if (c == '"')
			{
				if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
				{
					current.Append('"');
					i++;
				}
				else
					inQuotes = !inQuotes;
			}
			else if (c == ',' && !inQuotes)
			{
				fields.Add(current.ToString());
				current.Clear();
			}
			else
				current.Append(c);
		}

		fields.Add(current.ToString());
		return fields.ToArray();
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
			if (segment.EndsWith("Benchmarks", StringComparison.Ordinal) || segment.EndsWith("Benchmark", StringComparison.Ordinal))
				return segment;
		}

		if (IsSortBenchmarksMethodFilter(trimmed))
			return "SortBenchmarks";

		if (trimmed.Contains("GraphComparison", StringComparison.Ordinal))
			return "GraphComparisonBenchmarks";

		int dot = trimmed.IndexOf('.');
		if (dot > 0)
			return trimmed[..dot].Split('*')[0];

		int star = trimmed.IndexOf('*');
		if (star > 0)
			return trimmed[..star];

		return trimmed;
	}

	static bool IsSortBenchmarksMethodFilter(string trimmed) =>
		trimmed.Contains("IntSort_", StringComparison.Ordinal)
		|| trimmed.Contains("FloatSort_", StringComparison.Ordinal)
		|| trimmed.Contains("IntArgsort_", StringComparison.Ordinal)
		|| trimmed.Contains("FloatArgsort_", StringComparison.Ordinal);

	static string SanitizeScope(string scope) =>
		Regex.Replace(scope, @"[^\w.-]", "_");
}
