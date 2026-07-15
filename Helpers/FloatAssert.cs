using BAVCL.Utility;

namespace BAVCL.Tests.Helpers;

public static class FloatAssert
{
    public const float DefaultTolerance = 1e-4f;

    public static void ShouldBeCloseTo(this float[] actual, float[] expected, float tolerance = DefaultTolerance)
    {
        actual.Should().HaveCount(expected.Length);
        for (int i = 0; i < expected.Length; i++)
            Util.IsClose(actual[i], expected[i], tolerance).Should().BeTrue(
                $"expected[{i}]={expected[i]}, actual[{i}]={actual[i]}");
    }

    public static void ShouldBeCloseTo(this float actual, float expected, float tolerance = DefaultTolerance) =>
        Util.IsClose(actual, expected, tolerance).Should().BeTrue(
            $"expected={expected}, actual={actual}");
}
