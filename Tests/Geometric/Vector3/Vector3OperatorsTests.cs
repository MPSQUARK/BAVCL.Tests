using BAVCL.Geometric;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Geometric.Vector3Tests;

public class Vector3OperatorsTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    private BAVCL.Geometric.Vector3 CreateVec3(float[] values) => new(Gpu, values);

    [Fact]
    public void Addition_AddsElementWise()
    {
        var a = CreateVec3([1f, 2f, 3f, 4f, 5f, 6f]);
        var b = CreateVec3([1f, 1f, 1f, 2f, 2f, 2f]);

        var result = a + b;

        result.SyncCPU();
        result.Value.ShouldBeCloseTo([2f, 3f, 4f, 6f, 7f, 8f]);
    }

    [Fact]
    public void ScalarMultiplication_ScalesAllComponents()
    {
        var vec = CreateVec3([1f, 2f, 3f]);

        var result = vec * 2f;

        result.SyncCPU();
        result.Value.ShouldBeCloseTo([2f, 4f, 6f]);
    }

    [Fact]
    public void Division_DividesElementWise()
    {
        var a = CreateVec3([4f, 6f, 8f]);
        var b = CreateVec3([2f, 3f, 4f]);

        var result = a / b;

        result.SyncCPU();
        result.Value.ShouldBeCloseTo([2f, 2f, 2f]);
    }
}
