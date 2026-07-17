namespace BAVCL.Tests.Helpers;

/// <summary>
/// Maps NumPy logical shapes to BAVCL Vector storage (row-major, Columns = last axis).
/// </summary>
public static class BavclShape
{
    public static Vector Create(GPU gpu, int[] numpyShape, float[] data, bool cache = true)
    {
        int columns = numpyShape.Length == 1 ? 1 : numpyShape[^1];
        return new Vector(gpu, data, columns, cache);
    }

    public static int[] ToNumpyShape(Vector vector)
    {
        (int rows, int cols) = vector.Shape();

        if (cols == 1)
            return [rows];

        if (rows == 1)
            return [1, cols];

        return [rows, cols];
    }

    public static int[] BroadcastOutputShape(int[] shapeA, int[] shapeB) =>
        BroadcastReference.BroadcastShapes(shapeA, shapeB);

    public static int BavclColumns(int[] numpyOutShape) =>
        numpyOutShape.Length == 1 ? 1 : numpyOutShape[^1];

    public static void ShouldMatchNumpyShape(Vector vector, int[] expectedNumpyShape, float[] expectedData)
    {
        vector.SyncCPU();

        int expectedLength = expectedNumpyShape.Aggregate(1, (a, b) => a * b);
        vector.Length.Should().Be(expectedLength);
        vector.Columns.Should().Be(BavclColumns(expectedNumpyShape));
        vector.Value.ShouldBeCloseTo(expectedData);
    }
}
