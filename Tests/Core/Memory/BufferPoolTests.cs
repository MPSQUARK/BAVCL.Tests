using BAVCL.Core.Memory;using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.Memory;

public sealed class BufferPoolTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
	[Fact]
	public void Rent_ReturnsEntityWithNonZeroBufferId()
	{
		BufferPool pool = BufferPools.For(Gpu);

		using BufferEntity<int> entity = pool.Int.Rent(16);

		entity.BufferId.Should().BeGreaterThan(0u);
		entity.View.Length.Should().Be(16);
		entity.Capacity.Should().BeGreaterOrEqualTo(16);
	}

	[Fact]
	public void Rent_RoundsCapacityToPowerOfTwo()
	{
		BufferPool pool = BufferPools.For(Gpu);

		using BufferEntity<int> entity = pool.Int.Rent(5);

		entity.Capacity.Should().Be(8);
	}

	[Fact]
	public void EntityDispose_ReturnsToPool()
	{
		BufferPool pool = BufferPools.For(Gpu);

		using (pool.Int.Rent(8)) { }

		Action act = () =>
		{
			using BufferEntity<int> second = pool.Int.Rent(8);
			second.BufferId.Should().BeGreaterThan(0u);
		};

		act.Should().NotThrow();
	}

	[Fact]
	public void BufferPools_For_ReturnsSameInstancePerGpu()
	{
		BufferPool a = BufferPools.For(Gpu);
		BufferPool b = BufferPools.For(Gpu);

		ReferenceEquals(a, b).Should().BeTrue();
	}

	[Fact]
	public void Pool_ExposesOnlyIntAndFloatLanes()
	{
		BufferPool pool = BufferPools.For(Gpu);

		pool.Int.Should().NotBeNull();
		pool.Float.Should().NotBeNull();
	}
}
