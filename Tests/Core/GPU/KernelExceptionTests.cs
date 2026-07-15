using BAVCL.Core.Exceptions;
using BAVCL.Tests.Helpers;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Tests.Core.GPU;

public class KernelExceptionTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void TestSQRTKernel_ThrowsKernelNotCompiledException()
    {
        var act = () =>
        {
            Gpu.TestSQRTKernel(
                Gpu.accelerator.DefaultStream,
                1,
                Gpu.accelerator.Allocate1D<float>(1).View,
                Gpu.accelerator.Allocate1D<float>(1).View);
        };

        act.Should().Throw<KernelNotCompiledException>()
            .WithMessage("*TestSQRTKernel*");
    }

    [Fact]
    public void TestMYSQRTKernel_ThrowsKernelNotCompiledException()
    {
        var act = () =>
        {
            Gpu.TestMYSQRTKernel(
                Gpu.accelerator.DefaultStream,
                1,
                Gpu.accelerator.Allocate1D<float>(1).View,
                Gpu.accelerator.Allocate1D<float>(1).View);
        };

        act.Should().Throw<KernelNotCompiledException>()
            .WithMessage("*TestMYSQRTKernel*");
    }
}
