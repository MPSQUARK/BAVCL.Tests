namespace BAVCL.Samples.Workloads;

/// <summary>Registry of graph comparison showcase workloads.</summary>
public static class SampleWorkloads
{
	static readonly ISampleWorkload[] AllWorkloads =
	[
		new InterpolationLerpWorkload(),
		new StatsLogTransformWorkload(),
		new GraphicsReinhardWorkload(),
		new NeuralDenseLayerWorkload(),
		new SolverJacobiStepWorkload(),
	];

	public static IReadOnlyList<ISampleWorkload> All => AllWorkloads;

	public static ISampleWorkload Get(string name) =>
		AllWorkloads.FirstOrDefault(w => w.Name == name)
		?? throw new ArgumentException($"unknown workload '{name}'.", nameof(name));
}
