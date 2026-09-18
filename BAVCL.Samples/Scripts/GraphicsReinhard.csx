#r "../../../BAVCL-graph/bin/Release/net10.0/BAVCL.dll"
#r "../bin/Release/net10.0/BAVCL.Samples.dll"

using System.Diagnostics;
using BAVCL;
using BAVCL.Graphs;
using BAVCL.Samples.Workloads;

const int N = 1_000_000;
var gpu = GPUManager.Default;
var workload = new GraphicsReinhardWorkload();
workload.Initialize(gpu, N);

float parity = workload.ComputeMaxAbsError();
Console.WriteLine($"GraphicsReinhard parity max |eager - graph| = {parity:E3}");

const int iterations = 20;
var eagerWatch = Stopwatch.StartNew();
for (int i = 0; i < iterations; i++)
	workload.RunEager();
eagerWatch.Stop();

using (CompiledGraph plan = workload.CaptureAndCompile())
{
	var graphWatch = Stopwatch.StartNew();
	for (int i = 0; i < iterations; i++)
	{
		workload.Feed(plan);
		plan.Run();
	}
	graphWatch.Stop();

	Console.WriteLine($"N={N:N0}  eager={eagerWatch.ElapsedMilliseconds / iterations:F2} ms/iter");
	Console.WriteLine($"N={N:N0}  graph={graphWatch.ElapsedMilliseconds / iterations:F2} ms/iter");
}
