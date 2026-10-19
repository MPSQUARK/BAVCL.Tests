using BAVCL.Core.Exceptions;

namespace BAVCL.Tests.Core.GPU;

[Collection("GpuSerial")]
public class SortingDomainLoadTests
{
	[Fact]
	public void SortAscXIP_Float_WithoutSortingDomain_ThrowsKernelNotCompiledException()
	{
		global::BAVCL.GPU gpu = GPUManager.GetGPU();
		KernelModuleLoader.Load<float>(gpu, KernelWorkloads.Default);
		var vector = new Vector(gpu, [3f, 1f, 2f]);

		var act = () => vector.SortAscXIP();

		act.Should().Throw<KernelNotCompiledException>()
			.WithMessage("*floatToSortableInt*");
	}

	[Fact]
	public void ArgsortAscX_Float_WithoutSortingDomain_ThrowsKernelNotCompiledException()
	{
		global::BAVCL.GPU gpu = GPUManager.GetGPU();
		KernelModuleLoader.Load<float>(gpu, KernelWorkloads.Default);
		var vector = new Vector(gpu, [3f, 1f, 2f]);

		var act = () => vector.ArgsortAscX();

		act.Should().Throw<KernelNotCompiledException>()
			.WithMessage("*floatToSortableInt*");
	}

	[Fact]
	public void SortAscXIP_2D_Int_WithoutSortingDomain_ThrowsKernelNotCompiledException()
	{
		global::BAVCL.GPU gpu = GPUManager.GetGPU();
		KernelModuleLoader.Load<int>(gpu, KernelWorkloads.Default);
		var vector = new VectorInt(gpu, [3, 1, 2, 6, 5, 4], columns: 3);

		var act = () => vector.SortAscXIP();

		act.Should().Throw<KernelNotCompiledException>()
			.WithMessage("*segmentedSortIntAscIP*");
	}
}
