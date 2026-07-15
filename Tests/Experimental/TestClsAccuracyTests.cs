using BAVCL.Experimental;
using BAVCL.Utility;

namespace BAVCL.Tests.Experimental;

public class TestClsAccuracyTests
{
    private const float SqrtTolerance = 0.01f;

    [Theory]
    [InlineData(1f)]
    [InlineData(4f)]
    [InlineData(9f)]
    [InlineData(16f)]
    [InlineData(2f)]
    public void Sqrt_ApproximatesMathSqrt(float input)
    {
        var expected = MathF.Sqrt(input);
        var actual = TestCls.Sqrt(input);

        Util.IsClose(actual, expected, SqrtTolerance).Should().BeTrue(
            $"Sqrt({input}): expected {expected}, got {actual}");
    }

    [Theory]
    [InlineData(1f)]
    [InlineData(8f)]
    [InlineData(27f)]
    public void CBRT_ApproximatesMathCbrt(double input)
    {
        var expected = Math.Cbrt(input);
        var actual = TestCls.CBRT(input);

        Math.Abs(actual - expected).Should().BeLessThan(0.1);
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(8.0)]
    [InlineData(1024.0)]
    public void LOG2_ApproximatesMathLog2(double input)
    {
        var expected = Math.Log2(input);
        var actual = TestCls.LOG2(input);

        Math.Abs(actual - expected).Should().BeLessThan(0.1);
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(10.0)]
    [InlineData(100.0)]
    public void LOG10_ApproximatesMathLog10(double input)
    {
        var expected = Math.Log10(input);
        var actual = TestCls.LOG10(input);

        Math.Abs(actual - expected).Should().BeLessThan(0.1);
    }
}
