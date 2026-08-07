using BAVCL.Modules.Sorting;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.Sorting;

[Collection("GpuSerial")]
public class VectorIntSortTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    static readonly Random Rng = new(42);

    [Fact]
    public void SortAscIP_1D_MatchesArraySort()
    {
        int[] data = RandomData(256);
        var vector = CreateVectorInt(data);
        var expected = (int[])data.Clone();
        Array.Sort(expected);

        vector.SortAscIP();
        SyncValues(vector).Should().Equal(expected);
    }

    [Fact]
    public void SortDescIP_1D_MatchesArraySort()
    {
        int[] data = RandomData(256);
        var vector = CreateVectorInt(data);
        var expected = (int[])data.Clone();
        Array.Sort(expected, Comparer<int>.Create((a, b) => b.CompareTo(a)));

        vector.SortDescIP();
        SyncValues(vector).Should().Equal(expected);
    }

    [Theory]
    [InlineData(SortOrder.Ascending)]
    [InlineData(SortOrder.Descending)]
    public void SortIP_WithOrder_MatchesNamedMethods(SortOrder order)
    {
        int[] data = RandomData(128);
        var viaOrder = CreateVectorInt(data);
        var viaNamed = CreateVectorInt(data);

        viaOrder.SortIP(order);
        if (order == SortOrder.Ascending)
            viaNamed.SortAscIP();
        else
            viaNamed.SortDescIP();

        SyncValues(viaOrder).Should().Equal(SyncValues(viaNamed));
    }

    [Fact]
    public void SortIP_2D_SortsEachRowIndependently()
    {
        int rows = 4;
        int cols = 8;
        int[] data = RandomData(rows * cols);
        var vector = CreateVectorInt(data, columns: cols);
        var expected = (int[])data.Clone();
        SortRowsReference(expected, rows, cols, SortOrder.Ascending);

        vector.SortAscIP();
        SyncValues(vector).Should().Equal(expected);
    }

    [Fact]
    public void SortIP_Empty_IsNoOp()
    {
        var vector = CreateVectorInt([], cache: false);

        vector.SortAscIP();

        vector.Length.Should().Be(0);
    }

    [Fact]
    public void SortIP_LengthOne_IsNoOp()
    {
        var vector = CreateVectorInt([7]);

        vector.SortAscIP();

        SyncValues(vector).Should().Equal([7]);
    }

    [Fact]
    public void SortIP_ColumnVector_MatchesFlat1D()
    {
        int[] data = RandomData(128);
        var flat = CreateVectorInt(data);
        var column = CreateVectorInt(data, columns: 1);

        flat.SortAscIP();
        column.SortAscIP();

        SyncValues(flat).Should().Equal(SyncValues(column));
    }

    [Fact]
    public void SortAscXIP_MatchesCpuSort_1D()
    {
        int[] data = RandomData(512);
        var cpu = CreateVectorInt(data);
        var gpu = CreateVectorInt(data);

        cpu.SortAscIP();
        gpu.SortAscXIP();

        SyncValues(gpu).Should().Equal(SyncValues(cpu));
    }

    [Fact]
    public void SortDescXIP_MatchesCpuSort_2D()
    {
        int rows = 3;
        int cols = 16;
        int[] data = RandomData(rows * cols);
        var cpu = CreateVectorInt(data, columns: cols);
        var gpu = CreateVectorInt(data, columns: cols);

        cpu.SortDescIP();
        gpu.SortDescXIP();

        SyncValues(gpu).Should().Equal(SyncValues(cpu));
    }

    [Fact]
    public void SortAsc_AllocatingCpu_DoesNotMutateInput_AndReturnsSortedCopy()
    {
        int[] data = RandomData(256);
        var vector = CreateVectorInt(data);
        var expected = (int[])data.Clone();
        Array.Sort(expected);

        VectorInt result = vector.SortAsc();

        SyncValues(vector).Should().Equal(data);
        SyncValues(result).Should().Equal(expected);
    }

    [Fact]
    public void SortAscX_AllocatingGpu_DoesNotMutateInput_AndReturnsSortedCopy()
    {
        int[] data = RandomData(256);
        var vector = CreateVectorInt(data);
        var expected = (int[])data.Clone();
        Array.Sort(expected);

        VectorInt result = vector.SortAscX();

        SyncValues(vector).Should().Equal(data);
        SyncValues(result).Should().Equal(expected);
    }

    static int[] RandomData(int length)
    {
        var data = new int[length];
        for (int i = 0; i < length; i++)
            data[i] = Rng.Next(-10_000, 10_000);
        return data;
    }

    static void SortRowsReference(int[] data, int rows, int cols, SortOrder order)
    {
        for (int row = 0; row < rows; row++)
        {
            int offset = row * cols;
            if (order == SortOrder.Descending)
                Array.Sort(data, offset, cols, Comparer<int>.Create((a, b) => b.CompareTo(a)));
            else
                Array.Sort(data, offset, cols);
        }
    }
}
