using BAVCL.Core.Enums;
using BAVCL.Core.Exceptions;
using BAVCL.Geometric;
using BAVCL.Geometric.Enums;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Geometric.Vector3Tests;

public class Vector3IndexingTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void Indexer_SingleVertex_ReturnsCoordValue()
    {
        var vec = new Vector3(Gpu, [1f, 2f, 3f]);

        vec[0, Coord.x].Should().Be(1f);
        vec[0, Coord.y].Should().Be(2f);
        vec[0, Coord.z].Should().Be(3f);
    }

    [Fact]
    public void Indexer_TwoByTwoGrid_ReturnsCoordValues()
    {
        // 2x2 grid of vertices (4 vertices, 12 floats)
        var vec = new Vector3(Gpu,
        [
            1f, 2f, 3f,   // row 0
            4f, 5f, 6f,   // row 1
            7f, 8f, 9f,   // row 2
            10f, 11f, 12f // row 3
        ]);

        vec[0, Coord.y].Should().Be(2f);
        vec[1, Coord.z].Should().Be(6f);
        vec[2, Coord.x].Should().Be(7f);
        vec[3, Coord.y].Should().Be(11f);
    }

    [Fact]
    public void Indexer_ThreeByThreeGrid_ReturnsCoordValues()
    {
        // 3x3 grid of vertices (9 vertices, 27 floats)
        var values = Enumerable.Range(1, 27).Select(i => (float)i).ToArray();
        var vec = new Vector3(Gpu, values);

        vec[0, Coord.x].Should().Be(1f);
        vec[0, Coord.z].Should().Be(3f);
        vec[4, Coord.y].Should().Be(14f); // row 4: indices 13,14,15
        vec[8, Coord.z].Should().Be(27f);
    }

    [Fact]
    public void GetAtSetAt_RoundTrip()
    {
        var vec = new Vector3(Gpu, [1f, 2f, 3f]);

        vec.SetAt(0, Coord.x, 99f);
        vec.GetAt(0, Coord.x).Should().Be(99f);
    }

    [Fact]
    public void GetAtSetAt_WithIndexingMode_SyncsGpu()
    {
        var vec = new Vector3(Gpu, [1f, 2f, 3f, 4f, 5f, 6f]);

        vec.SetAt(1, Coord.z, IndexingMode.SyncBoth, 99f);
        vec.GetAt(1, Coord.z, IndexingMode.SyncCPU).Should().Be(99f);
    }
}
