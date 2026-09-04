using BAVCL.Core.Exceptions;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.GPU;

public class KernelExceptionTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void TestSQRTKernel_ThrowsKernelNotCompiledException()
    {
        var act = () =>
        {
            Gpu.TestSqrtIP_Kern(
                Gpu.accelerator.DefaultStream,
                1,
                Gpu.accelerator.Allocate1D<float>(1).View,
                Gpu.accelerator.Allocate1D<float>(1).View);
        };

        act.Should().Throw<KernelNotCompiledException>()
            .WithMessage("*TestSqrtIP_Kern*");
    }

    [Fact]
    public void TestMYSQRTKernel_ThrowsKernelNotCompiledException()
    {
        var act = () =>
        {
            Gpu.TestMySqrtIP_Kern(
                Gpu.accelerator.DefaultStream,
                1,
                Gpu.accelerator.Allocate1D<float>(1).View,
                Gpu.accelerator.Allocate1D<float>(1).View);
        };

        act.Should().Throw<KernelNotCompiledException>()
            .WithMessage("*TestMySqrtIP_Kern*");
    }
}
