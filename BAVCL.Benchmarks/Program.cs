using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;

namespace BAVCL.Benchmarks;

internal static class Program
{
	static void Main(string[] args)
	{
		if (args.Contains("--migrate-results"))
		{
			MigrateMarkdownToCsv.Run();
			BenchmarkConfig.RelocateExistingRuns();
			return;
		}

		if (args.Contains("--relocate-runs"))
		{
			BenchmarkConfig.RelocateExistingRuns();
			return;
		}

		if (args.Contains("--sum-accuracy-report"))
		{
			SumStrategyAccuracyReport.Print();
			SumStrategyAccuracyReport.Print(10_000);
			return;
		}

		if (args.Contains("--mask-compaction-ab-report"))
		{
			MaskCompactionAbGateReport.WriteReport();
			return;
		}

		string scope = BenchmarkConfig.DeriveScopeFromArgs(args);
		var (reportPath, runId, timestamp, suite, category) = BenchmarkConfig.CreateRunReportPath(scope);
		string machine = Environment.MachineName;
		bool probe = BenchmarkCategories.IsProbe(scope);

		Job job = Job.InProcess
			.WithMinWarmupCount(1)
			.WithMaxWarmupCount(2)
			.WithMinIterationCount(2)
			.WithMaxIterationCount(3);

		IConfig config = ManualConfig.CreateEmpty()
			.AddColumnProvider(DefaultColumnProviders.Instance)
			.AddColumn(StatisticColumn.Min, StatisticColumn.Max)
			.AddLogger(ConsoleLogger.Default)
			.AddExporter(new DatedCsvExporter(reportPath, runId, timestamp, suite, category, machine))
			.AddJob(job)
			.WithArtifactsPath(BenchmarkConfig.GetCategoryDirectory(scope, probe))
			.WithOptions(ConfigOptions.DisableLogFile);

		Console.WriteLine($"Scope:    {scope}");
		Console.WriteLine($"Category: {category}");
		Console.WriteLine($"Report:   {reportPath}");

		BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);

		int pruned = ProbeCleanup.Run();
		if (pruned > 0)
			Console.WriteLine($"Pruned {pruned} probe CSV(s) older than {ProbeCleanup.Retention.TotalDays:0} days.");
	}
}
