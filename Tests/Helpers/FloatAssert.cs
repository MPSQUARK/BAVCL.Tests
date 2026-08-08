namespace BAVCL.Tests.Helpers;

public static class FloatAssert
{
    public const float DefaultTolerance = 1e-4f;

    public static void ShouldBeCloseTo(this float[] actual, float[] expected, float tolerance = DefaultTolerance)
    {
        actual.Should().HaveCount(expected.Length);
        for (int i = 0; i < expected.Length; i++)
            IsClose(actual[i], expected[i], tolerance).Should().BeTrue(
                $"expected[{i}]={expected[i]}, actual[{i}]={actual[i]}");
    }

    public static void ShouldBeCloseTo(this float actual, float expected, float tolerance = DefaultTolerance) =>
        IsClose(actual, expected, tolerance).Should().BeTrue(
            $"expected={expected}, actual={actual}");

    static bool IsClose(float actual, float expected, float tolerance)
    {
        if (float.IsNaN(actual) && float.IsNaN(expected))
            return true;

        if (float.IsInfinity(actual) && float.IsInfinity(expected))
            return Math.Sign(actual) == Math.Sign(expected);

        float diff = MathF.Abs(actual - expected);
        float scale = MathF.Max(MathF.Abs(actual), MathF.Abs(expected));
        return diff <= tolerance + tolerance * scale;
    }
}
