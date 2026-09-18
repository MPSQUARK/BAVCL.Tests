using BAVCL.Graphs;

namespace BAVCL.Samples.Workloads;

/// <summary>Reinhard tone map: <c>c = c / (1 + c)</c>.</summary>
public sealed class GraphicsReinhardWorkload : ISampleWorkload
{
	GPU _gpu = null!;
	int _n;

	float[] _cHost = null!;
	Vector _c = null!;
	Vector _sink = null!;

	public string Name => "GraphicsReinhard";

	public bool UsesMatrixSize => false;

	public void Initialize(GPU gpu, int n)
	{
		_gpu = gpu;
		_n = n;

		var rng = new Random(44);
		_cHost = new float[n];
		SampleWorkloadHelpers.FillRandom(_cHost, rng, 0f, 4f);

		_c = new Vector(gpu, _cHost, cache: true);
		_sink = new Vector(gpu, 1);
	}

	public void RunEager() => _sink = _c / (1f + _c);

	public CompiledGraph CaptureAndCompile()
	{
		using var graph = Graph.Capture(_gpu, Name);
		var c = graph.Input<float>("c", _n);
		var mapped = c / (1f + c);
		graph.Output(SampleWorkloadHelpers.OutputName, mapped);
		return graph.Compile();
	}

	public void Feed(CompiledGraph graph) => graph.Feed("c", _cHost);

	public float ComputeMaxAbsError(float tolerance = 1e-4f)
	{
		RunEager();
		ReadOnlySpan<float> eager = _sink.RetrieveReadOnlySpan();

		using CompiledGraph plan = CaptureAndCompile();
		Feed(plan);
		plan.Run();
		float[] graphResult = plan.Fetch<float>(SampleWorkloadHelpers.OutputName);

		float error = SampleWorkloadHelpers.MaxAbsError(eager, graphResult);
		if (error > tolerance)
			throw new InvalidOperationException($"{Name} parity failed: max abs error {error} > {tolerance}.");

		return error;
	}
}
