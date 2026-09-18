using BAVCL.Graphs;
using BAVCL.Samples.Workloads;
using BenchmarkDotNet.Attributes;

namespace BAVCL.Benchmarks;

/// <summary>
/// Eager vs captured graph on the same graph-enabled BAVCL build for five showcase workloads.
/// </summary>
[MemoryDiagnoser]
public class GraphComparisonBenchmarks
{
	static readonly GPU Gpu = GPUManager.Default;

	readonly InterpolationLerpWorkload _interpolation = new();
	readonly StatsLogTransformWorkload _logTransform = new();
	readonly GraphicsReinhardWorkload _reinhard = new();
	readonly NeuralDenseLayerWorkload _denseLayer = new();
	readonly SolverJacobiStepWorkload _jacobi = new();

	CompiledGraph? _interpolationPlan;
	CompiledGraph? _logTransformPlan;
	CompiledGraph? _reinhardPlan;
	CompiledGraph? _denseLayerPlan;
	CompiledGraph? _jacobiPlan;

	[Params(BenchmarkSizes.Small, BenchmarkSizes.Typical, BenchmarkSizes.Large)]
	public int N { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		_interpolation.Initialize(Gpu, GraphComparisonSizes.VectorLength(N));
		_logTransform.Initialize(Gpu, GraphComparisonSizes.VectorLength(N));
		_reinhard.Initialize(Gpu, GraphComparisonSizes.VectorLength(N));
		_denseLayer.Initialize(Gpu, GraphComparisonSizes.MatrixOrder(N));
		_jacobi.Initialize(Gpu, GraphComparisonSizes.MatrixOrder(N));

		_interpolationPlan = _interpolation.CaptureAndCompile();
		_logTransformPlan = _logTransform.CaptureAndCompile();
		_reinhardPlan = _reinhard.CaptureAndCompile();
		_denseLayerPlan = _denseLayer.CaptureAndCompile();
		_jacobiPlan = _jacobi.CaptureAndCompile();
	}

	[GlobalCleanup]
	public void Cleanup()
	{
		DisposePlan(ref _interpolationPlan);
		DisposePlan(ref _logTransformPlan);
		DisposePlan(ref _reinhardPlan);
		DisposePlan(ref _denseLayerPlan);
		DisposePlan(ref _jacobiPlan);
	}

	static void DisposePlan(ref CompiledGraph? plan)
	{
		plan?.Dispose();
		plan = null;
	}

	[Benchmark] public void InterpolationLerp_Eager() => _interpolation.RunEager();

	[Benchmark]
	public void InterpolationLerp_Graph_Run()
	{
		_interpolation.Feed(_interpolationPlan!);
		_interpolationPlan!.Run();
	}

	[Benchmark]
	public void InterpolationLerp_Graph_Compile()
	{
		using CompiledGraph plan = _interpolation.CaptureAndCompile();
	}

	[Benchmark] public void StatsLogTransform_Eager() => _logTransform.RunEager();

	[Benchmark]
	public void StatsLogTransform_Graph_Run()
	{
		_logTransform.Feed(_logTransformPlan!);
		_logTransformPlan!.Run();
	}

	[Benchmark]
	public void StatsLogTransform_Graph_Compile()
	{
		using CompiledGraph plan = _logTransform.CaptureAndCompile();
	}

	[Benchmark] public void GraphicsReinhard_Eager() => _reinhard.RunEager();

	[Benchmark]
	public void GraphicsReinhard_Graph_Run()
	{
		_reinhard.Feed(_reinhardPlan!);
		_reinhardPlan!.Run();
	}

	[Benchmark]
	public void GraphicsReinhard_Graph_Compile()
	{
		using CompiledGraph plan = _reinhard.CaptureAndCompile();
	}

	[Benchmark] public void NeuralDenseLayer_Eager() => _denseLayer.RunEager();

	[Benchmark]
	public void NeuralDenseLayer_Graph_Run()
	{
		_denseLayer.Feed(_denseLayerPlan!);
		_denseLayerPlan!.Run();
	}

	[Benchmark]
	public void NeuralDenseLayer_Graph_Compile()
	{
		using CompiledGraph plan = _denseLayer.CaptureAndCompile();
	}

	[Benchmark] public void SolverJacobiStep_Eager() => _jacobi.RunEager();

	[Benchmark]
	public void SolverJacobiStep_Graph_Run()
	{
		_jacobi.Feed(_jacobiPlan!);
		_jacobiPlan!.Run();
	}

	[Benchmark]
	public void SolverJacobiStep_Graph_Compile()
	{
		using CompiledGraph plan = _jacobi.CaptureAndCompile();
	}
}
