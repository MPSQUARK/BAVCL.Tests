using System.Diagnostics;
using System.Text;
using BAVCL.Modules.Masking;
using BAVCL.Types;

namespace BAVCL.Benchmarks;

/// <summary>
/// 0.9.3 A/B gate — accuracy + like-for-like perf (CPU vs CPU, GPU vs GPU).
/// Run: <c>dotnet run --project BAVCL.Benchmarks -- --mask-compaction-ab-report</c>
/// </summary>
public static class MaskCompactionAbGateReport
{
	static readonly GPU Gpu = GPUManager.Default;

	static readonly int[] PerfSizes = [1_000, 100_000, 10_000_000];
	const int PerfWarmup = 2;
	const int PerfIterations = 5;
	const double PerfGateMinN = 100_000;

	public static string WriteReport()
	{
		KernelModuleLoader.Load<float>(Gpu, KernelWorkloads.Default);
		KernelModuleLoader.Load<int>(Gpu, KernelWorkloads.Default);
		KernelModuleLoader.Load<float>(Gpu, KernelWorkloads.Statistics);
		KernelModuleLoader.Load<int>(Gpu, KernelWorkloads.Statistics);

		string reportPath = BenchmarkConfig.CreateReportPath("mask-compaction-ab-gate-report");

		var sb = new StringBuilder();
		sb.AppendLine("# Mask compaction A/B gate report (0.9.3)");
		sb.AppendLine();
		sb.AppendLine($"**Generated:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
		sb.AppendLine($"**Machine:** {Environment.MachineName}");
		sb.AppendLine($"**GPU:** {Gpu.accelerator.Name}");
		sb.AppendLine();
		sb.AppendLine("## Methodology");
		sb.AppendLine();
		sb.AppendLine("| Track | Legacy | New | Notes |");
		sb.AppendLine("|-------|--------|-----|-------|");
		sb.AppendLine("| **CPU** | `SelectedSourceIndices` + CPU gather | CPU flags + exclusive scan + CPU compact | Both stay on host (`syncToGpu: false`) |");
		sb.AppendLine("| **GPU** | Device `gather` from pre-built index buffer | Full scan pipeline (`maskBroadcastFlags` → scan → compact) | Same fixed mask/vector per N; legacy indices built once per row |");
		sb.AppendLine();
		sb.AppendLine("Accuracy checks use **end-to-end** legacy `FilterX` (CPU indices + GPU gather) vs new `FilterX` (scan pipeline) — correctness only, not timed together.");
		sb.AppendLine();
		sb.AppendLine("## H1 — Accuracy (end-to-end legacy vs scan)");
		sb.AppendLine();

		int accuracyPassed = 0;
		int accuracyFailed = 0;
		foreach (AccuracyCase case_ in AccuracyCases())
		{
			bool passed = case_.Run();
			sb.AppendLine($"- [{(passed ? 'x' : ' ')}] `{case_.Name}` — {(passed ? "PASS" : "FAIL")}");
			if (passed)
				accuracyPassed++;
			else
				accuracyFailed++;
		}

		sb.AppendLine();
		sb.AppendLine($"**Accuracy summary:** {accuracyPassed} passed, {accuracyFailed} failed");
		sb.AppendLine();
		WritePerfTable(sb, "H3a — CPU performance (legacy CPU vs scan CPU)", PerfSizes, RunCpuPair);
		WritePerfTable(sb, "H3b — GPU performance (legacy gather vs scan GPU)", PerfSizes, RunGpuPair);
		sb.AppendLine();
		sb.AppendLine("## H4 — Merge criteria");
		sb.AppendLine();
		bool accuracyOk = accuracyFailed == 0;
		sb.AppendLine($"- [{(accuracyOk ? 'x' : ' ')}] All accuracy A/B checks green");
		sb.AppendLine("- [ ] CPU and GPU perf gates reviewed (see tables above)");
		sb.AppendLine("- [x] Production `SelectedSourceIndices` removed (`MaskCompactionReference` kept for tests)");
		sb.AppendLine();
		sb.AppendLine("## Related commands");
		sb.AppendLine();
		sb.AppendLine("```bash");
		sb.AppendLine("dotnet test ../BAVCL.Tests/BAVCL.Tests.csproj --filter Category=AB");
		sb.AppendLine("dotnet run -c Release --project BAVCL.Benchmarks -- --filter *MaskCompactionAbCpuBenchmarks*");
		sb.AppendLine("dotnet run -c Release --project BAVCL.Benchmarks -- --filter *MaskCompactionAbGpuBenchmarks*");
		sb.AppendLine("```");

		File.WriteAllText(reportPath, sb.ToString());
		Console.WriteLine(sb.ToString());
		Console.WriteLine();
		Console.WriteLine($"Report written: {reportPath}");
		return reportPath;
	}

	static void WritePerfTable(StringBuilder sb, string title, int[] sizes, Func<Vector, Mask, int[], int[], PerfPair> runPair)
	{
		sb.AppendLine($"## {title}");
		sb.AppendLine();
		sb.AppendLine($"Gate: scan ≤ legacy at N ≥ {PerfGateMinN:n0} (5% tolerance).");
		sb.AppendLine();
		sb.AppendLine("| N | Filter legacy (ms) | Filter scan (ms) | Ratio | Gate | Partition legacy | Partition scan | Ratio | Gate |");
		sb.AppendLine("|---|-------------------:|-----------------:|------:|:----:|-----------------:|---------------:|------:|:----:|");

		int gatesPassed = 0;
		int gatesTotal = 0;
		foreach (int n in sizes)
		{
			(Vector vector, Mask mask, int[] filterIndices, int[] falseIndices) = CreateFixture(n);
			PerfPair pair = runPair(vector, mask, filterIndices, falseIndices);

			bool filterGate = n < PerfGateMinN || pair.FilterScan <= pair.FilterLegacy * 1.05;
			bool partGate = n < PerfGateMinN || pair.PartitionScan <= pair.PartitionLegacy * 1.05;
			if (n >= PerfGateMinN)
			{
				gatesTotal += 2;
				if (filterGate) gatesPassed++;
				if (partGate) gatesPassed++;
			}

			double filterRatio = pair.FilterLegacy / Math.Max(pair.FilterScan, 1e-9);
			double partRatio = pair.PartitionLegacy / Math.Max(pair.PartitionScan, 1e-9);
			sb.AppendLine(
				$"| {n:n0} | {pair.FilterLegacy:F3} | {pair.FilterScan:F3} | {filterRatio:F2}× | {(filterGate ? "PASS" : "FAIL")} | {pair.PartitionLegacy:F3} | {pair.PartitionScan:F3} | {partRatio:F2}× | {(partGate ? "PASS" : "FAIL")} |");
		}

		sb.AppendLine();
		sb.AppendLine($"**Gate (N ≥ {PerfGateMinN:n0}):** {gatesPassed}/{gatesTotal} passed");
		sb.AppendLine();
	}

	static PerfPair RunCpuPair(Vector vector, Mask mask, int[] _, int[] __) => new(
		MedianMs(() => MaskCompactionReference.Filter(vector, mask, syncToGpu: false)),
		MedianMs(() => MaskCompactionCpu.Filter(vector, mask, syncToGpu: false)),
		MedianMs(() => MaskCompactionReference.Partition(vector, mask, syncToGpu: false)),
		MedianMs(() => MaskCompactionCpu.Partition(vector, mask, syncToGpu: false)));

	static PerfPair RunGpuPair(Vector vector, Mask mask, int[] filterIndices, int[] falseIndices)
	{
		int[] trueIndices = filterIndices;
		return new PerfPair(
			MedianMs(() => MaskCompactionReference.GatherFilterX(vector, filterIndices)),
			MedianMs(() => MaskCompaction.FilterX(vector, mask)),
			MedianMs(() => MaskCompactionReference.GatherPartitionX(vector, trueIndices, falseIndices)),
			MedianMs(() => MaskCompaction.PartitionX(vector, mask)));
	}

	static (Vector Vector, Mask Mask, int[] FilterIndices, int[] FalseIndices) CreateFixture(int n)
	{
		var rng = new Random(17);
		float[] data = new float[n];
		bool[] bits = new bool[n];
		for (int i = 0; i < n; i++)
		{
			data[i] = (float)rng.NextDouble();
			bits[i] = rng.NextDouble() > 0.5;
		}

		Vector vector = new(Gpu, data, cache: true);
		Mask mask = new(Gpu, bits, cache: true);
		Shape vectorShape = vector.Shape();
		Shape maskShape = mask.Shape();
		Shape outputShape = vectorShape.BroadcastWith(maskShape);

		int[] filterIndices = MaskCompactionReference.SelectedSourceIndices(mask, maskShape, vectorShape, outputShape, selected: true);
		int[] falseIndices = MaskCompactionReference.SelectedSourceIndices(mask, maskShape, vectorShape, outputShape, selected: false);
		return (vector, mask, filterIndices, falseIndices);
	}

	static IEnumerable<AccuracyCase> AccuracyCases()
	{
		yield return new("Filter 1D sparse", () =>
		{
			Vector v = new(Gpu, [10f, 20f, 30f, 40f]);
			Mask m = new(Gpu, [true, false, true, false]);
			return FloatEqual(MaskCompactionReference.FilterX(v, m), MaskCompaction.FilterX(v, m));
		});

		yield return new("CPU filter matches GPU scan", () =>
		{
			Vector v = new(Gpu, [10f, 20f, 30f, 40f]);
			Mask m = new(Gpu, [true, false, true, false]);
			return FloatEqual(MaskCompactionCpu.Filter(v, m, syncToGpu: false), MaskCompaction.FilterX(v, m));
		});

		yield return new("Partition 1D", () =>
		{
			Vector v = new(Gpu, [1f, 2f, 3f, 4f]);
			Mask m = new(Gpu, [true, false, true, false]);
			var (rt, rf) = MaskCompactionReference.PartitionX(v, m);
			var (nt, nf) = MaskCompaction.PartitionX(v, m);
			return FloatEqual(rt, nt) && FloatEqual(rf, nf);
		});

		yield return new("Where indices", () =>
		{
			Mask m = new(Gpu, [false, true, false, true, true]);
			return IntEqual(MaskCompactionReference.Where(m), MaskCompaction.WhereX(m));
		});

		yield return new("Tail word (N=33)", () =>
		{
			bool[] bits = [.. Enumerable.Range(0, 33).Select(i => i % 2 == 0)];
			Vector v = new(Gpu, [.. Enumerable.Range(0, 33).Select(i => (float)i)]);
			Mask m = new(Gpu, bits);
			return FloatEqual(MaskCompactionReference.FilterX(v, m), MaskCompaction.FilterX(v, m));
		});

		yield return new("Broadcast 2D", () =>
		{
			Vector v = new(Gpu, [1f, 2f, 3f, 4f], columns: 2);
			Mask m = new(Gpu, [true, false], columns: 0);
			return FloatEqual(MaskCompactionReference.FilterX(v, m), MaskCompaction.FilterX(v, m));
		});

		yield return new("Operator | mask", () =>
		{
			Vector v = new(Gpu, [5f, 6f, 7f, 8f]);
			Mask m = new(Gpu, [true, true, false, false]);
			return FloatEqual(MaskCompactionReference.FilterX(v, m), v | m);
		});
	}

	static double MedianMs(Action action)
	{
		for (int i = 0; i < PerfWarmup; i++)
			action();

		double[] samples = new double[PerfIterations];
		for (int i = 0; i < PerfIterations; i++)
		{
			Gpu.Synchronize();
			var sw = Stopwatch.StartNew();
			action();
			Gpu.Synchronize();
			sw.Stop();
			samples[i] = sw.Elapsed.TotalMilliseconds;
		}

		Array.Sort(samples);
		return samples[PerfIterations / 2];
	}

	static bool FloatEqual(Vector expected, Vector actual)
	{
		if (expected.Length != actual.Length)
			return false;

		ReadOnlySpan<float> a = expected.RetrieveReadOnlySpan();
		ReadOnlySpan<float> b = actual.RetrieveReadOnlySpan();
		for (int i = 0; i < a.Length; i++)
		{
			if (a[i] != b[i])
				return false;
		}

		return true;
	}

	static bool IntEqual(VectorInt expected, VectorInt actual)
	{
		if (expected.Length != actual.Length)
			return false;

		ReadOnlySpan<int> a = expected.RetrieveReadOnlySpan();
		ReadOnlySpan<int> b = actual.RetrieveReadOnlySpan();
		for (int i = 0; i < a.Length; i++)
		{
			if (a[i] != b[i])
				return false;
		}

		return true;
	}

	readonly struct AccuracyCase(string name, Func<bool> run)
	{
		public string Name { get; } = name;

		public bool Run() => run();
	}

	readonly struct PerfPair(double filterLegacy, double filterScan, double partitionLegacy, double partitionScan)
	{
		public double FilterLegacy { get; } = filterLegacy;
		public double FilterScan { get; } = filterScan;
		public double PartitionLegacy { get; } = partitionLegacy;
		public double PartitionScan { get; } = partitionScan;
	}
}
