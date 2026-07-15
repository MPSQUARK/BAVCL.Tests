using BAVCL.Geometric;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Geometric.Vector3Tests;

public class Vector3ConversionTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void ToVector_ConvertsToGenericVector()
    {
        var vec3 = new Vector3(Gpu, VectorFactory.Vector3Data);

        var vector = vec3.ToVector();

        vector.Length.Should().Be(9);
        vector.Columns.Should().Be(3);
        SyncValues(vector).ShouldBeCloseTo(VectorFactory.Vector3Data);
    }

    [Fact]
    public void VectorToVector3_ConvertsWhenLengthValid()
    {
        var vector = CreateVector(VectorFactory.Vector3Data);

        var vec3 = vector.ToVector3();

        vec3.SyncCPU();
        vec3.Value.ShouldBeCloseTo(VectorFactory.Vector3Data);
    }
}
