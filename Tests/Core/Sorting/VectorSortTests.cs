using BAVCL.Modules.Sorting;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.Sorting;

[Collection("GpuSerial")]
public class VectorSortTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    static readonly Random Rng = new(17);

    [Fact]
    public void SortAscending_1D_MatchesArraySort()
    {
        float[] data = RandomData(256);
        var vector = CreateVector(data);
        var expected = (float[])data.Clone();
        Array.Sort(expected);

        vector.SortAscending();
        SyncValues(vector).ShouldBeCloseTo(expected);
    }

    [Fact]
    public void SortDescending_2D_SortsEachRowIndependently()
    {
        int rows = 3;
        int cols = 10;
        float[] data = RandomData(rows * cols);
        var vector = CreateVector(data, columns: cols);
        var expected = (float[])data.Clone();
        SortRowsReference(expected, rows, cols, SortOrder.Descending);

        vector.SortDescending();
        SyncValues(vector).ShouldBeCloseTo(expected);
    }

    [Fact]
    public void Sort_SingleRowMatrix_Matches1DVector()
    {
        float[] data = RandomData(48);
        var as1D = CreateVector(data);
        var asMatrix = CreateVector(data, columns: data.Length);

        as1D.SortAscending();
        asMatrix.SortAscending();

        SyncValues(as1D).ShouldBeCloseTo(SyncValues(asMatrix));
    }

    [Fact]
    public void SortAscendingX_MatchesCpuSort_1D()
    {
        float[] data = RandomData(512);
        var cpu = CreateVector(data);
        var gpu = CreateVector(data);

        cpu.SortAscending();
        gpu.SortAscendingX();

        SyncValues(gpu).ShouldBeCloseTo(SyncValues(cpu));
    }

    [Fact]
    public void SortDescendingX_MatchesCpuSort_2D()
    {
        int rows = 4;
        int cols = 12;
        float[] data = RandomData(rows * cols);
        var cpu = CreateVector(data, columns: cols);
        var gpu = CreateVector(data, columns: cols);

        cpu.SortDescending();
        gpu.SortDescendingX();

        SyncValues(gpu).ShouldBeCloseTo(SyncValues(cpu));
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
