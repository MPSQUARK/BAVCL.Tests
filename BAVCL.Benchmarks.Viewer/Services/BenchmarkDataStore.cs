namespace BAVCL.Benchmarks.Viewer.Services;

public sealed class BenchmarkDataStore
{
	readonly List<BenchmarkRow> _rows = [];
	readonly List<string> _folderCategories = [];

	public IReadOnlyList<BenchmarkRow> Rows => _rows;

	public void SetFolderCategories(IEnumerable<string> categories)
	{
		_folderCategories.Clear();
		foreach (string category in categories)
		{
			if (!string.IsNullOrWhiteSpace(category))
				_folderCategories.Add(category);
		}
	}

	public void LoadFiles(IEnumerable<(string Name, string Text)> files)
	{
		_rows.Clear();
		foreach ((string name, string text) in files)
		{
			string folder = ExtractFolderCategory(name);
			if (!string.IsNullOrWhiteSpace(folder) && !_folderCategories.Contains(folder, StringComparer.OrdinalIgnoreCase))
				_folderCategories.Add(folder);

			_rows.AddRange(BenchmarkCsvParser.Parse(name, text, folder));
		}

		_folderCategories.Sort(StringComparer.OrdinalIgnoreCase);
	}

	public IReadOnlyList<string> Categories() => _folderCategories;

	public IReadOnlyList<string> Methods(string? category) =>
		Filter(category, null, null).Select(r => r.Method).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(m => m).ToList();

	public IReadOnlyList<int> DistinctN(string? category, IEnumerable<string>? methods, IEnumerable<string>? runIds)
	{
		return Filter(category, methods, runIds)
			.Select(r => r.N)
			.Where(n => n is > 0)
			.Select(n => n!.Value)
			.Distinct()
			.OrderBy(n => n)
			.ToList();
	}

	public IEnumerable<BenchmarkRow> Filter(
		string? category,
		IEnumerable<string>? methods,
		IEnumerable<string>? runIds,
		int? n = null)
	{
		HashSet<string>? methodFilter = methods?.Where(m => !string.IsNullOrWhiteSpace(m)).ToHashSet(StringComparer.OrdinalIgnoreCase);
		if (methodFilter is { Count: 0 })
			methodFilter = null;

		HashSet<string>? runFilter = runIds?.ToHashSet(StringComparer.OrdinalIgnoreCase);

		foreach (BenchmarkRow row in _rows)
		{
			if (category is not null && !RowCategory(row).Equals(category, StringComparison.OrdinalIgnoreCase))
				continue;
			if (methodFilter is not null && !methodFilter.Contains(row.Method))
				continue;
			if (runFilter is not null && !runFilter.Contains(row.RunId))
				continue;
			if (n is not null && row.N != n)
				continue;
			if (row.N is null)
				continue;

			yield return row;
		}
	}

	public static string RowCategory(BenchmarkRow row) =>
		!string.IsNullOrWhiteSpace(row.FolderCategory) ? row.FolderCategory : row.Category;

	public static string ExtractFolderCategory(string relativePath)
	{
		string normalized = relativePath.Replace('\\', '/');
		int slash = normalized.IndexOf('/');
		return slash > 0 ? normalized[..slash] : "";
	}

	public static string FormatTimestamp(string timestamp)
	{
		if (timestamp.Length >= 15 && timestamp[10] == '_')
		{
			string date = timestamp[..10];
			string time = timestamp[11..];
			if (time.Length >= 6)
				return $"{date} {time[..2]}:{time[2..4]}:{time[4..6]}";
		}

		return timestamp;
	}

	public static string FormatNanoseconds(double ns)
	{
		if (ns >= 1_000_000_000)
			return $"{ns / 1_000_000_000:0.##} s";
		if (ns >= 1_000_000)
			return $"{ns / 1_000_000:0.##} ms";
		if (ns >= 1_000)
			return $"{ns / 1_000:0.##} μs";
		return $"{ns:0.##} ns";
	}

	public static string FormatN(int n) => n >= 1_000_000 ? $"{n / 1_000_000.0:0.#}M" : n >= 1_000 ? $"{n / 1_000.0:0.#}k" : n.ToString("N0");

	public static bool IsGpuMethod(string method) =>
		method.Contains("_Gpu_", StringComparison.OrdinalIgnoreCase)
		|| method.Contains("Gpu_", StringComparison.OrdinalIgnoreCase);
}
