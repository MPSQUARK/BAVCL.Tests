using BAVCL.Core.Memory;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.Memory;

public sealed class BufferEntityTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
	[Fact]
	public void KernelPath_EntityViewUsableInGpuScope()
	{
		BufferPool pool = BufferPools.For(Gpu);

		using BufferEntity<int> entity = pool.Int.Rent(4);
		using (GpuScope.Begin(entity.PinTarget))
		{
			entity.View.Length.Should().Be(4);
			entity.BufferId.Should().BeGreaterThan(0u);
		}
	}

	[Fact]
	public void RentFloat_ReturnsEntity()
	{
		BufferPool pool = BufferPools.For(Gpu);

		using BufferEntity<float> entity = pool.Float.Rent(8);

		entity.View.Length.Should().Be(8);
		entity.BufferId.Should().BeGreaterThan(0u);
	}
}
