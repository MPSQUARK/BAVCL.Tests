using BAVCL.Samples.Workloads;
using FluentAssertions;

namespace BAVCL.Tests.Graphs;

public class GraphSampleParityTests
{
	static readonly GPU Gpu = GPUManager.Default;

	[Theory]
	[InlineData(100)]
	[InlineData(256)]
	public void InterpolationLerp_MatchesEager(int n)
	{
		var workload = new InterpolationLerpWorkload();
		workload.Initialize(Gpu, n);
		workload.ComputeMaxAbsError().Should().BeLessThan(1e-4f);
	}

	[Theory]
	[InlineData(100)]
	[InlineData(256)]
	public void StatsLogTransform_MatchesEager(int n)
	{
		var workload = new StatsLogTransformWorkload();
		workload.Initialize(Gpu, n);
		workload.ComputeMaxAbsError().Should().BeLessThan(1e-4f);
	}

	[Theory]
	[InlineData(100)]
	[InlineData(256)]
	public void GraphicsReinhard_MatchesEager(int n)
	{
		var workload = new GraphicsReinhardWorkload();
		workload.Initialize(Gpu, n);
		workload.ComputeMaxAbsError().Should().BeLessThan(1e-4f);
	}

	[Theory]
	[InlineData(16)]
	[InlineData(32)]
	public void NeuralDenseLayer_MatchesEager(int k)
	{
		var workload = new NeuralDenseLayerWorkload();
		workload.Initialize(Gpu, k);
		workload.ComputeMaxAbsError().Should().BeLessThan(1e-4f);
	}

	[Theory]
	[InlineData(16)]
	[InlineData(32)]
	public void SolverJacobiStep_MatchesEager(int k)
	{
		var workload = new SolverJacobiStepWorkload();
		workload.Initialize(Gpu, k);
		workload.ComputeMaxAbsError().Should().BeLessThan(1e-4f);
	}
}
