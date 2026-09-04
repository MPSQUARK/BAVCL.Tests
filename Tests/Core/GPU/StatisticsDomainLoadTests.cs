using BAVCL.Core.Exceptions;
using BAVCL.Modules.Arithmetic;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.GPU;

[Collection("GpuSerial")]
public class StatisticsDomainLoadTests
{
    [Fact]
    public void SumX_WithoutStatisticsDomain_ThrowsKernelNotCompiledException()
    {
        global::BAVCL.GPU gpu = GPUManager.GetGPU();
        KernelModuleLoader.Load<float>(gpu, KernelWorkloads.Default);
        var vector = new Vector(gpu, [1f, 2f, 3f]);

        var act = () => vector.SumX();

        act.Should().Throw<KernelNotCompiledException>()
            .WithMessage("*sumReduceFloatGroupedCompensated*");
    }

    [Fact]
    public void SumX_SmallN_WithoutStatisticsDomain_ThrowsGroupedCompensatedKernel()
    {
        global::BAVCL.GPU gpu = GPUManager.GetGPU();
        KernelModuleLoader.Load<float>(gpu, KernelWorkloads.Default);
        var vector = new Vector(gpu, [1f, 2f, 3f]);

        var act = () => vector.SumX();

        act.Should().Throw<KernelNotCompiledException>()
            .WithMessage("*sumReduceFloatGroupedCompensated*");
    }

    [Fact]
    public void DotX_WithoutStatisticsDomain_ThrowsKernelNotCompiledException()
    {
        global::BAVCL.GPU gpu = GPUManager.GetGPU();
        KernelModuleLoader.Load<float>(gpu, KernelWorkloads.Default);
        var left = new Vector(gpu, [1f, 2f, 3f]);
        var right = new Vector(gpu, [4f, 5f, 6f]);

        var act = () => left.DotX(right);

        act.Should().Throw<KernelNotCompiledException>()
            .WithMessage("*dotReduceFloatGroupedCompensated*");
    }
}
