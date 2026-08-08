using BAVCL.Core.Exceptions;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorTests;

/// <summary>
/// Vector.Dot is always a scalar inner product: sum(element-wise multiply).
/// Matrix multiplication is a separate operation (not Dot).
/// </summary>
public class VectorDotProductTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Theory]
    [InlineData(new float[] { 1, 2, 3 }, new float[] { 4, 5, 6 }, 32f)]
    [InlineData(new float[] { 2, 0, 1 }, new float[] { 3, 4, 5 }, 11f)]
    [InlineData(new float[] { 1, -1, 2 }, new float[] { 3, 3, 3 }, 6f)]
    public void Dot_1D_ReturnsScalarInnerProduct(float[] a, float[] b, float expected)
    {
        var vectorA = CreateVector(a);
        var vectorB = CreateVector(b);

        vectorA.Dot(vectorB).ShouldBeCloseTo(expected);
        vectorA.Dot(vectorB).ShouldBeCloseTo(expected);
        expected.ShouldBeCloseTo(BroadcastReference.InnerProduct(a, b));
    }

    [Theory]
    [InlineData(2f)]
    [InlineData(-3f)]
    public void Dot_Scalar_ReturnsScaledSum(float scalar)
    {
        var data = new float[] { 1f, 2f, 3f };
        var vector = CreateVector(data);
        float expected = data.Sum() * scalar;

        vector.Dot(scalar).ShouldBeCloseTo(expected);
        vector.Dot(scalar).ShouldBeCloseTo(expected);
    }

    [Fact]
    public void Dot_EqualLength2D_ReturnsScalarFrobeniusInnerProduct()
    {
        // Same-length operands: Dot flattens and sums element-wise products → one scalar.
        var aData = BroadcastReference.SequentialData(2, 3, 1f, 1f);
        var bData = BroadcastReference.SequentialData(2, 3, 2f, 0.5f);
        var a = BavclShape.Create(Gpu, [2, 3], aData);
        var b = BavclShape.Create(Gpu, [2, 3], bData);

        float result = a.Dot(b);

        result.ShouldBeCloseTo(BroadcastReference.InnerProduct(aData, bData));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    public void Dot_RowWithColumn_ReturnsScalarInnerProduct(int row, int col)
    {
        var aData = BroadcastReference.SequentialData(2, 3, 1f, 1f);
        var bData = BroadcastReference.SequentialData(3, 2, 1f, 1f);
        var a = BavclShape.Create(Gpu, [2, 3], aData);
        var b = BavclShape.Create(Gpu, [3, 2], bData);

        var rowVec = a.GetRowAsVector(row);
        var colVec = b.GetColumnAsVectorX(col);
        float[] rowData = a.GetRowAsArray(row);
        float[] colData = b.GetColumnAsArray(col);

        rowVec.Dot(colVec).ShouldBeCloseTo(BroadcastReference.InnerProduct(rowData, colData));
    }

    [Fact]
    public void Dot_UnequalLength_SpecRequiresEqualLength()
    {
        // SPEC: Dot product requires equal-length operands.
        var a = new float[] { 1f, 2f, 3f };
        var b = new float[] { 1f, 2f };

        Action reference = () => _ = BroadcastReference.InnerProduct(a, b);
        reference.Should().Throw<ArgumentException>();

        var vectorA = CreateVector(a);
        var vectorB = CreateVector(b);
        Action bavcl = () => _ = vectorA.Dot(vectorB);
        bavcl.Should().Throw<LengthMismatchException>();
    }
}
