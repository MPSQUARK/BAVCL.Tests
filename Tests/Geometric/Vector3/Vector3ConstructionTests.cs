using BAVCL.Geometric;
using BAVCL.Geometric.Enums;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Geometric.Vector3Tests;

public class Vector3ConstructionTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void Constructor_WithValues_SetsLengthAndColumns()
    {
        var vec3 = new Vector3(Gpu, VectorFactory.Vector3Data);

        vec3.Length.Should().Be(9);
        vec3.Columns.Should().Be(3);
    }

    [Fact]
    public void Constructor_InvalidLength_Throws()
    {
        var act = () => new Vector3(Gpu, [1f, 2f]);

        act.Should().Throw<Exception>().WithMessage("*multiple of 3*");
    }

    [Fact]
    public void Zeros_CreatesZeroFilledVector3()
    {
        var vec3 = Zeros(Gpu, 6);

        vec3.Length.Should().Be(6);
        vec3.SyncCPU();
        vec3.Value.Should().AllBeEquivalentTo(0f);
    }

    [Fact]
    public void Fill_CreatesConstantVector3()
    {
        var vec3 = Fill(Gpu, 2.5f, 6);

        vec3.SyncCPU();
        vec3.Value.Should().AllBeEquivalentTo(2.5f);
    }
}
