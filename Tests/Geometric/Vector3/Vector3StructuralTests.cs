using BAVCL.Geometric;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Geometric.Vector3Tests;

public class Vector3StructuralTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void Copy_CreatesEqualVector3()
    {
        var original = new Vector3(Gpu, [1f, 2f, 3f, 4f, 5f, 6f]);

        var copy = original.Copy();

        copy.SyncCPU();
        copy.Value.ShouldBeCloseTo(original.Value);
    }

    [Fact]
    public void Concat_AppendsVector3Values()
    {
        var a = new Vector3(Gpu, [1f, 2f, 3f]);
        var b = new Vector3(Gpu, [4f, 5f, 6f]);

        var result = Vector3.Concat(a, b);

        result.SyncCPU();
        result.Length.Should().Be(6);
        result.Value.ShouldBeCloseTo([1f, 2f, 3f, 4f, 5f, 6f]);
    }

    [Fact]
    public void AccessRow_ReturnsSingleVertexRow()
    {
        var vec = new Vector3(Gpu, [1f, 2f, 3f, 4f, 5f, 6f]);

        var row = Vector3.AccessRow(vec, 1);

        row.SyncCPU();
        row.Value.ShouldBeCloseTo([4f, 5f, 6f]);
    }
}
