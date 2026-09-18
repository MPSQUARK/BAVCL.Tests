using BAVCL.Core;
using BAVCL.Graphs;
using BAVCL.Modules.Arithmetic;
using BAVCL.Modules.GpuOps;

namespace BAVCL.Samples.Workloads;

/// <summary>Log transform: <c>y = Log(Abs(x) + ε)</c> (natural log).</summary>
public sealed class StatsLogTransformWorkload : ISampleWorkload
{
	const float Epsilon = 1e-6f;
	const float LogBase = (float)Math.E;

	GPU _gpu = null!;
	int _n;

	float[] _xHost = null!;
	Vector _x = null!;
	Vector _sink = null!;

	public string Name => "StatsLogTransform";

	public bool UsesMatrixSize => false;

	public void Initialize(GPU gpu, int n)
	{
		_gpu = gpu;
		_n = n;

		var rng = new Random(43);
		_xHost = new float[n];
		SampleWorkloadHelpers.FillRandom(_xHost, rng, -10f, 10f);

		_x = new Vector(gpu, _xHost, cache: true);
		_sink = new Vector(gpu, 1);
	}

	public void RunEager() => _sink = _x.AbsX().OP(Epsilon, Operations.add).LogX(LogBase);

	public CompiledGraph CaptureAndCompile()
	{
		using var graph = Graph.Capture(_gpu, Name);
		var x = graph.Input<float>("x", _n);
		var y = (x.Abs() + Epsilon).Log(LogBase);
		graph.Output(SampleWorkloadHelpers.OutputName, y);
		return graph.Compile();
	}

	public void Feed(CompiledGraph graph) => graph.Feed("x", _xHost);

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
