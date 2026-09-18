namespace BAVCL.Samples.Workloads;

internal static class SampleWorkloadHelpers
{
	internal const string OutputName = "y";

	internal static float MaxAbsError(ReadOnlySpan<float> left, ReadOnlySpan<float> right)
	{
		if (left.Length != right.Length)
			throw new ArgumentException($"length mismatch: eager={left.Length}, graph={right.Length}");

		float max = 0f;
		for (int i = 0; i < left.Length; i++)
		{
			float error = MathF.Abs(left[i] - right[i]);
			if (error > max)
				max = error;
		}

		return max;
	}

	internal static void FillRandom(float[] data, Random rng, float min = -1f, float max = 1f)
	{
		float span = max - min;
		for (int i = 0; i < data.Length; i++)
			data[i] = min + (float)rng.NextDouble() * span;
	}
}
