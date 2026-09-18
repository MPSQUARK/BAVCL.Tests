namespace BAVCL.Benchmarks.Viewer.Services;

public sealed class BenchmarkCsvLoader(IWebHostEnvironment environment, BenchmarkDataStore store)
{
	public string BenchmarksPath => ResolveBenchmarksPath();

	public BenchmarkLoadResult Reload()
	{
		string path = ResolveBenchmarksPath();
		if (!Directory.Exists(path))
			return new BenchmarkLoadResult(path, 0, 0, $"Directory not found: {path}");

		var folderCategories = Directory.EnumerateDirectories(path)
			.Select(d => Path.GetFileName(d))
			.Where(n => !string.IsNullOrWhiteSpace(n))
			.OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
			.ToList();
		store.SetFolderCategories(folderCategories);

		var files = Directory.EnumerateFiles(path, "*.csv", SearchOption.AllDirectories)
			.OrderBy(f => f)
			.Select(f => (Name: Path.GetRelativePath(path, f), Text: File.ReadAllText(f)))
			.ToList();

		store.LoadFiles(files);
		return new BenchmarkLoadResult(path, files.Count, store.Rows.Count, null);
	}

	string ResolveBenchmarksPath()
	{
		string fromContentRoot = Path.GetFullPath(
			Path.Combine(environment.ContentRootPath, "..", "BAVCL.Benchmarks", "Benchmarks"));

		if (Directory.Exists(fromContentRoot))
			return fromContentRoot;

		string fromBin = Path.GetFullPath(
			Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "BAVCL.Benchmarks", "Benchmarks"));

		return Directory.Exists(fromBin) ? fromBin : fromContentRoot;
	}
}

public sealed record BenchmarkLoadResult(string Path, int FileCount, int RowCount, string? Error);
