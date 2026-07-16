namespace BAVCL.Tests.Helpers;

/// <summary>
/// Maps NumPy logical shapes to BAVCL Vector storage (row-major, Columns = last axis).
/// </summary>
public static class BavclShape
{
    public static Vector Create(GPU gpu, int[] numpyShape, float[] data, bool cache = true)
    {
        if (numpyShape.Length == 1)
            return new Vector(gpu, data, columns: 1, cache);

        int cols = numpyShape[^1];
        return new Vector(gpu, data, columns: cols, cache);
    }

  public static int[] ToNumpyShape(Vector vector)
    {
        if (vector.Columns == 1)
            return [vector.Length];

        if (vector.RowCount() == 1)
            return [1, vector.Columns];

        return [vector.RowCount(), vector.Columns];
    }

    public static int[] BroadcastOutputShape(int[] shapeA, int[] shapeB) =>
        BroadcastReference.BroadcastShapes(shapeA, shapeB);

    public static int BavclColumns(int[] numpyOutShape) =>
        numpyOutShape.Length == 1 ? 1 : numpyOutShape[^1];

    public static void ShouldMatchNumpyShape(Vector vector, int[] expectedNumpyShape, float[] expectedData)
    {
        var actual = vector;
        actual.SyncCPU();

        int expectedLength = expectedNumpyShape.Aggregate(1, (a, b) => a * b);
        actual.Length.Should().Be(expectedLength);
        actual.Columns.Should().Be(BavclColumns(expectedNumpyShape));
        actual.Value.ShouldBeCloseTo(expectedData);
    }
}
