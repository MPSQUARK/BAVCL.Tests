namespace BAVCL.Tests.Helpers;

/// <summary>
/// Maps NumPy logical shapes to BAVCL VectorInt storage (row-major).
/// </summary>
public static class BavclShapeInt
{
    public static VectorInt Create(GPU gpu, int[] numpyShape, int[] data, bool cache = true)
    {
        int columns = BavclShape.NumpyColumns(numpyShape);
        return new VectorInt(gpu, data, columns, cache);
    }

    public static int[] ExpectedBinary(
        int[] a, int[] shapeA,
        int[] b, int[] shapeB,
        Func<float, float, float> op) =>
        BroadcastReference
            .Binary(
                a.Select(x => (float)x).ToArray(), shapeA,
                b.Select(x => (float)x).ToArray(), shapeB,
                op)
            .Select(x => (int)x)
            .ToArray();

    public static void ShouldMatchNumpyShape(VectorInt vector, int[] expectedNumpyShape, int[] expectedData)
    {
        vector.SyncCPU();

        int expectedLength = expectedNumpyShape.Aggregate(1, (x, y) => x * y);
        vector.Length.Should().Be(expectedLength);
        vector.Columns.Should().Be(BavclShape.BavclColumns(expectedNumpyShape));
        vector.Value.Should().Equal(expectedData);
    }
}
