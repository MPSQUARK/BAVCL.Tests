using System.Globalization;
using System.Text;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using Perfolizer.Horology;

namespace BAVCL.Benchmarks;

/// <summary>
/// Appends each benchmark class summary into a single datetime-stamped CSV report.
/// </summary>
internal sealed class DatedCsvExporter(
	string filePath,
	string runId,
	string timestamp,
	string suite,
	string category,
	string machine) : IExporter
{
	readonly object _lock = new();
	bool _headerWritten;

	static readonly string[] MetadataColumns =
	[
		"RunId",
		"Timestamp",
		"Category",
		"Suite",
		"Machine",
	];

	static readonly IExporter InnerExporter = new CsvExporter(
		CsvSeparator.Comma,
		new SummaryStyle(
			cultureInfo: CultureInfo.InvariantCulture,
			printUnitsInHeader: true,
			printUnitsInContent: false,
			timeUnit: TimeUnit.Nanosecond,
			sizeUnit: null));

	public string Name => nameof(DatedCsvExporter);

	public void ExportToLog(Summary summary, ILogger logger) =>
		ExportToFiles(summary, logger);

	public IEnumerable<string> ExportToFiles(Summary summary, ILogger logger)
	{
		var capture = new StringBuilder();
		var stringLogger = new StringLogger(capture);
		InnerExporter.ExportToLog(summary, stringLogger);

		string[] lines = capture.ToString()
			.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		if (lines.Length == 0)
			return [filePath];

		string[] headers = ParseCsvLine(lines[0]);
		int jobIndex = IndexOf(headers, "Job");
		int methodIndex = IndexOf(headers, "Method");
		int meanIndex = IndexOf(headers, "Mean");

		lock (_lock)
		{
			using var writer = new StreamWriter(filePath, append: _headerWritten, Encoding.UTF8);
			if (!_headerWritten)
			{
				writer.WriteLine(string.Join(",", MetadataColumns.Concat(headers).Select(CsvEscape)));
				_headerWritten = true;
			}

			for (int i = 1; i < lines.Length; i++)
			{
				string[] row = ParseCsvLine(lines[i]);
				if (ShouldSkipRow(row, jobIndex, methodIndex, meanIndex))
					continue;

				string[] metadata =
				[
					runId,
					timestamp,
					category,
					suite,
					machine,
				];

				writer.WriteLine(string.Join(",", metadata.Concat(row).Select(CsvEscape)));
			}
		}

		return [filePath];
	}

	static bool ShouldSkipRow(string[] row, int jobIndex, int methodIndex, int meanIndex)
	{
		if (jobIndex >= 0 && jobIndex < row.Length)
		{
			string job = row[jobIndex].Trim();
			if (job.Equals("ShortRun", StringComparison.OrdinalIgnoreCase))
				return true;
		}

		if (meanIndex >= 0 && meanIndex < row.Length)
		{
			string mean = row[meanIndex].Trim();
			if (mean.Equals("NA", StringComparison.OrdinalIgnoreCase) || mean == "?")
				return true;
		}

		if (methodIndex >= 0 && methodIndex < row.Length && string.IsNullOrWhiteSpace(row[methodIndex]))
			return true;

		return false;
	}

	static int IndexOf(string[] headers, string name)
	{
		for (int i = 0; i < headers.Length; i++)
		{
			string header = headers[i];
			if (header.Equals(name, StringComparison.OrdinalIgnoreCase)
				|| header.StartsWith(name + " ", StringComparison.OrdinalIgnoreCase))
				return i;
		}

		return -1;
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

	static string CsvEscape(string value)
	{
		if (string.IsNullOrEmpty(value))
			return "";

		bool needsQuotes = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
		if (!needsQuotes)
			return value;

		return "\"" + value.Replace("\"", "\"\"") + "\"";
	}

	sealed class StringLogger(StringBuilder buffer) : ILogger
	{
		public string Id => nameof(StringLogger);
		public int Priority => 0;

		public void Write(LogKind logKind, string text) => buffer.Append(text);

		public void WriteLine() => buffer.AppendLine();

		public void WriteLine(string text) => buffer.AppendLine(text);

		public void WriteLine(LogKind logKind, string text) => buffer.AppendLine(text);

		public void Flush() { }
	}
}
