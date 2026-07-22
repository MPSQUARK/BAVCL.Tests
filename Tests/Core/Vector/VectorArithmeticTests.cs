using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorTests;

public class VectorArithmeticTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void UnaryPlus_ReturnsAbs()
    {
        var vector = CreateVector(VectorFactory.MixedSigns);

        var result = +vector;
        var expected = SyncValues(vector.AbsX());

        SyncValues(result).ShouldBeCloseTo(expected);
    }

    [Fact]
    public void UnaryMinus_NegatesValues()
    {
        var vector = CreateVector([1f, -2f, 3f]);

        var result = -vector;

        SyncValues(result).ShouldBeCloseTo([-1f, 2f, -3f]);
    }

    [Theory]
    [MemberData(nameof(BinaryOpData))]
    public void VectorVectorOperators_MatchCpuReference(
        float[] a, float[] b, string op)
    {
        var vectorA = CreateVector(a);
        var vectorB = CreateVector(b);

        var result = op switch
        {
            "add" => vectorA + vectorB,
            "sub" => vectorA - vectorB,
            "mul" => vectorA * vectorB,
            "div" => vectorA / vectorB,
            "pow" => vectorA ^ vectorB,
            _ => throw new ArgumentException(op)
        };

        var expected = op switch
        {
            "add" => CpuReference.Add(a, b),
            "sub" => CpuReference.Subtract(a, b),
            "mul" => CpuReference.Multiply(a, b),
            "div" => CpuReference.Divide(a, b),
            "pow" => CpuReference.Pow(a, b),
            _ => throw new ArgumentException(op)
        };

        SyncValues(result).ShouldBeCloseTo(expected);
    }

    [Theory]
    [InlineData(2f)]
    [InlineData(-3f)]
    public void VectorScalarOperators_MatchCpuReference(float scalar)
    {
        var vector = CreateVector([2f, 4f, 6f]);

        SyncValues(vector + scalar).ShouldBeCloseTo([2f + scalar, 4f + scalar, 6f + scalar]);
        SyncValues(vector * scalar).ShouldBeCloseTo(CpuReference.Scale([2f, 4f, 6f], scalar));
        SyncValues(scalar - vector).ShouldBeCloseTo([scalar - 2f, scalar - 4f, scalar - 6f]);
        SyncValues(scalar / vector).ShouldBeCloseTo([scalar / 2f, scalar / 4f, scalar / 6f]);
    }

    public static IEnumerable<object[]> BinaryOpData()
    {
        yield return [new float[] { 1, 2, 3 }, new float[] { 4, 5, 6 }, "add"];
        yield return [new float[] { 10, 20, 30 }, new float[] { 1, 2, 3 }, "sub"];
        yield return [new float[] { 2, 3, 4 }, new float[] { 5, 6, 7 }, "mul"];
        yield return [new float[] { 10, 20, 30 }, new float[] { 2, 4, 5 }, "div"];
        yield return [new float[] { 2, 3, 4 }, new float[] { 2, 2, 2 }, "pow"];
    }
}
