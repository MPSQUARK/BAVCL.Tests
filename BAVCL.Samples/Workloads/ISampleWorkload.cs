using BAVCL.Graphs;

namespace BAVCL.Samples.Workloads;

/// <summary>
/// Shared eager vs graph workload used by dotnet-script demos, benchmarks, and parity tests.
/// </summary>
public interface ISampleWorkload
{
	/// <summary>Short identifier (e.g. <c>InterpolationLerp</c>).</summary>
	string Name { get; }

	/// <summary>When true, <paramref name="n"/> is the matrix order K rather than a 1D length.</summary>
	bool UsesMatrixSize { get; }

	/// <summary>Allocates host and GPU inputs for size <paramref name="n"/>.</summary>
	void Initialize(GPU gpu, int n);

	/// <summary>Runs the eager GPU pipeline once, retaining the result in an internal sink.</summary>
	void RunEager();

	/// <summary>Captures and compiles the graph for the current size.</summary>
	CompiledGraph CaptureAndCompile();

	/// <summary>Uploads host inputs into a compiled graph.</summary>
	void Feed(CompiledGraph graph);

	/// <summary>Maximum absolute difference between the last eager and graph results.</summary>
	float ComputeMaxAbsError(float tolerance = 1e-4f);
}
