using System;
using System.Numerics;
using BAVCL.Core.Helpers;

namespace BAVCL.Benchmarks.Baselines;

/// <summary>
/// Reference sum implementations for accuracy reports only — not used in production.
/// </summary>
internal static class SumBaselines
{
	internal static float NaiveSimd(ReadOnlySpan<float> data)
	{
		int lanes = Vector<float>.Count;
		int index = 0;
		Vector<float> sumVec = Vector<float>.Zero;

		for (; index <= data.Length - lanes; index += lanes)
			sumVec += new Vector<float>(data.Slice(index, lanes));

		float result = 0f;
		for (int lane = 0; lane < Vector<float>.Count; lane++)
			result += sumVec[lane];

		for (; index < data.Length; index++)
			result += data[index];

		return result;
	}

	internal static float Kahan(ReadOnlySpan<float> data)
	{
		float sum = 0f;
		float compensation = 0f;
		for (int index = 0; index < data.Length; index++)
		{
			float corrected = data[index] - compensation;
			float total = sum + corrected;
			compensation = (total - sum) - corrected;
			sum = total;
		}

		return sum;
	}

	internal static float NeumaierFloat32(ReadOnlySpan<float> data)
	{
		float sum = 0f;
		float compensation = 0f;
		for (int index = 0; index < data.Length; index++)
			CompensatedSumOps.NeumaierAdd(ref sum, ref compensation, data[index]);
		return sum + compensation;
	}
}
