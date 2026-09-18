using BAVCL.Graphs;

namespace BAVCL.Samples.Workloads;

/// <summary>1D linear interpolation: <c>y = y0·(1−t) + y1·t</c>.</summary>
public sealed class InterpolationLerpWorkload : ISampleWorkload
{
	GPU _gpu = null!;
	int _n;

	float[] _y0Host = null!;
	float[] _y1Host = null!;
	float[] _tHost = null!;

	Vector _y0 = null!;
	Vector _y1 = null!;
	Vector _t = null!;
	Vector _sink = null!;

	public string Name => "InterpolationLerp";

	public bool UsesMatrixSize => false;

	public void Initialize(GPU gpu, int n)
	{
		_gpu = gpu;
		_n = n;

		var rng = new Random(42);
		_y0Host = new float[n];
		_y1Host = new float[n];
		_tHost = new float[n];
		SampleWorkloadHelpers.FillRandom(_y0Host, rng, 0f, 100f);
		SampleWorkloadHelpers.FillRandom(_y1Host, rng, 0f, 100f);
		SampleWorkloadHelpers.FillRandom(_tHost, rng, 0f, 1f);

		_y0 = new Vector(gpu, _y0Host, cache: true);
		_y1 = new Vector(gpu, _y1Host, cache: true);
		_t = new Vector(gpu, _tHost, cache: true);
		_sink = new Vector(gpu, 1);
	}

	public void RunEager() => _sink = _y0 * (1f - _t) + _y1 * _t;

	public CompiledGraph CaptureAndCompile()
	{
		using var graph = Graph.Capture(_gpu, Name);
		var y0 = graph.Input<float>("y0", _n);
		var y1 = graph.Input<float>("y1", _n);
		var t = graph.Input<float>("t", _n);
		var y = y0 * (1f - t) + y1 * t;
		graph.Output(SampleWorkloadHelpers.OutputName, y);
		return graph.Compile();
	}

	public void Feed(CompiledGraph graph)
	{
		graph.Feed("y0", _y0Host);
		graph.Feed("y1", _y1Host);
		graph.Feed("t", _tHost);
	}

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
