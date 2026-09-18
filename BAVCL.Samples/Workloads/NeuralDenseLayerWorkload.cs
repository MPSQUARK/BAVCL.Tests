using BAVCL.Graphs;
using BAVCL.Modules.Arithmetic;

namespace BAVCL.Samples.Workloads;

/// <summary>Dense layer with ReLU: <c>y = ReLU(W·x + b)</c> via <c>(y + |y|) / 2</c>.</summary>
public sealed class NeuralDenseLayerWorkload : ISampleWorkload
{
	GPU _gpu = null!;
	int _k;

	float[] _wHost = null!;
	float[] _xHost = null!;
	float[] _bHost = null!;

	Vector _w = null!;
	Vector _x = null!;
	Vector _b = null!;
	Vector _sink = null!;

	public string Name => "NeuralDenseLayer";

	public bool UsesMatrixSize => true;

	public void Initialize(GPU gpu, int n)
	{
		_gpu = gpu;
		_k = n;

		int matrixLength = _k * _k;
		var rng = new Random(45);
		_wHost = new float[matrixLength];
		_xHost = new float[_k];
		_bHost = new float[_k];
		SampleWorkloadHelpers.FillRandom(_wHost, rng, -0.5f, 0.5f);
		SampleWorkloadHelpers.FillRandom(_xHost, rng, -1f, 1f);
		SampleWorkloadHelpers.FillRandom(_bHost, rng, -0.1f, 0.1f);

		_w = new Vector(gpu, _wHost, columns: _k, cache: true);
		_x = new Vector(gpu, _xHost, columns: 1, cache: true);
		_b = new Vector(gpu, _bHost, columns: 1, cache: true);
		_sink = new Vector(gpu, 1, columns: 1);
	}

	public void RunEager()
	{
		Vector linear = _w.MatrixMultiplyX(_x) + _b;
		_sink = (linear + linear.AbsX()) * 0.5f;
	}

	public CompiledGraph CaptureAndCompile()
	{
		using var graph = Graph.Capture(_gpu, Name);
		var w = graph.Input<float>("W", _k * _k, columns: _k);
		var x = graph.Input<float>("x", _k, columns: 1);
		var b = graph.Input<float>("b", _k, columns: 1);
		var linear = w.MatMul(x) + b;
		var relu = (linear + linear.Abs()) * 0.5f;
		graph.Output(SampleWorkloadHelpers.OutputName, relu);
		return graph.Compile();
	}

	public void Feed(CompiledGraph graph)
	{
		graph.Feed("W", _wHost);
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
