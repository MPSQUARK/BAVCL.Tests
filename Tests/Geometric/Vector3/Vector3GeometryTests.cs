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

        var result = a.Cross(b);

        result.SyncCPU();
        result.Value.ShouldBeCloseTo([0f, 0f, 1f]);
    }

    [Fact]
    public void Magnitude_ReturnsLengthPerRow()
    {
        var vec = new Vector3(Gpu, [3f, 4f, 0f]);

        var magnitudes = vec.Magnitude();

        magnitudes.SyncCPU();
        magnitudes.Value[0].ShouldBeCloseTo(5f);
    }

    [Fact]
    public void Distance_ReturnsDistancePerRow()
    {
        var a = new Vector3(Gpu, [0f, 0f, 0f]);
        var b = new Vector3(Gpu, [3f, 4f, 0f]);

        var distances = a.Distance(b);

        distances.SyncCPU();
        distances.Value[0].ShouldBeCloseTo(5f);
    }

    [Fact]
    public void Magnitude_DifferentLengths_ThrowsLengthMismatchException()
    {
        var a = new Vector3(Gpu, [1f, 2f, 3f]);
        var b = new Vector3(Gpu, [1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f]);

        var act = () => a.Magnitude(b);

        act.Should().Throw<LengthMismatchException>()
            .WithMessage("*Magnitude*");
    }

    [Fact]
    public void Distance_DifferentLengths_ThrowsLengthMismatchException()
    {
        var a = new Vector3(Gpu, [1f, 2f, 3f]);
        var b = new Vector3(Gpu, [1f, 2f, 3f, 4f, 5f, 6f]);

        var act = () => a.Distance(b);

        act.Should().Throw<LengthMismatchException>()
            .WithMessage("*Distance*");
    }
}
