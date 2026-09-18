using System.Security.Cryptography;
using System.Text;
using ScottPlot;

namespace BAVCL.Benchmarks.Viewer.Services;

public static class BenchmarkChartBuilder
{
	const int ScalingWidth = 1200;
	const int ScalingHeight = 460;
	const int HistoryWidth = 1200;
	const int HistoryHeight = 400;

	public static void SaveScalingPlot(
		BenchmarkDataStore store,
		string category,
		IEnumerable<string> methods,
		IEnumerable<string>? runIds,
		string pngPath)
	{
		SavePlot(pngPath, ScalingWidth, ScalingHeight, plot =>
			BuildScalingPlot(plot, store, category, methods, runIds));
	}

	public static void SaveHistoryPlot(
		BenchmarkDataStore store,
		string category,
		IEnumerable<string> methods,
		IEnumerable<string>? runIds,
		string pngPath)
	{
		SavePlot(pngPath, HistoryWidth, HistoryHeight, plot =>
		{
			if (!BuildHistoryPlot(plot, store, category, methods, runIds))
			{
				plot.Clear();
				PlotTheme.Apply(plot);
				plot.Axes.Title.Label.Text = "No run history for selection";
			}
		});
	}

	public static string PlotKey(string category, IEnumerable<string> methods, IEnumerable<string>? runIds)
	{
		string methodList = string.Join(',', methods.OrderBy(m => m, StringComparer.OrdinalIgnoreCase));
		string runs = runIds is null
			? ""
			: string.Join(',', runIds.OrderBy(r => r, StringComparer.OrdinalIgnoreCase));
		string raw = $"{category}|{methodList}|{runs}";
		byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
		return Convert.ToHexString(hash)[..12].ToLowerInvariant();
	}

	static void SavePlot(string pngPath, int width, int height, Action<Plot> build)
	{
		string dir = Path.GetDirectoryName(pngPath)!;
		Directory.CreateDirectory(dir);
		string tempPath = Path.Combine(dir, $".{Path.GetFileName(pngPath)}.{Guid.NewGuid():N}.tmp");

		Plot plot = new();
		try
		{
			build(plot);
			plot.SavePng(tempPath, width, height);
			plot.Dispose();

			if (File.Exists(pngPath))
				File.Delete(pngPath);
			File.Move(tempPath, pngPath);
		}
		finally
		{
			if (File.Exists(tempPath))
			{
				try { File.Delete(tempPath); } catch { /* best effort */ }
			}
		}
	}

	static void BuildScalingPlot(Plot plot, BenchmarkDataStore store, string category, IEnumerable<string> methods, IEnumerable<string>? runIds)
	{
		plot.Clear();
		PlotTheme.Apply(plot);

		string[] methodList = methods.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(m => m).ToArray();
		if (methodList.Length == 0)
		{
			plot.Axes.Title.Label.Text = "Select at least one method";
			return;
		}

		var allMeans = methodList
			.SelectMany(m => store.Filter(category, [m], runIds))
			.Where(r => r.N is > 0)
			.Select(r => r.MeanNs)
			.ToArray();

		if (allMeans.Length == 0)
		{
			plot.Axes.Title.Label.Text = "No data for selected runs";
			return;
		}

		var (scale, unit) = ChartFormatting.ToDisplayScale(allMeans);
		double scaleMax = scale.Max(Math.Abs);
		double scaleFactor = scaleMax > 0 ? allMeans.Max(Math.Abs) / scaleMax : 1;

		int colorIndex = 0;
		double fastest = allMeans.Min();

		foreach (string method in methodList)
		{
			var groups = store.Filter(category, [method], runIds)
				.GroupBy(r => r.RunId)
				.OrderBy(g => g.Key)
				.ToList();

			foreach (var group in groups)
			{
				var points = group.Where(r => r.N is > 0).OrderBy(r => r.N).ToList();
				if (points.Count == 0)
					continue;

				double[] xs = points.Select(p => Math.Log10(p.N!.Value)).ToArray();
				double[] ys = points.Select(p => p.MeanNs / scaleFactor).ToArray();
				bool gpu = BenchmarkDataStore.IsGpuMethod(method);
				var scatter = plot.Add.Scatter(xs, ys);
				scatter.LegendText = SeriesLabel(method, points[0].Timestamp, methodList.Length > 1);
				scatter.LineWidth = gpu ? 2.5f : 2f;
				scatter.MarkerSize = 6;
				scatter.Color = PlotTheme.SeriesPalette[colorIndex % PlotTheme.SeriesPalette.Length];
				if (!gpu)
					scatter.LinePattern = LinePattern.Dashed;

				if (Math.Abs(points.Min(p => p.MeanNs) - fastest) < 1e-6)
					scatter.Color = PlotTheme.Speed;

				colorIndex++;
			}
		}

		plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericAutomatic
		{
			LabelFormatter = y => $"{Math.Pow(10, y):N0}",
		};
		plot.Axes.Left.TickGenerator = new ScottPlot.TickGenerators.NumericAutomatic
		{
			LabelFormatter = y => ChartFormatting.FormatNanosecondsAxis(y * scaleFactor),
		};
		plot.Axes.Title.Label.Text = methodList.Length > 1 ? "Mean time vs N (comparison)" : "Mean time vs N";
		plot.Axes.Bottom.Label.Text = "N";
		plot.Axes.Left.Label.Text = $"Mean ({unit})";
		plot.ShowLegend(Alignment.UpperRight);
		plot.Legend.FontSize = 11;
	}

	static bool BuildHistoryPlot(Plot plot, BenchmarkDataStore store, string category, IEnumerable<string> methods, IEnumerable<string>? runIds)
	{
		plot.Clear();
		PlotTheme.Apply(plot);

		string[] methodList = methods.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(m => m).ToArray();
		if (methodList.Length == 0)
			return false;

		int? targetN = store.Filter(category, methodList, runIds)
			.Select(r => r.N)
			.Where(n => n is > 0)
			.Select(n => n!.Value)
			.DefaultIfEmpty()
			.Max();

		if (targetN is null or 0)
			return false;

		var allMeans = methodList
			.SelectMany(m => store.Filter(category, [m], runIds).Where(r => r.N == targetN))
			.Select(r => r.MeanNs)
			.ToArray();

		if (allMeans.Length == 0)
			return false;

		var (scale, unit) = ChartFormatting.ToDisplayScale(allMeans);
		double scaleMax = scale.Max(Math.Abs);
		double scaleFactor = scaleMax > 0 ? allMeans.Max() / scaleMax : 1;

		int colorIndex = 0;
		bool multiMethod = methodList.Length > 1;
		string[]? singleLabels = null;

		foreach (string method in methodList)
		{
			var points = store.Filter(category, [method], runIds)
				.Where(r => r.N == targetN)
				.OrderBy(r => r.Timestamp)
				.ToList();

			if (points.Count == 0)
				continue;

			double[] rawYs = points.Select(p => p.MeanNs).ToArray();
			double[] ys = rawYs.Select(v => v / scaleFactor).ToArray();
			double[] xs = Enumerable.Range(0, points.Count).Select(i => (double)i).ToArray();

			var scatter = plot.Add.Scatter(xs, ys);
			scatter.LegendText = multiMethod ? MethodGrouping.ShortLabel(method) : $"N = {targetN:n0}";
			scatter.Color = PlotTheme.SeriesPalette[colorIndex % PlotTheme.SeriesPalette.Length];
			scatter.LineWidth = 2;
			scatter.MarkerSize = 7;

			double min = rawYs.Min();
			int fastestIndex = Array.IndexOf(rawYs, min);
			if (fastestIndex >= 0)
			{
				var highlight = plot.Add.Marker(xs[fastestIndex], ys[fastestIndex]);
				highlight.Color = PlotTheme.Speed;
				highlight.Size = 10;
			}

			if (!multiMethod)
				singleLabels = points.Select(p => BenchmarkDataStore.FormatTimestamp(p.Timestamp)).ToArray();

			colorIndex++;
		}

		if (!multiMethod && singleLabels is not null)
		{
			double[] xs = Enumerable.Range(0, singleLabels.Length).Select(i => (double)i).ToArray();
			plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(
				xs.Select((x, i) => new Tick(x, singleLabels[i])).ToArray());
			plot.Axes.Bottom.TickLabelStyle.Rotation = 45;
			plot.Axes.Bottom.Label.Text = "Run";
		}
		else
		{
			plot.Axes.Bottom.Label.Text = "Run index";
		}

		plot.Axes.Left.TickGenerator = new ScottPlot.TickGenerators.NumericAutomatic
		{
			LabelFormatter = y => ChartFormatting.FormatNanosecondsAxis(y * scaleFactor),
		};
		plot.Axes.Title.Label.Text = $"Run history @ N = {targetN:n0}";
		plot.Axes.Left.Label.Text = $"Mean ({unit})";
		if (multiMethod)
			plot.ShowLegend(Alignment.UpperRight);
		return true;
	}

	static string SeriesLabel(string method, string timestamp, bool includeMethod) =>
		includeMethod
			? $"{MethodGrouping.ShortLabel(method)} · {BenchmarkDataStore.FormatTimestamp(timestamp)}"
			: BenchmarkDataStore.FormatTimestamp(timestamp);
}
