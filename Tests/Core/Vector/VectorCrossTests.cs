using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorTests;

/// <summary>
/// Vector.Cross is matrix multiply (not geometric cross product — see Vector3.Cross).
/// </summary>
public class VectorCrossTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Theory]
    [InlineData(2, 3, 2)]
    [InlineData(3, 2, 4)]
    [InlineData(2, 2, 2)]
    public void Cross_2D_MatMul_MatchesReference(int m, int k, int n)
    {
        var aData = BroadcastReference.SequentialData(m, k, 1f, 1f);
        var bData = BroadcastReference.SequentialData(k, n, 2f, 0.5f);
        var a = BavclShape.Create(Gpu, [m, k], aData);
        var b = BavclShape.Create(Gpu, [k, n], bData);
        var expected = BroadcastReference.MatMul(aData, m, k, bData, k, n);

        var result = Vector.Cross(a, b);

        BavclShape.ShouldMatchNumpyShape(result, [m, n], expected);
    }

    [Theory]
    [InlineData(2, 3)]
    [InlineData(3, 4)]
    public void Cross_MatrixTimes1D_MatchesReference(int m, int k)
    {
        var aData = BroadcastReference.SequentialData(m, k, 1f, 1f);
        var bData = BroadcastReference.SequentialData(k, 1f, 1f);
        var a = BavclShape.Create(Gpu, [m, k], aData);
        var b = BavclShape.Create(Gpu, [k], bData);
        var expected = BroadcastReference.MatMulVector(aData, m, k, bData);

        var result = Vector.Cross(a, b);

        result.Length.Should().Be(m);
        result.Columns.Should().Be(1);
        SyncValues(result).ShouldBeCloseTo(expected);
    }

    [Theory]
    [InlineData(2, 3)]
    [InlineData(4, 2)]
    public void Cross_1DTimesMatrix_MatchesReference(int k, int n)
    {
        // 1×K row vector times K×N matrix → 1×N (stored as length N, columns N).
        var aData = BroadcastReference.SequentialData(1, k, 1f, 1f);
        var bData = BroadcastReference.SequentialData(k, n, 2f, 0.5f);
        var a = BavclShape.Create(Gpu, [1, k], aData);
        var b = BavclShape.Create(Gpu, [k, n], bData);
        var expected = BroadcastReference.MatMul(aData, 1, k, bData, k, n);

        var result = Vector.Cross(a, b);

        BavclShape.ShouldMatchNumpyShape(result, [1, n], expected);
    }

    [Fact]
    public void Cross_1DLengthN_TimesMxN_MatchesMatrixVectorProduct()
    {
        var matrixData = BroadcastReference.SequentialData(2, 3, 10f, 1f);
        var vecData = BroadcastReference.SequentialData(3, 1f, 1f);
        var matrix = BavclShape.Create(Gpu, [2, 3], matrixData);
        var vector = BavclShape.Create(Gpu, [3], vecData);
        var expected = BroadcastReference.MatMulVector(matrixData, 2, 3, vecData);

        var result = Vector.Cross(matrix, vector);

        result.Length.Should().Be(2);
        SyncValues(result).ShouldBeCloseTo(expected);
    }

    [Fact]
    public void Cross_IncompatibleInnerDimensions_Throws()
    {
        var a = BavclShape.Create(Gpu, [2, 3], BroadcastReference.SequentialData(2, 3));
        var b = BavclShape.Create(Gpu, [2, 2], BroadcastReference.SequentialData(2, 2));

        Action act = () => _ = Vector.Cross(a, b);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*inner dimensions*");
    }

    [Fact]
    public void Cross_EqualShape2D_IsMatMul_NotElementWise()
    {
        var aData = new float[] { 1, 2, 3, 4 };
        var bData = new float[] { 5, 6, 7, 8 };
        var a = BavclShape.Create(Gpu, [2, 2], aData);
        var b = BavclShape.Create(Gpu, [2, 2], bData);

        var result = Vector.Cross(a, b);
        var expected = BroadcastReference.MatMul(aData, 2, 2, bData, 2, 2);

        BavclShape.ShouldMatchNumpyShape(result, [2, 2], expected);
        SyncValues(a * b).Should().NotBeEquivalentTo(expected);
    }
}
