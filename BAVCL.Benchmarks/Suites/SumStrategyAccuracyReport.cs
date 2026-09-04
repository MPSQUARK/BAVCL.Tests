using BAVCL.Benchmarks.Baselines;
using BAVCL.Core.Helpers;

namespace BAVCL.Benchmarks;

/// <summary>
/// Reports abs error for sum strategies — run manually via <c>--sum-accuracy-report</c>.
/// </summary>
public static class SumStrategyAccuracyReport
{
	public static void Print(int n = 10_000_000)
	{
		float[] pathological = Enumerable.Repeat(0.1f, n).ToArray();
		float expected = n * 0.1f;

		float naive = SumBaselines.NaiveSimd(pathological);
		float kahan = SumBaselines.Kahan(pathological);
		float neumaierF32 = SumBaselines.NeumaierFloat32(pathological);
		float production = CompensatedSumOps.SumNeumaier(pathological);

		Console.WriteLine($"Sum accuracy (N={n:n0}, value=0.1f, expected={expected:G9})");
		Console.WriteLine($"  Naive SIMD:     abs err = {MathF.Abs(naive - expected):E3}");
		Console.WriteLine($"  Kahan:          abs err = {MathF.Abs(kahan - expected):E3}");
		Console.WriteLine($"  Neumaier (f32): abs err = {MathF.Abs(neumaierF32 - expected):E3}");
		Console.WriteLine($"  Production:     abs err = {MathF.Abs(production - expected):E3}");
	}
}
