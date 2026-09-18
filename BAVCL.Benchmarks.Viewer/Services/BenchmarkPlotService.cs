namespace BAVCL.Benchmarks.Viewer.Services;

public sealed class BenchmarkPlotService(IWebHostEnvironment environment, BenchmarkDataStore store)
{
	public string PlotsDirectory
	{
		get
		{
			string dir = Path.Combine(environment.WebRootPath, "plots");
			Directory.CreateDirectory(dir);
			return dir;
		}
	}

	public (string ScalingUrl, string HistoryUrl) GeneratePlots(
		string category,
		IEnumerable<string> methods,
		IEnumerable<string> runIds)
	{
		string[] methodList = methods.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(m => m).ToArray();
		string key = BenchmarkChartBuilder.PlotKey(category, methodList, runIds);
		string scalingName = $"scaling_{key}.png";
		string historyName = $"history_{key}.png";
		string scalingPath = Path.Combine(PlotsDirectory, scalingName);
		string historyPath = Path.Combine(PlotsDirectory, historyName);

		BenchmarkChartBuilder.SaveScalingPlot(store, category, methodList, runIds, scalingPath);
		BenchmarkChartBuilder.SaveHistoryPlot(store, category, methodList, runIds, historyPath);

		return ($"/plots/{scalingName}", $"/plots/{historyName}");
	}
}
