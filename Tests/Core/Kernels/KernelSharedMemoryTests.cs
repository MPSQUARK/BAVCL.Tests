using BAVCL.Core.Kernels;

namespace BAVCL.Tests.Core.Kernels;

public class KernelSharedMemoryTests
{
    [Fact]
    public void VarianceReduceSharedBytes_IsExpectedLayout()
    {
        KernelSharedMemory.VarianceReduceSharedBytes().Should().Be(256 * (sizeof(int) + sizeof(float) + sizeof(float)));
    }

    [Fact]
    public void VarianceReduceWarpTreeBytes_IsExpectedLayout()
    {
        KernelSharedMemory.VarianceReduceWarpTreeBytes().Should().Be(
            KernelSharedMemory.VarianceWarpLeaderSlots * (sizeof(int) + sizeof(float) + sizeof(float)));
    }

    [Theory]
    [InlineData(10_000, 0)]
    [InlineData(3072, 0)]
    [InlineData(3071, 1)]
    [InlineData(96, 1)]
    public void SelectVarianceReducePath_PicksExpectedPath(long limit, int expectedPathId)
    {
        var expected = (KernelSharedMemory.VarianceReducePath)expectedPathId;
        KernelSharedMemory.SelectVarianceReducePath(limit).Should().Be(expected);
    }

    [Theory]
    [InlineData(95)]
    [InlineData(0)]
    public void SelectVarianceReducePath_WhenBelowWarpTree_Throws(long limit)
    {
        var act = () => KernelSharedMemory.SelectVarianceReducePath(limit);
        act.Should().Throw<NotSupportedException>()
            .WithMessage("*at least 96 bytes*");
    }
}
