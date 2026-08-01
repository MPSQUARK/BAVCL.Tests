using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorIntTests;

public class VectorIntBroadcastTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void Broadcast_RowVectorPlusMatrix_MatchesReference()
    {
        var shapeM = new[] { 2, 3 };
        var shapeR = new[] { 1, 3 };
        var matrixData = BroadcastReference.SequentialData(2, 3, 10, 1).Select(x => (int)x).ToArray();
        var rowData = BroadcastReference.SequentialData(1, 3, 1, 1).Select(x => (int)x).ToArray();

        var matrix = BavclShapeInt.Create(Gpu, shapeM, matrixData);
        var row = BavclShapeInt.Create(Gpu, shapeR, rowData);
        var expected = BavclShapeInt.ExpectedBinary(matrixData, shapeM, rowData, shapeR, (x, y) => x + y);

        BavclShapeInt.ShouldMatchNumpyShape(matrix + row, shapeM, expected);
        BavclShapeInt.ShouldMatchNumpyShape(row + matrix, shapeM, expected);
    }

    [Fact]
    public void Broadcast_ColumnPlusMatrix_MatchesReference()
    {
        var shapeM = new[] { 2, 3 };
        var shapeC = new[] { 2, 1 };
        var matrixData = BroadcastReference.SequentialData(2, 3, 10, 1).Select(x => (int)x).ToArray();
        var colData = BroadcastReference.SequentialData(2, 1, 1, 1).Select(x => (int)x).ToArray();

        var matrix = BavclShapeInt.Create(Gpu, shapeM, matrixData);
        var col = BavclShapeInt.Create(Gpu, shapeC, colData);
        var expected = BavclShapeInt.ExpectedBinary(matrixData, shapeM, colData, shapeC, (x, y) => x + y);

        BavclShapeInt.ShouldMatchNumpyShape(matrix + col, shapeM, expected);
    }

    [Fact]
    public void EqualShape2D_ElementWise_MatchesReference()
    {
        var shape = new[] { 2, 3 };
        var aData = BroadcastReference.SequentialData(2, 3, 5, 1).Select(x => (int)x).ToArray();
        var bData = BroadcastReference.SequentialData(2, 3, 1, 1).Select(x => (int)x).ToArray();

        var a = BavclShapeInt.Create(Gpu, shape, aData);
        var b = BavclShapeInt.Create(Gpu, shape, bData);
        var expected = BavclShapeInt.ExpectedBinary(aData, shape, bData, shape, (x, y) => x + y);

        BavclShapeInt.ShouldMatchNumpyShape(a + b, shape, expected);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(-3)]
    public void VectorScalarOperators_MatchReference(int scalar)
    {
        var vector = CreateVectorInt([2, 4, 6]);

        SyncValues(vector + scalar).Should().Equal([2 + scalar, 4 + scalar, 6 + scalar]);
        SyncValues(vector * scalar).Should().Equal([2 * scalar, 4 * scalar, 6 * scalar]);
        SyncValues(scalar - vector).Should().Equal([scalar - 2, scalar - 4, scalar - 6]);
        SyncValues(scalar / vector).Should().Equal([scalar / 2, scalar / 4, scalar / 6]);
    }

    [Fact]
    public void Broadcast_1DPlusMatrix_Requires2DStorage()
    {
        var matrix = BavclShapeInt.Create(Gpu, [2, 3], [10, 11, 12, 13, 14, 15]);
        var vec = BavclShapeInt.Create(Gpu, [3], [1, 1, 1]);

        Action act = () => _ = matrix + vec;

        act.Should().Throw<ArgumentException>()
            .WithMessage("*2D storage layout*");
    }
}
