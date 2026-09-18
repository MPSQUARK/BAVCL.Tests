using ScottPlot;

namespace BAVCL.Benchmarks.Viewer.Services;

public static class PlotTheme
{
	public static readonly Color Teal = Color.FromHex("#0d8f8a");
	public static readonly Color Navy = Color.FromHex("#1e4d8c");
	public static readonly Color Speed = Color.FromHex("#e0634a");
	public static readonly Color Muted = Color.FromHex("#7b8da6");
	public static readonly Color Grid = Color.FromHex("#d8e0ec");

	public static readonly Color[] SeriesPalette =
	[
		Teal,
		Navy,
		Speed,
		Color.FromHex("#2a4a73"),
		Color.FromHex("#5b9bd5"),
		Color.FromHex("#0d8f5f"),
	];

	public static void Apply(Plot plot)
	{
		plot.FigureBackground.Color = Colors.White;
		plot.DataBackground.Color = Colors.White;
		plot.Axes.Color(Muted);
		plot.Grid.MajorLineColor = Grid.WithAlpha(0.8);
		plot.Legend.BackgroundColor = Colors.White.WithAlpha(0.9);
		plot.Legend.OutlineColor = Grid;
		plot.Legend.FontName = "DM Sans";
		plot.Axes.Title.Label.FontName = "Sora";
		plot.Axes.Bottom.Label.FontName = "DM Sans";
		plot.Axes.Left.Label.FontName = "DM Sans";
	}
}
