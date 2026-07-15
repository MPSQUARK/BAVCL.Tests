using BAVCL.Core.Enums;
using BAVCL.Geometric;
using BAVCL.Geometric.Enums;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Geometric.Vector3Tests;

public class Vector3IndexingTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void Indexer_ReturnsCoordValue()
    {
        var vec = new Vector3(Gpu, [1f, 2f, 3f, 4f, 5f, 6f]);

        vec[0, Coord.y].Should().Be(2f);
        vec[1, Coord.z].Should().Be(6f);
    }

    [Fact]
    public void GetAtSetAt_RoundTrip()
    {
        var vec = new Vector3(Gpu, [1f, 2f, 3f]);

        vec.SetAt(0, Coord.x, 99f);
        vec.GetAt(0, Coord.x).Should().Be(99f);
    }
}
