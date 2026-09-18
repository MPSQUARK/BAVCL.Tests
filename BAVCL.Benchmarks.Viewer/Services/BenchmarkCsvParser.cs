using System.Globalization;

namespace BAVCL.Benchmarks.Viewer.Services;

public static class BenchmarkCsvParser
{
	public static IReadOnlyList<BenchmarkRow> Parse(string fileName, string csvText, string folderCategory = "")
	{
		var rows = new List<BenchmarkRow>();
		string[] lines = csvText.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		if (lines.Length < 2)
			return rows;

		string[] headers = ParseCsvLine(lines[0]);
		var index = BuildIndex(headers);

		for (int i = 1; i < lines.Length; i++)
		{
			string[] cells = ParseCsvLine(lines[i]);
			if (cells.Length == 0)
				continue;

			string method = Get(cells, index, "Method");
			if (string.IsNullOrWhiteSpace(method))
				continue;

			string job = Get(cells, index, "Job");
			if (job.Equals("ShortRun", StringComparison.OrdinalIgnoreCase))
				continue;

			double? meanNs = ParseDouble(Get(cells, index, "Mean"));
			if (meanNs is null)
				continue;

			string csvCategory = Get(cells, index, "Category");
			string timestamp = Get(cells, index, "Timestamp");
			rows.Add(new BenchmarkRow(
				RunId: Get(cells, index, "RunId"),
				Timestamp: timestamp,
				RunAt: BenchmarkRow.ParseRunAt(timestamp),
				Category: csvCategory,
				FolderCategory: !string.IsNullOrWhiteSpace(folderCategory) ? folderCategory : csvCategory,
				Suite: Get(cells, index, "Suite"),
				Machine: Get(cells, index, "Machine"),
				Method: method,
				Job: job,
				N: ParseInt(Get(cells, index, "N")),
				MeanNs: meanNs.Value,
				ErrorNs: ParseDouble(Get(cells, index, "Error")),
				StdDevNs: ParseDouble(Get(cells, index, "StdDev")),
				MinNs: ParseDouble(Get(cells, index, "Min")),
				MaxNs: ParseDouble(Get(cells, index, "Max")),
				Allocated: NullIfEmpty(Get(cells, index, "Allocated")),
				SourceFile: fileName));
		}

		return rows;
	}

	static Dictionary<string, int> BuildIndex(string[] headers)
	{
		var index = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		for (int i = 0; i < headers.Length; i++)
		{
			string header = headers[i].Trim();
			string key = header.Split(' ', '[')[0];
			index.TryAdd(key, i);
		}

		return index;
	}

	static string Get(string[] cells, Dictionary<string, int> index, string name)
	{
		if (!index.TryGetValue(name, out int i) || i >= cells.Length)
			return "";

		return cells[i].Trim();
	}

	static int? ParseInt(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return null;

		return int.TryParse(text.Replace(",", ""), NumberStyles.Integer, CultureInfo.InvariantCulture, out int n)
			? n
			: null;
	}

	static double? ParseDouble(string text)
	{
		if (string.IsNullOrWhiteSpace(text) || text.Equals("NA", StringComparison.OrdinalIgnoreCase))
			return null;

		return double.TryParse(text.Replace(",", ""), NumberStyles.Float, CultureInfo.InvariantCulture, out double n)
			? n
			: null;
	}

	static string? NullIfEmpty(string text) => string.IsNullOrWhiteSpace(text) ? null : text;

	static string[] ParseCsvLine(string line)
	{
		var fields = new List<string>();
		var current = new System.Text.StringBuilder();
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
}
