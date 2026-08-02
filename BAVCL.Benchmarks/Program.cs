using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;

namespace BAVCL.Benchmarks;

internal static class Program
{
	static void Main(string[] args)
	{
		string scope = BenchmarkConfig.DeriveScopeFromArgs(args);
		string reportPath = BenchmarkConfig.CreateRunReportPath(scope);
		Directory.CreateDirectory(BenchmarkConfig.GetArtifactsPath());

		Job job = Job.InProcess
			.WithMinWarmupCount(1)
			.WithMaxWarmupCount(2)
			.WithMinIterationCount(2)
			.WithMaxIterationCount(3);

		IConfig config = ManualConfig.CreateEmpty()
			.AddColumnProvider(DefaultColumnProviders.Instance)
			.AddColumn(StatisticColumn.Min, StatisticColumn.Max)
			.AddLogger(ConsoleLogger.Default)
			.AddExporter(new DatedCombinedMarkdownExporter(reportPath, scope))
			.AddJob(job)
			.WithArtifactsPath(BenchmarkConfig.GetArtifactsPath())
			.WithOptions(ConfigOptions.DisableLogFile);

		Console.WriteLine($"Scope:  {scope}");
		Console.WriteLine($"Report: {reportPath}");
		BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
	}
}
