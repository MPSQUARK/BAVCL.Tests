using System.Globalization;

namespace BAVCL.Benchmarks.Viewer.Services;

public sealed record BenchmarkRow(
	string RunId,
	string Timestamp,
	DateTime? RunAt,
	string Category,
	string FolderCategory,
	string Suite,
	string Machine,
	string Method,
	string Job,
	int? N,
	double MeanNs,
	double? ErrorNs,
	double? StdDevNs,
	double? MinNs,
	double? MaxNs,
	string? Allocated,
	string SourceFile)
{
	public static DateTime? ParseRunAt(string timestamp)
	{
		if (string.IsNullOrWhiteSpace(timestamp))
			return null;

		if (timestamp.Length >= 15 && timestamp[10] == '_')
		{
			string date = timestamp[..10];
			string time = timestamp[11..];
			if (time.Length >= 6
			    && DateTime.TryParseExact(
				    $"{date} {time[..2]}:{time[2..4]}:{time[4..6]}",
				    "yyyy-MM-dd HH:mm:ss",
				    CultureInfo.InvariantCulture,
				    DateTimeStyles.AssumeLocal,
				    out DateTime runAt))
				return runAt;
		}

		return DateTime.TryParse(timestamp, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime parsed)
			? parsed
			: null;
	}
}
