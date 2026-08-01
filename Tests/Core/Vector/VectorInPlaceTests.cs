using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorTests;

public class VectorInPlaceTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void Abs_IP_MutatesInPlace()
    {
        var vector = CreateVector([.. VectorFactory.MixedSigns]);

        vector.Abs_IP();
        vector.SyncCPU();

        vector.Value.ShouldBeCloseTo(CpuReference.Abs(VectorFactory.MixedSigns));
    }

    [Fact]
    public void IPOP_Add_MutatesInPlace()
    {
        var vector = CreateVector([1f, 2f, 3f]);
        var other = CreateVector([10f, 20f, 30f]);

        vector.IPOP(other, Operations.add);
        vector.SyncCPU();

        vector.Value.ShouldBeCloseTo([11f, 22f, 33f]);
    }

    [Fact]
    public void IPOP_ScalarMultiply_MutatesInPlace()
    {
        var vector = CreateVector([2f, 4f, 6f]);

        vector.IPOP(3f, Operations.multiply);
        vector.SyncCPU();

        vector.Value.ShouldBeCloseTo([6f, 12f, 18f]);
    }

    [Fact]
    public void Normalise_IP_DividesBySum()
    {
        var vector = CreateVector([2f, 2f, 2f]);

        vector.Normalise_IP();
        vector.SyncCPU();

        vector.Sum().ShouldBeCloseTo(1f, 1e-3f);
    }
}
