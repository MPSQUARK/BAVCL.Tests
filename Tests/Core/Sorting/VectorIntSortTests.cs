using BAVCL.Modules.Sorting;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.Sorting;

[Collection("GpuSerial")]
public class VectorIntSortTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    static readonly Random Rng = new(42);

    [Fact]
    public void SortAscending_1D_MatchesArraySort()
    {
        int[] data = RandomData(256);
        var vector = CreateVectorInt(data);
        var expected = (int[])data.Clone();
        Array.Sort(expected);

        vector.SortAscending();
        SyncValues(vector).Should().Equal(expected);
    }

    [Fact]
    public void SortDescending_1D_MatchesArraySort()
    {
        int[] data = RandomData(256);
        var vector = CreateVectorInt(data);
        var expected = (int[])data.Clone();
        Array.Sort(expected, Comparer<int>.Create((a, b) => b.CompareTo(a)));

        vector.SortDescending();
        SyncValues(vector).Should().Equal(expected);
    }

    [Theory]
    [InlineData(SortOrder.Ascending)]
    [InlineData(SortOrder.Descending)]
    public void Sort_WithOrder_MatchesNamedMethods(SortOrder order)
    {
        int[] data = RandomData(128);
        var viaOrder = CreateVectorInt(data);
        var viaNamed = CreateVectorInt(data);

        viaOrder.Sort(order);
        if (order == SortOrder.Ascending)
            viaNamed.SortAscending();
        else
            viaNamed.SortDescending();

        SyncValues(viaOrder).Should().Equal(SyncValues(viaNamed));
    }

    [Fact]
    public void Sort_2D_SortsEachRowIndependently()
    {
        int rows = 4;
        int cols = 8;
        int[] data = RandomData(rows * cols);
        var vector = CreateVectorInt(data, columns: cols);
        var expected = (int[])data.Clone();
        SortRowsReference(expected, rows, cols, SortOrder.Ascending);

        vector.SortAscending();
        SyncValues(vector).Should().Equal(expected);
    }

    [Fact]
    public void Sort_Empty_IsNoOp()
    {
        var vector = CreateVectorInt([], cache: false);

        vector.SortAscending();

        vector.Length.Should().Be(0);
    }

    [Fact]
    public void Sort_LengthOne_IsNoOp()
    {
        var vector = CreateVectorInt([7]);

        vector.SortAscending();

        SyncValues(vector).Should().Equal([7]);
    }

    [Fact]
    public void Sort_SingleRowMatrix_Matches1DVector()
    {
        int[] data = RandomData(64);
        var as1D = CreateVectorInt(data);
        var asMatrix = CreateVectorInt(data, columns: data.Length);

        as1D.SortAscending();
        asMatrix.SortAscending();

        SyncValues(as1D).Should().Equal(SyncValues(asMatrix));
    }

    [Fact]
    public void SortAscendingX_MatchesCpuSort_1D()
    {
        int[] data = RandomData(512);
        var cpu = CreateVectorInt(data);
        var gpu = CreateVectorInt(data);

        cpu.SortAscending();
        gpu.SortAscendingX();

        SyncValues(gpu).Should().Equal(SyncValues(cpu));
    }

    [Fact]
    public void SortDescendingX_MatchesCpuSort_2D()
    {
        int rows = 3;
        int cols = 16;
        int[] data = RandomData(rows * cols);
        var cpu = CreateVectorInt(data, columns: cols);
        var gpu = CreateVectorInt(data, columns: cols);

        cpu.SortDescending();
        gpu.SortDescendingX();

        SyncValues(gpu).Should().Equal(SyncValues(cpu));
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
