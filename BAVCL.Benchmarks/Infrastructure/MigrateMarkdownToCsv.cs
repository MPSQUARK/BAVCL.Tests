using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace BAVCL.Benchmarks;

public static class MigrateMarkdownToCsv
{
	static readonly Regex TimeRe = new(@"([\d,]+(?:\.\d+)?)\s*(ns|μs|us|ms|s)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
	static readonly Regex SuiteHeaderRe = new(@"^\*\*Suite:\*\*\s*(.+?)\s*$", RegexOptions.Multiline | RegexOptions.Compiled);
	static readonly Regex DatedReportRe = new(@"^(\d{4}-\d{2}-\d{2}_\d{6})(?:[-_](.+))?$", RegexOptions.Compiled);
	static readonly Regex MethodNSuffixRe = new(@"_(\d+)([KkMm])$", RegexOptions.Compiled);

	public sealed record MigrationSummary(int Converted, int MovedReports, int DeletedProbes, int Skipped);

	public static MigrationSummary Run(bool deleteLegacy = true)
	{
		string legacyDir = BenchmarkConfig.GetLegacyResultsPath();
		string runsDir = BenchmarkConfig.GetBenchmarksPath();
		string reportsDir = BenchmarkConfig.GetReportsPath();
		string plotsDir = Path.Combine(BenchmarkConfig.GetProjectRoot(), "Plots");

		Directory.CreateDirectory(runsDir);
		Directory.CreateDirectory(reportsDir);

		int converted = 0;
		int movedReports = 0;
		int deletedProbes = 0;
		int skipped = 0;

		if (!Directory.Exists(legacyDir))
		{
			Console.WriteLine("No legacy Results/ folder found — nothing to migrate.");
			return new MigrationSummary(0, 0, 0, 0);
		}

		foreach (string file in Directory.EnumerateFiles(legacyDir, "*.md", SearchOption.AllDirectories))
		{
			string name = Path.GetFileName(file);
			if (name.Equals("README.md", StringComparison.OrdinalIgnoreCase))
			{
				skipped++;
				continue;
			}

			string relative = Path.GetRelativePath(legacyDir, file);
			string suiteFromPath = Path.GetDirectoryName(relative)?.Split(Path.DirectorySeparatorChar).LastOrDefault() ?? "Unknown";

			if (IsProbePath(relative, suiteFromPath))
			{
				File.Delete(file);
				deletedProbes++;
				continue;
			}

			if (IsReportFile(name))
			{
				string dest = Path.Combine(reportsDir, name);
				if (File.Exists(dest))
					dest = Path.Combine(reportsDir, $"{Path.GetFileNameWithoutExtension(name)}-{Guid.NewGuid():N}.md");

				File.Move(file, dest);
				movedReports++;
				continue;
			}

			string text = File.ReadAllText(file, Encoding.UTF8);
			string suite = ParseSuite(text, suiteFromPath);
			string stem = Path.GetFileNameWithoutExtension(name);
			var dated = DatedReportRe.Match(stem);
			string timestamp = dated.Success ? dated.Groups[1].Value : stem;
			string normalizedSuite = Regex.Replace(suite, @"[^\w.-]", "_");
			string category = BenchmarkCategories.GetCategory(normalizedSuite);
			string runId = $"{category}/{normalizedSuite}/{timestamp}";
			string machine = Environment.MachineName;

			var rows = ParseMarkdownTable(text);
			if (rows.Count == 0)
			{
				skipped++;
				continue;
			}

			string categoryDir = Path.Combine(runsDir, category);
			Directory.CreateDirectory(categoryDir);
			string csvPath = Path.Combine(categoryDir, $"{timestamp}.csv");
			WriteCsv(csvPath, runId, timestamp, category, normalizedSuite, machine, rows);
			converted++;
		}

		if (deleteLegacy)
		{
			Directory.Delete(legacyDir, recursive: true);
			if (Directory.Exists(plotsDir))
				Directory.Delete(plotsDir, recursive: true);
		}

		Console.WriteLine($"Migration complete: {converted} converted, {movedReports} reports moved, {deletedProbes} probe files deleted, {skipped} skipped.");
		return new MigrationSummary(converted, movedReports, deletedProbes, skipped);
	}

	static bool IsProbePath(string relative, string suiteFromPath) =>
		relative.Contains("Probe", StringComparison.OrdinalIgnoreCase)
		|| suiteFromPath.Contains("Probe", StringComparison.OrdinalIgnoreCase);

	static bool IsReportFile(string name) =>
		name.Contains("gate-report", StringComparison.OrdinalIgnoreCase)
		|| name.Contains("decision-report", StringComparison.OrdinalIgnoreCase)
		|| name.Equals("decision-report.md", StringComparison.OrdinalIgnoreCase);

	static string ParseSuite(string text, string fallback)
	{
		Match match = SuiteHeaderRe.Match(text);
		return match.Success ? match.Groups[1].Value.Trim() : fallback;
	}

	sealed record ParsedRow(string Method, string Job, int? N, double MeanNs, double StdDevNs, double? ErrorNs, double? MinNs, double? MaxNs, string? Allocated);

	static List<ParsedRow> ParseMarkdownTable(string text)
	{
		var rows = new List<ParsedRow>();
		string[] lines = text.Split('\n');
		bool inTable = false;
		var headers = new List<string>();

		foreach (string line in lines)
		{
			if (!line.StartsWith('|'))
			{
				inTable = false;
				continue;
			}

			string[] cells = line.Split('|')[1..^1].Select(c => c.Trim()).ToArray();
			if (cells.Length == 0)
				continue;

			if (cells.All(c => c.All(ch => ch is '-' or ':' or ' ')))
			{
				inTable = true;
				continue;
			}

			if (!inTable && headers.Count == 0)
			{
				headers = cells.Select(c => c.ToLowerInvariant()).ToList();
				inTable = true;
				continue;
			}

			if (headers.Count == 0)
				continue;

			var col = new Dictionary<string, string>();
			for (int i = 0; i < headers.Count; i++)
				col[headers[i]] = i < cells.Length ? cells[i] : "";

			string method = StripDecorators(col.GetValueOrDefault("method", ""));
			if (string.IsNullOrWhiteSpace(method) || method.Equals("method", StringComparison.OrdinalIgnoreCase))
				continue;

			string job = StripDecorators(col.GetValueOrDefault("job", ""));
			if (job.Equals("ShortRun", StringComparison.OrdinalIgnoreCase))
				continue;

			double? meanNs = ParseTimeToNanoseconds(col.GetValueOrDefault("mean", ""));
			if (meanNs is null)
				continue;

			int? n = ParseInt(col.GetValueOrDefault("n", ""));
			(method, n) = CanonicalMethodAndN(method, n);

			rows.Add(new ParsedRow(
				Method: method,
				Job: job,
				N: n,
				MeanNs: meanNs.Value,
				StdDevNs: ParseTimeToNanoseconds(col.GetValueOrDefault("stddev", "")) ?? 0,
				ErrorNs: ParseTimeToNanoseconds(col.GetValueOrDefault("error", "")),
				MinNs: ParseTimeToNanoseconds(col.GetValueOrDefault("min", "")),
				MaxNs: ParseTimeToNanoseconds(col.GetValueOrDefault("max", "")),
				Allocated: col.TryGetValue("allocated", out string? alloc) ? alloc : null));
		}

		return rows;
	}

	static (string Method, int? N) CanonicalMethodAndN(string method, int? nColumn)
	{
		if (nColumn is not null)
			return (method, nColumn);

		Match match = MethodNSuffixRe.Match(method);
		if (!match.Success)
			return (method, null);

		int num = int.Parse(match.Groups[1].Value);
		string suffix = match.Groups[2].Value.ToUpperInvariant();
		int multiplier = suffix switch { "K" => 1_000, "M" => 1_000_000, _ => 1 };
		string baseMethod = method[..match.Index];
		return (baseMethod, num * multiplier);
	}

	static string StripDecorators(string value) =>
		value.Trim().Trim('*').Trim();

	static int? ParseInt(string text)
	{
		string cleaned = StripDecorators(text).Replace(",", "");
		if (string.IsNullOrWhiteSpace(cleaned) || cleaned == "-")
			return null;

		return int.TryParse(cleaned, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n) ? n : null;
	}

	static double? ParseTimeToNanoseconds(string text)
	{
		string cleaned = StripDecorators(text).Replace(",", "");
		Match match = TimeRe.Match(cleaned);
		if (!match.Success)
			return null;

		double value = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
		string unit = match.Groups[2].Value.ToLowerInvariant().Replace("μ", "u");
		double scale = unit switch
		{
			"ns" => 1.0,
			"us" => 1_000.0,
			"ms" => 1_000_000.0,
			"s" => 1_000_000_000.0,
			_ => 1.0,
		};

		return value * scale;
	}

	static void WriteCsv(string path, string runId, string timestamp, string category, string suite, string machine, List<ParsedRow> rows)
	{
		var sb = new StringBuilder();
		sb.AppendLine("RunId,Timestamp,Category,Suite,Machine,Method,Job,N,Mean [ns],Error [ns],StdDev [ns],Min [ns],Max [ns],Allocated");

		foreach (ParsedRow row in rows)
		{
			sb.Append(Csv(runId)).Append(',')
				.Append(Csv(timestamp)).Append(',')
				.Append(Csv(category)).Append(',')
				.Append(Csv(suite)).Append(',')
				.Append(Csv(machine)).Append(',')
				.Append(Csv(row.Method)).Append(',')
				.Append(Csv(row.Job)).Append(',')
				.Append(row.N?.ToString(CultureInfo.InvariantCulture) ?? "").Append(',')
				.Append(row.MeanNs.ToString(CultureInfo.InvariantCulture)).Append(',')
				.Append(row.ErrorNs?.ToString(CultureInfo.InvariantCulture) ?? "").Append(',')
				.Append(row.StdDevNs.ToString(CultureInfo.InvariantCulture)).Append(',')
				.Append(row.MinNs?.ToString(CultureInfo.InvariantCulture) ?? "").Append(',')
				.Append(row.MaxNs?.ToString(CultureInfo.InvariantCulture) ?? "").Append(',')
				.Append(Csv(row.Allocated ?? ""))
				.AppendLine();
		}

		File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
	}

	static string Csv(string value)
	{
		if (string.IsNullOrEmpty(value))
			return "";

		if (!value.Contains(',') && !value.Contains('"') && !value.Contains('\n'))
			return value;

		return "\"" + value.Replace("\"", "\"\"") + "\"";
	}
}
