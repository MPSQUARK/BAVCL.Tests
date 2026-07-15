using BAVCL.Core.Exceptions;
using BAVCL.Geometric;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Geometric.Vector3Tests;

public class Vector3GeometryTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void Cross_ProducesPerpendicularVector()
    {
        var a = new Vector3(Gpu, [1f, 0f, 0f]);
        var b = new Vector3(Gpu, [0f, 1f, 0f]);

        var result = Vector3.Cross(a, b);

        result.SyncCPU();
        result.Value.ShouldBeCloseTo([0f, 0f, 1f]);
    }

    [Fact]
    public void Magnitude_ReturnsLengthPerRow()
    {
        var vec = new Vector3(Gpu, [3f, 4f, 0f]);

        var magnitudes = Vector3.Magnitude(vec);

        magnitudes.SyncCPU();
        magnitudes.Value[0].ShouldBeCloseTo(5f);
    }

    [Fact]
    public void Distance_ReturnsDistancePerRow()
    {
        var a = new Vector3(Gpu, [0f, 0f, 0f]);
        var b = new Vector3(Gpu, [3f, 4f, 0f]);

        var distances = Vector3.Distance(a, b);

        distances.SyncCPU();
        distances.Value[0].ShouldBeCloseTo(5f);
    }

    [Fact]
    public void Magnitude_DifferentLengths_ThrowsVector3LengthMismatchException()
    {
        var a = new Vector3(Gpu, [1f, 2f, 3f]);
        var b = new Vector3(Gpu, [1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f]);

        var act = () => Vector3.Magnitude(a, b);

        act.Should().Throw<Vector3LengthMismatchException>()
            .WithMessage("*Magnitude*");
    }

    [Fact]
    public void Distance_DifferentLengths_ThrowsVector3LengthMismatchException()
    {
        var a = new Vector3(Gpu, [1f, 2f, 3f]);
        var b = new Vector3(Gpu, [1f, 2f, 3f, 4f, 5f, 6f]);

        var act = () => Vector3.Distance(a, b);

        act.Should().Throw<Vector3LengthMismatchException>()
            .WithMessage("*Distance*");
    }
}
