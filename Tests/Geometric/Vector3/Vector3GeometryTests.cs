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

    [Fact]
    public void Normalise_SingleVec3_ReturnsUnitVector()
    {
        var vec = new Vector3(Gpu, [1f, 2f, 3f]);
        float invMagnitude = 1f / MathF.Sqrt(14f);

        var result = vec.Normalise();

        result.SyncCPU();
        result.Value.ShouldBeCloseTo([invMagnitude, 2f * invMagnitude, 3f * invMagnitude]);
    }

    [Fact]
    public void Normalise_Batch_NormalisesEachRowIndependently()
    {
        var vec = new Vector3(Gpu, [1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f, 11f, 12f]);

        var result = Vector3.Normalise(vec);

        float invMag0 = 1f / MathF.Sqrt(14f);
        float invMag1 = 1f / MathF.Sqrt(77f);
        float invMag2 = 1f / MathF.Sqrt(194f);
        float invMag3 = 1f / MathF.Sqrt(365f);

        result.SyncCPU();
        result.Value.ShouldBeCloseTo(
        [
            invMag0, 2f * invMag0, 3f * invMag0,
            4f * invMag1, 5f * invMag1, 6f * invMag1,
            7f * invMag2, 8f * invMag2, 9f * invMag2,
            10f * invMag3, 11f * invMag3, 12f * invMag3,
        ]);
    }

    [Fact]
    public void Normalise_PostNormaliseMagnitude_IsApproximatelyOne()
    {
        var vec = new Vector3(Gpu, [1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f, 11f, 12f]);

        var normalised = vec.Normalise();
        var magnitudes = normalised.Magnitude();

        magnitudes.SyncCPU();
        for (int row = 0; row < vec.Length / 3; row++)
            magnitudes.Value[row].ShouldBeCloseTo(1f);
    }

    [Fact]
    public void Normalise_ZeroVector_ProducesNaN()
    {
        var result = new Vector3(Gpu, [0f, 0f, 0f]).Normalise();

        result.SyncCPU();
        result.Value.Should().AllSatisfy(v => float.IsNaN(v).Should().BeTrue());
    }

    [Fact]
    public void Normalise_StaticAndInstance_ReturnEqualResults()
    {
        var vec = new Vector3(Gpu, [3f, 4f, 0f, 1f, 1f, 1f]);

        var staticResult = Vector3.Normalise(vec);
        var instanceResult = vec.Normalise();

        staticResult.SyncCPU();
        instanceResult.SyncCPU();
        staticResult.Value.ShouldBeCloseTo(instanceResult.Value);
    }
}
