using BAVCL.Core.Helpers;
using BAVCL.Core.Kernels;
using BAVCL.Benchmarks.Infrastructure;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Modules.Statistics;

[Collection("GpuSerial")]
public class VarReducePathTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    static readonly Random Rng = new(47);

    [Fact]
    public void DeviceDefaultPath_UsesSharedScratchOnTypicalHardware()
    {
        KernelSharedMemory.SelectVarianceReducePath(Gpu.accelerator)
            .Should().Be(KernelSharedMemory.VarianceReducePath.SharedScratch);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(17)]
    [InlineData(256)]
    [InlineData(4096)]
    [InlineData(65_536)]
    public void VarFloat_EachKernelPath_MatchesCpuReference(int length)
    {
        float[] data = RandomData(length);
        var vector = CreateVector(data);
        float expected = VarianceAccumOps.PopulationVariance(data);

        foreach (VarReduceKernelHarness.Path path in Enum.GetValues<VarReduceKernelHarness.Path>())
            VarReduceKernelHarness.VarFloat(vector, path).ShouldBeCloseTo(expected, 1e-5f);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(256)]
    [InlineData(4096)]
    public void VarInt_EachKernelPath_MatchesCpuReference(int length)
    {
        int[] data = RandomIntData(length);
        var vector = CreateVectorInt(data);
        float expected = VarianceAccumOps.PopulationVariance(data);

        foreach (VarReduceKernelHarness.Path path in Enum.GetValues<VarReduceKernelHarness.Path>())
            VarReduceKernelHarness.VarInt(vector, path).ShouldBeCloseTo(expected, 1e-5f);
    }

    [Theory]
    [InlineData(256)]
    [InlineData(10_000)]
    public void VarFloat_BothKernelPaths_Agree(int length)
    {
        float[] data = RandomData(length, largeMeanOffset: true);
        var vector = CreateVector(data);

        float shared = VarReduceKernelHarness.VarFloat(vector, VarReduceKernelHarness.Path.SharedScratch);
        float warpTree = VarReduceKernelHarness.VarFloat(vector, VarReduceKernelHarness.Path.WarpTree);

        shared.ShouldBeCloseTo(warpTree, 1e-3f);
    }

    [Fact]
    public void VarX_UsesProductionKernelSelectedAtLoad()
    {
        float[] data = [100_001f, 100_002f, 100_003f, 100_004f, 100_005f];
        var vector = CreateVector(data);
        vector.VarX().ShouldBeCloseTo(2f, 1e-5f);
    }

    static float[] RandomData(int length, bool largeMeanOffset = false)
    {
        float[] data = new float[length];
        for (int i = 0; i < length; i++)
        {
            float value = (float)(Rng.NextDouble() * 20 - 10);
            data[i] = largeMeanOffset ? value + 100_000f : value;
        }

        return data;
    }

    static int[] RandomIntData(int length)
    {
        int[] data = new int[length];
        for (int i = 0; i < length; i++)
            data[i] = Rng.Next(-50, 50);
        return data;
    }
}
