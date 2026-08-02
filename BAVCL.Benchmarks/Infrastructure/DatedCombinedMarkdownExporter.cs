using System.Text;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;

namespace BAVCL.Benchmarks;

/// <summary>
/// Appends each benchmark class summary into a single datetime-stamped markdown report.
/// </summary>
internal sealed class DatedCombinedMarkdownExporter(string filePath, string scope) : IExporter
{
	readonly object _lock = new();
	bool _headerWritten;

	public string Name => nameof(DatedCombinedMarkdownExporter);

	public void ExportToLog(Summary summary, ILogger logger)
	{
		var capture = new StringBuilder();
		var stringLogger = new StringLogger(capture);
		MarkdownExporter.GitHub.ExportToLog(summary, stringLogger);

		lock (_lock)
		{
			if (!_headerWritten)
			{
				string timestamp = Path.GetFileNameWithoutExtension(filePath);
				string header =
					$"# BAVCL Benchmark Run — {scope}\n\n" +
					$"**Suite:** {scope}  \n" +
					$"**Timestamp:** {timestamp}\n\n";
				File.WriteAllText(filePath, header);
				_headerWritten = true;
			}

			File.AppendAllText(filePath, capture.ToString());
		}
	}

	public IEnumerable<string> ExportToFiles(Summary summary, ILogger logger)
	{
		ExportToLog(summary, logger);
		return [filePath];
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
