using BAVCL.Modules.Sorting;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.Sorting;

[Collection("GpuSerial")]
public class ArgsortTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    static readonly Random Rng = new(99);

    [Fact]
    public void ArgsortAscending_1D_DoesNotMutateInput_AndIndicesProduceSortedOrder()
    {
        int[] data = RandomIntData(128);
        var vector = CreateVectorInt(data);
        int[] before = SyncValues(vector);

        VectorInt indices = vector.ArgsortAscending();

        SyncValues(vector).Should().Equal(before);
        AssertArgsortProducesSortedOrder(before, SyncValues(indices), columns: 0, SortOrder.Ascending);
    }

    [Fact]
    public void ArgsortDescending_2D_PerRowIndices()
    {
        int rows = 3;
        int cols = 6;
        int[] data = RandomIntData(rows * cols);
        var vector = CreateVectorInt(data, columns: cols);

        VectorInt indices = vector.ArgsortDescending();

        AssertArgsortProducesSortedOrder(data, SyncValues(indices), cols, SortOrder.Descending);
    }

    [Fact]
    public void Argsort_Empty_ReturnsEmpty()
    {
        var vector = CreateVectorInt([], cache: false);

        VectorInt indices = vector.ArgsortAscending();

        indices.Length.Should().Be(0);
    }

    [Fact]
    public void Argsort_LengthOne_ReturnsZero()
    {
        var vector = CreateVectorInt([42]);

        VectorInt indices = vector.ArgsortAscending();

        SyncValues(indices).Should().Equal([0]);
    }

    [Fact]
    public void Argsort_1DAndSingleRowMatrix_ProduceSameIndices()
    {
        int[] data = RandomIntData(32);
        var as1D = CreateVectorInt(data);
        var asMatrix = CreateVectorInt(data, columns: data.Length);

        int[] idx1D = SyncValues(as1D.ArgsortAscending());
        int[] idxMatrix = SyncValues(asMatrix.ArgsortAscending());

        idx1D.Should().Equal(idxMatrix);
    }

    [Fact]
    public void ArgsortAscendingX_MatchesCpuArgsort_Int()
    {
        int[] data = RandomIntData(256);
        var cpu = CreateVectorInt(data);
        var gpu = CreateVectorInt(data);

        int[] expected = SyncValues(cpu.ArgsortAscending());
        int[] actual = SyncValues(gpu.ArgsortAscendingX());

        AssertArgsortProducesSortedOrder(data, actual, columns: 0, SortOrder.Ascending);
        AssertArgsortProducesSortedOrder(data, expected, columns: 0, SortOrder.Ascending);
    }

    [Fact]
    public void ArgsortAscendingX_DoesNotMutateInput()
    {
        float[] data = RandomFloatData(64);
        var vector = CreateVector(data);
        float[] before = SyncValues(vector);

        _ = vector.ArgsortAscendingX();

        SyncValues(vector).ShouldBeCloseTo(before);
    }

    [Fact]
    public void ArgsortDescendingX_MatchesCpuArgsort_Float2D()
    {
        int rows = 2;
        int cols = 20;
        float[] data = RandomFloatData(rows * cols);
        var cpu = CreateVector(data, columns: cols);
        var gpu = CreateVector(data, columns: cols);

        int[] expected = SyncValues(cpu.ArgsortDescending());
        int[] actual = SyncValues(gpu.ArgsortDescendingX());

        AssertArgsortProducesSortedOrder(data, actual, cols, SortOrder.Descending);
        AssertArgsortProducesSortedOrder(data, expected, cols, SortOrder.Descending);
    }

    static int[] RandomIntData(int length)
    {
        var data = new int[length];
        for (int i = 0; i < length; i++)
            data[i] = Rng.Next(-5000, 5000);
        return data;
    }

    static float[] RandomFloatData(int length)
    {
        var data = new float[length];
        for (int i = 0; i < length; i++)
            data[i] = (float)(Rng.NextDouble() * 100.0);
        return data;
    }

    static void AssertArgsortProducesSortedOrder(int[] values, int[] indices, int columns, SortOrder order)
    {
        int segmentLength = columns == 0 ? values.Length : columns;
        int rowCount = columns == 0 ? 1 : values.Length / columns;

        for (int row = 0; row < rowCount; row++)
        {
            int offset = row * segmentLength;
            for (int i = 0; i < segmentLength - 1; i++)
            {
                int idxA = indices[offset + i];
                int idxB = indices[offset + i + 1];
                int cmp = values[offset + idxA].CompareTo(values[offset + idxB]);
                if (order == SortOrder.Descending)
                    cmp.Should().BeGreaterThanOrEqualTo(0);
                else
                    cmp.Should().BeLessThanOrEqualTo(0);
            }
        }
    }

    static void AssertArgsortProducesSortedOrder(float[] values, int[] indices, int columns, SortOrder order)
    {
        int segmentLength = columns == 0 ? values.Length : columns;
        int rowCount = columns == 0 ? 1 : values.Length / columns;

        for (int row = 0; row < rowCount; row++)
        {
            int offset = row * segmentLength;
            for (int i = 0; i < segmentLength - 1; i++)
            {
                int idxA = indices[offset + i];
                int idxB = indices[offset + i + 1];
                int cmp = values[offset + idxA].CompareTo(values[offset + idxB]);
                if (order == SortOrder.Descending)
                    cmp.Should().BeGreaterThanOrEqualTo(0);
                else
                    cmp.Should().BeLessThanOrEqualTo(0);
            }
        }
    }
}
