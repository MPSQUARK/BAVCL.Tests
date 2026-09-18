using BAVCL.Graphs;
using BAVCL.Modules.Arithmetic;

namespace BAVCL.Samples.Workloads;

/// <summary>Jacobi update: <c>x ← x + ω·(b − A·x)</c>.</summary>
public sealed class SolverJacobiStepWorkload : ISampleWorkload
{
	const float Omega = 0.9f;

	GPU _gpu = null!;
	int _k;

	float[] _aHost = null!;
	float[] _xHost = null!;
	float[] _bHost = null!;

	Vector _a = null!;
	Vector _x = null!;
	Vector _b = null!;
	Vector _sink = null!;

	public string Name => "SolverJacobiStep";

	public bool UsesMatrixSize => true;

	public void Initialize(GPU gpu, int n)
	{
		_gpu = gpu;
		_k = n;

		int matrixLength = _k * _k;
		var rng = new Random(46);
		_aHost = new float[matrixLength];
		_xHost = new float[_k];
		_bHost = new float[_k];
		SampleWorkloadHelpers.FillRandom(_aHost, rng, -0.25f, 0.25f);
		SampleWorkloadHelpers.FillRandom(_xHost, rng, -1f, 1f);
		SampleWorkloadHelpers.FillRandom(_bHost, rng, -1f, 1f);

		// Diagonal dominance for stable iteration smoke data.
		for (int i = 0; i < _k; i++)
			_aHost[i * _k + i] += _k;

		_a = new Vector(gpu, _aHost, columns: _k, cache: true);
		_x = new Vector(gpu, _xHost, columns: 1, cache: true);
		_b = new Vector(gpu, _bHost, columns: 1, cache: true);
		_sink = new Vector(gpu, 1, columns: 1);
	}

	public void RunEager() => _sink = _x + Omega * (_b - _a.MatrixMultiplyX(_x));

	public CompiledGraph CaptureAndCompile()
	{
		using var graph = Graph.Capture(_gpu, Name);
		var a = graph.Input<float>("A", _k * _k, columns: _k);
		var x = graph.Input<float>("x", _k, columns: 1);
		var b = graph.Input<float>("b", _k, columns: 1);
		var updated = x + Omega * (b - a.MatMul(x));
		graph.Output(SampleWorkloadHelpers.OutputName, updated);
		return graph.Compile();
	}

	public void Feed(CompiledGraph graph)
	{
		graph.Feed("A", _aHost);
		graph.Feed("x", _xHost);
		graph.Feed("b", _bHost);
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
