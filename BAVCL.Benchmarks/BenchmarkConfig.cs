namespace BAVCL.Benchmarks;

public static class BenchmarkConfig
{
    public static string GetArtifactsPath() =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "BaselineResults"));
}
