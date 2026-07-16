using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorTests;

public class VectorBroadcastTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    // KNOWN: vectormatrixOpKernel performs row-wise reduction, not NumPy broadcast.
    // These tests assert spec-correct NumPy semantics.

    [Theory]
    [MemberData(nameof(BroadcastTestData.CompatibleBroadcastOps), MemberType = typeof(BroadcastTestData))]
    public void Operator_Broadcast_MatchesNumPyReference(
        int[] shapeA, float[] dataA, int[] shapeB, float[] dataB, string op)
    {
        var a = BavclShape.Create(Gpu, shapeA, dataA);
        var b = BavclShape.Create(Gpu, shapeB, dataB);
        var outShape = BavclShape.BroadcastOutputShape(shapeA, shapeB);
        var expected = BroadcastTestData.ExpectedBroadcast(op, dataA, shapeA, dataB, shapeB);

        var result = BroadcastTestData.ApplyOp(a, b, op);

        BavclShape.ShouldMatchNumpyShape(result, outShape, expected);
    }

    [Theory]
    [MemberData(nameof(BroadcastTestData.CompatibleBroadcastPairs), MemberType = typeof(BroadcastTestData))]
    public void OP_Static_Broadcast_MatchesNumPyReference(
        int[] shapeA, float[] dataA, int[] shapeB, float[] dataB)
    {
        var a = BavclShape.Create(Gpu, shapeA, dataA);
        var b = BavclShape.Create(Gpu, shapeB, dataB);
        var outShape = BavclShape.BroadcastOutputShape(shapeA, shapeB);
        var expected = BroadcastReference.Add(dataA, shapeA, dataB, shapeB);

        var result = Vector.OP(a, b, Operations.add);

        BavclShape.ShouldMatchNumpyShape(result, outShape, expected);
    }

    [Theory]
    [MemberData(nameof(BroadcastTestData.CompatibleBroadcastPairs), MemberType = typeof(BroadcastTestData))]
    public void Operator_ReverseOperand_Broadcast_MatchesNumPyReference(
        int[] shapeA, float[] dataA, int[] shapeB, float[] dataB)
    {
        var a = BavclShape.Create(Gpu, shapeA, dataA);
        var b = BavclShape.Create(Gpu, shapeB, dataB);
        var outShape = BavclShape.BroadcastOutputShape(shapeB, shapeA);
        var expected = BroadcastReference.Subtract(dataB, shapeB, dataA, shapeA);

        var result = b - a;

        BavclShape.ShouldMatchNumpyShape(result, outShape, expected);
    }

    [Fact]
    public void Broadcast_RowVectorPlusMatrix_MatchesNumPy()
    {
        var shapeM = new[] { 2, 3 };
        var shapeR = new[] { 1, 3 };
        var matrixData = BroadcastReference.SequentialData(2, 3, 10f, 1f);
        var rowData = BroadcastReference.SequentialData(1, 3, 1f, 1f);

        var matrix = BavclShape.Create(Gpu, shapeM, matrixData);
        var row = BavclShape.Create(Gpu, shapeR, rowData);
        var expected = BroadcastReference.Add(matrixData, shapeM, rowData, shapeR);

        BavclShape.ShouldMatchNumpyShape(matrix + row, shapeM, expected);
        BavclShape.ShouldMatchNumpyShape(row + matrix, shapeM, expected);
    }

    [Fact]
    public void Broadcast_1DLengthNPlusMxN_MatchesNumPy()
    {
        var shapeM = new[] { 2, 3 };
        var shapeV = new[] { 3 };
        var matrixData = BroadcastReference.SequentialData(2, 3, 10f, 1f);
        var vecData = BroadcastReference.SequentialData(3, 1f, 1f);

        var matrix = BavclShape.Create(Gpu, shapeM, matrixData);
        var vec = BavclShape.Create(Gpu, shapeV, vecData);
        var expected = BroadcastReference.Add(matrixData, shapeM, vecData, shapeV);

        BavclShape.ShouldMatchNumpyShape(matrix + vec, shapeM, expected);
    }

    [Fact]
    public void Broadcast_ColumnProxyPlusMatrix_MatchesNumPy()
    {
        // (M,1) stored as 1D length M — NumPy broadcast shape metadata is (M,1).
        var shapeM = new[] { 2, 3 };
        var shapeC = new[] { 2, 1 };
        var matrixData = BroadcastReference.SequentialData(2, 3, 10f, 1f);
        var colData = BroadcastReference.SequentialData(2, 1, 1f, 1f);

        var matrix = BavclShape.Create(Gpu, shapeM, matrixData);
        var col = BavclShape.Create(Gpu, shapeC, colData);
        var expected = BroadcastReference.Add(matrixData, shapeM, colData, shapeC);

        BavclShape.ShouldMatchNumpyShape(matrix + col, shapeM, expected);
    }

    [Fact]
    public void EqualShape2D_ElementWise_MatchesNumPy()
    {
        var shape = new[] { 2, 3 };
        var aData = BroadcastReference.SequentialData(2, 3, 1f, 1f);
        var bData = BroadcastReference.SequentialData(2, 3, 10f, 1f);

        var a = BavclShape.Create(Gpu, shape, aData);
        var b = BavclShape.Create(Gpu, shape, bData);

        BavclShape.ShouldMatchNumpyShape(a * b, shape, BroadcastReference.Multiply(aData, shape, bData, shape));
    }
}
