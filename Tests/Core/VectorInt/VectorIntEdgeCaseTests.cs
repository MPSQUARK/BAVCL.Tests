using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorIntTests;

public class VectorIntEdgeCaseTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void Multiply_Overflow_WrapsUnchecked()
    {
        var a = CreateVectorInt([int.MaxValue, 1]);
        var b = CreateVectorInt([2, 1]);

        VectorInt result = a * b;

        SyncValues(result).Should().Equal([unchecked(int.MaxValue * 2), 1]);
    }

    [Fact]
    public void Divide_ByZero_DoesNotThrowOnSync()
    {
        var a = CreateVectorInt([7, -7]);
        var b = CreateVectorInt([0, 0]);

        VectorInt result = a / b;

        Action sync = () => SyncValues(result);
        sync.Should().NotThrow();
    }

    [Fact]
    public void ExplicitCast_ToVectorInt_RejectsInfinity()
    {
        var vector = CreateVector([1f, float.PositiveInfinity]);

        Action cast = () => _ = (VectorInt)vector;

        cast.Should().Throw<InvalidCastException>();
    }
}
