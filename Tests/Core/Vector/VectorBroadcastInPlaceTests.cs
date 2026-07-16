using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorTests;

public class VectorBroadcastInPlaceTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    // KNOWN: IPOP broadcast path is unimplemented (empty stub in Vector.IPOP).

    [Theory]
    [MemberData(nameof(IpopBroadcastCases))]
    public void IPOP_Broadcast_MatchesNumPyReference(
        int[] leftShape, float[] leftData, int[] rightShape, float[] rightData, Operations op)
    {
        var left = BavclShape.Create(Gpu, leftShape, leftData);
        var right = BavclShape.Create(Gpu, rightShape, rightData);
        var outShape = BavclShape.BroadcastOutputShape(leftShape, rightShape);

        var expected = op switch
        {
            Operations.add => BroadcastReference.Add(leftData, leftShape, rightData, rightShape),
            Operations.subtract => BroadcastReference.Subtract(leftData, leftShape, rightData, rightShape),
            Operations.multiply => BroadcastReference.Multiply(leftData, leftShape, rightData, rightShape),
            Operations.divide => BroadcastReference.Divide(leftData, leftShape, rightData, rightShape),
            Operations.pow => BroadcastReference.Pow(leftData, leftShape, rightData, rightShape),
            _ => throw new ArgumentOutOfRangeException(nameof(op))
        };

        left.IPOP(right, op);
        left.SyncCPU();

        left.Length.Should().Be(outShape.Aggregate(1, (a, b) => a * b));
        left.Columns.Should().Be(BavclShape.BavclColumns(outShape));
        left.Value.ShouldBeCloseTo(expected);
    }

    [Fact]
    public void IPOP_ScalarOnMatrix_MatchesNumPy()
    {
        var shape = new[] { 2, 3 };
        var data = BroadcastReference.SequentialData(2, 3, 2f, 1f);
        var matrix = BavclShape.Create(Gpu, shape, data);

        matrix.IPOP(3f, Operations.multiply);
        matrix.SyncCPU();

        matrix.Value.ShouldBeCloseTo(BroadcastReference.Scale(data, shape, 3f));
    }

    [Fact]
    public void IPOP_RowBroadcastOnMatrix_MatchesNumPy()
    {
        var shapeM = new[] { 2, 3 };
        var shapeR = new[] { 1, 3 };
        var matrixData = BroadcastReference.SequentialData(2, 3, 10f, 1f);
        var rowData = BroadcastReference.SequentialData(1, 3, 1f, 1f);

        var matrix = BavclShape.Create(Gpu, shapeM, matrixData);
        var row = BavclShape.Create(Gpu, shapeR, rowData);

        matrix.IPOP(row, Operations.add);
        matrix.SyncCPU();

        matrix.Value.ShouldBeCloseTo(BroadcastReference.Add(matrixData, shapeM, rowData, shapeR));
    }

    public static IEnumerable<object[]> IpopBroadcastCases()
    {
        foreach (var m in new[] { 2, 3 })
        foreach (var n in new[] { 2, 3 })
        {
            yield return
            [
                new[] { m, n }, BroadcastReference.SequentialData(m, n, 10f, 1f),
                new[] { 1, n }, BroadcastReference.SequentialData(1, n, 1f, 1f),
                Operations.add
            ];

            yield return
            [
                new[] { m, n }, BroadcastReference.SequentialData(m, n, 10f, 1f),
                new[] { n }, BroadcastReference.SequentialData(n, 1f, 1f),
                Operations.multiply
            ];
        }
    }
}
