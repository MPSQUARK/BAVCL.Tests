using BAVCL.Modules.Sorting;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.Sorting;

[Collection("GpuSerial")]
public class VectorSortTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    static readonly Random Rng = new(17);

    [Fact]
    public void SortAscIP_1D_MatchesArraySort()
    {
        float[] data = RandomData(256);
        var vector = CreateVector(data);
        var expected = (float[])data.Clone();
        Array.Sort(expected);

        vector.SortAscIP();
        SyncValues(vector).ShouldBeCloseTo(expected);
    }

    [Fact]
    public void SortDescIP_2D_SortsEachRowIndependently()
    {
        int rows = 3;
        int cols = 10;
        float[] data = RandomData(rows * cols);
        var vector = CreateVector(data, columns: cols);
        var expected = (float[])data.Clone();
        SortRowsReference(expected, rows, cols, SortOrder.Descending);

        vector.SortDescIP();
        SyncValues(vector).ShouldBeCloseTo(expected);
    }

    [Fact]
    public void SortIP_ColumnVector_MatchesFlat1D()
    {
        float[] data = RandomData(96);
        var flat = CreateVector(data);
        var column = CreateVector(data, columns: 1);

        flat.SortAscIP();
        column.SortAscIP();

        SyncValues(flat).ShouldBeCloseTo(SyncValues(column));
    }

    [Fact]
    public void SortAscXIP_MatchesCpuSort_1D()
    {
        float[] data = RandomData(512);
        var cpu = CreateVector(data);
        var gpu = CreateVector(data);

        cpu.SortAscIP();
        gpu.SortAscXIP();

        SyncValues(gpu).ShouldBeCloseTo(SyncValues(cpu));
    }

    [Fact]
    public void SortDescXIP_MatchesCpuSort_2D()
    {
        int rows = 4;
        int cols = 12;
        float[] data = RandomData(rows * cols);
        var cpu = CreateVector(data, columns: cols);
        var gpu = CreateVector(data, columns: cols);

        cpu.SortDescIP();
        gpu.SortDescXIP();

        SyncValues(gpu).ShouldBeCloseTo(SyncValues(cpu));
    }

    [Fact]
    public void SortAsc_AllocatingCpu_DoesNotMutateInput_AndReturnsSortedCopy()
    {
        float[] data = RandomData(256);
        var vector = CreateVector(data);
        var expected = (float[])data.Clone();
        Array.Sort(expected);

        Vector result = vector.SortAsc();

        SyncValues(vector).ShouldBeCloseTo(data);
        SyncValues(result).ShouldBeCloseTo(expected);
    }

    [Fact]
    public void SortAscX_AllocatingGpu_DoesNotMutateInput_AndReturnsSortedCopy()
    {
        float[] data = RandomData(256);
        var vector = CreateVector(data);
        var expected = (float[])data.Clone();
        Array.Sort(expected);

        Vector result = vector.SortAscX();

        SyncValues(vector).ShouldBeCloseTo(data);
        SyncValues(result).ShouldBeCloseTo(expected);
    }

    static float[] RandomData(int length)
    {
        var data = new float[length];
        for (int i = 0; i < length; i++)
            data[i] = (float)(Rng.NextDouble() * 2000.0 - 1000.0);
        return data;
    }

    static void SortRowsReference(float[] data, int rows, int cols, SortOrder order)
    {
        for (int row = 0; row < rows; row++)
        {
            int offset = row * cols;
            if (order == SortOrder.Descending)
                Array.Sort(data, offset, cols, Comparer<float>.Create((a, b) => b.CompareTo(a)));
            else
                Array.Sort(data, offset, cols);
        }
    }
}
