namespace BAVCL.Tests.Helpers;

/// <summary>
/// Maps NumPy logical shapes to BAVCL Vector storage (row-major).
/// Columns: 0 = row 1D [n], 1 = column [m,1], N = matrix last axis.
/// </summary>
public static class BavclShape
{
    public static Vector Create(GPU gpu, int[] numpyShape, float[] data, bool cache = true)
    {
        int columns = NumpyColumns(numpyShape);
        return new Vector(gpu, data, columns, cache);
    }

    public static int NumpyColumns(int[] numpyShape) =>
        numpyShape.Length switch
        {
            1 => 0,
            _ when numpyShape[^1] == 1 => 1,
            _ => numpyShape[^1]
        };

    public static int[] ToNumpyShape(Vector vector)
    {
        if (vector.Columns == 0)
            return [vector.Length];

        if (vector.Columns == 1)
            return [vector.Length, 1];

        (int rows, int cols) = vector.Shape();
        if (rows == 1)
            return [1, cols];

        return [rows, cols];
    }

    public static int[] BroadcastOutputShape(int[] shapeA, int[] shapeB) =>
        BroadcastReference.BroadcastShapes(shapeA, shapeB);

    public static int BavclColumns(int[] numpyOutShape) => NumpyColumns(numpyOutShape);

    public static void ShouldMatchNumpyShape(Vector vector, int[] expectedNumpyShape, float[] expectedData)
    {
        vector.SyncCPU();

        int expectedLength = expectedNumpyShape.Aggregate(1, (a, b) => a * b);
        vector.Length.Should().Be(expectedLength);
        vector.Columns.Should().Be(BavclColumns(expectedNumpyShape));
        vector.Value.ShouldBeCloseTo(expectedData);
    }
}
