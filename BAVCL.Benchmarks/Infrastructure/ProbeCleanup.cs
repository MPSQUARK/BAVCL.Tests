namespace BAVCL.Benchmarks;

public static class ProbeCleanup
{
	public static readonly TimeSpan Retention = TimeSpan.FromDays(14);

	public static int Run(TimeSpan? retention = null)
	{
		string probesDir = BenchmarkConfig.GetProbesPath();
		if (!Directory.Exists(probesDir))
			return 0;

		TimeSpan keep = retention ?? Retention;
		DateTime cutoff = DateTime.UtcNow - keep;
		int deleted = 0;

		foreach (string file in Directory.EnumerateFiles(probesDir, "*.csv", SearchOption.AllDirectories))
		{
			DateTime lastWrite = File.GetLastWriteTimeUtc(file);
			if (lastWrite >= cutoff)
				continue;

			File.Delete(file);
			deleted++;
		}

		return deleted;
	}
}
