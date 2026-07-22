using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;

namespace BAVCL.Benchmarks;

internal static class Program
{
    static void Main(string[] args)
    {
        IConfig config = ManualConfig.CreateEmpty()
            .AddColumnProvider(DefaultColumnProviders.Instance)
            .AddColumn(StatisticColumn.Min, StatisticColumn.Max)
            .AddLogger(ConsoleLogger.Default)
            .AddExporter(MarkdownExporter.GitHub)
            .WithArtifactsPath(BenchmarkConfig.GetArtifactsPath())
            .WithOptions(ConfigOptions.DisableLogFile);

        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
    }
}
