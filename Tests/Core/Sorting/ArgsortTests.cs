using BAVCL.Modules.Sorting;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.Sorting;

[Collection("GpuSerial")]
public class ArgsortTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    static readonly Random Rng = new(99);

    [Fact]
    public void ArgsortAsc_1D_DoesNotMutateInput_AndIndicesProduceSortedOrder()
    {
        int[] data = RandomIntData(128);
        var vector = CreateVectorInt(data);
        int[] before = SyncValues(vector);

        VectorInt indices = vector.ArgsortAsc();

        SyncValues(vector).Should().Equal(before);
        AssertArgsortProducesSortedOrder(before, SyncValues(indices), columns: 0, SortOrder.Ascending);
    }

    [Fact]
    public void ArgsortDesc_2D_PerRowIndices()
    {
        int rows = 3;
        int cols = 6;
        int[] data = RandomIntData(rows * cols);
        var vector = CreateVectorInt(data, columns: cols);

        VectorInt indices = vector.ArgsortDesc();

        AssertArgsortProducesSortedOrder(data, SyncValues(indices), cols, SortOrder.Descending);
    }

    [Fact]
    public void Argsort_Empty_ReturnsEmpty()
    {
        var vector = CreateVectorInt([], cache: false);

        VectorInt indices = vector.ArgsortAsc();

        indices.Length.Should().Be(0);
    }

    [Fact]
    public void Argsort_LengthOne_ReturnsZero()
    {
        var vector = CreateVectorInt([42]);

        VectorInt indices = vector.ArgsortAsc();

        SyncValues(indices).Should().Equal([0]);
    }

    [Fact]
    public void ArgsortAscX_MatchesCpuArgsort_Int()
    {
        int[] data = RandomIntData(256);
        var cpu = CreateVectorInt(data);
        var gpu = CreateVectorInt(data);

        int[] expected = SyncValues(cpu.ArgsortAsc());
        int[] actual = SyncValues(gpu.ArgsortAscX());

        AssertArgsortProducesSortedOrder(data, actual, columns: 0, SortOrder.Ascending);
        AssertArgsortProducesSortedOrder(data, expected, columns: 0, SortOrder.Ascending);
    }

    [Fact]
    public void ArgsortAscX_DoesNotMutateInput()
    {
        float[] data = RandomFloatData(64);
        var vector = CreateVector(data);
        float[] before = SyncValues(vector);

        _ = vector.ArgsortAscX();

        SyncValues(vector).ShouldBeCloseTo(before);
    }

    [Fact]
    public void ArgsortDescX_MatchesCpuArgsort_Float2D()
    {
        int rows = 2;
        int cols = 20;
        float[] data = RandomFloatData(rows * cols);
        var cpu = CreateVector(data, columns: cols);
        var gpu = CreateVector(data, columns: cols);

        int[] expected = SyncValues(cpu.ArgsortDesc());
        int[] actual = SyncValues(gpu.ArgsortDescX());

        AssertArgsortProducesSortedOrder(data, actual, cols, SortOrder.Descending);
        AssertArgsortProducesSortedOrder(data, expected, cols, SortOrder.Descending);
    }

    [Fact]
    public void ArgsortAscIP_Cpu_WritesIntoCallerBuffer_AndDoesNotMutateInput()
    {
        int[] data = RandomIntData(128);
        var vector = CreateVectorInt(data);
        var indices = new VectorInt(vector.Gpu, vector.Length, vector.Columns);
        int[] before = SyncValues(vector);

        vector.ArgsortAscIP(indices);

        SyncValues(vector).Should().Equal(before);
        AssertArgsortProducesSortedOrder(before, SyncValues(indices), columns: 0, SortOrder.Ascending);
    }

    [Fact]
    public void ArgsortAscIP_ShapeMismatch_Throws()
    {
        int[] data = RandomIntData(128);
        var vector = CreateVectorInt(data);
        var indices = new VectorInt(vector.Gpu, vector.Length - 1, 0);

        Action act = () => vector.ArgsortAscIP(indices);

        act.Should().Throw<BAVCL.Core.Exceptions.ShapeMismatchException>();
    }

    [Fact]
    public void ArgsortAscXIP_Gpu_WritesIntoCallerBuffer_AndDoesNotMutateInput()
    {
        int[] data = RandomIntData(256);
        var vector = CreateVectorInt(data);
        var indices = new VectorInt(vector.Gpu, vector.Length, vector.Columns);
        int[] before = SyncValues(vector);

        vector.ArgsortAscXIP(indices);

        SyncValues(vector).Should().Equal(before);
        AssertArgsortProducesSortedOrder(before, SyncValues(indices), columns: 0, SortOrder.Ascending);
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
