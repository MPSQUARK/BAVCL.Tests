using BAVCL.Core.Exceptions;
using BAVCL.Core.Memory;
using BAVCL.Tests.Helpers;
using BAVCL.Types;

namespace BAVCL.Tests.Core.Memory;

public sealed class VesselPossessionTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
	[Fact]
	public void CreateVessel_StartsAsVessel()
	{
		Vessel<Mask> maskVessel = Mask.CreateVessel(Gpu, 64);
		Vessel<VectorInt> intVessel = VectorInt.CreateVessel(Gpu);

		maskVessel.IsVessel.Should().BeTrue();
		maskVessel.Target.ID.Should().Be(0u);
		intVessel.IsVessel.Should().BeTrue();
	}

	[Fact]
	public void Possess_SoulHasBufferId_Banish_RestoresVessel()
	{
		BufferPool pool = BufferPools.For(Gpu);
		const int elementCount = 64;
		int wordCount = (elementCount + 31) / 32;

		using BufferEntity<int> entity = pool.Int.Rent(wordCount);
		Vessel<Mask> vessel = Mask.CreateVessel(Gpu, elementCount);

		Mask soul;
		using (BufferEntity<int>.PossessionScope<Mask> possession = entity.Possess(vessel))
		{
			soul = possession.Soul;
			soul.ID.Should().BeGreaterThan(0u);
			soul.ElementCount.Should().Be(elementCount);
		}

		soul.ID.Should().Be(0u);
		vessel.IsVessel.Should().BeTrue();
	}

	[Fact]
	public void Possess_VectorIntSoul_AllowsCpuWrite()
	{
		BufferPool pool = BufferPools.For(Gpu);

		using BufferEntity<int> entity = pool.Int.Rent(4);
		Vessel<VectorInt> vessel = VectorInt.CreateVessel(Gpu);

		int[] values;
		using (var possession = entity.Possess(vessel))
		{
			using (possession.Soul.CpuScopeAndSync())
				possession.Soul.Value = [7, 8, 9, 10];

			values = possession.Soul.ToArray();
		}

		values.Should().Equal([7, 8, 9, 10]);
	}

	[Fact]
	public void Possess_ThenSecondPossessAfterBanish_Succeeds()
	{
		BufferPool pool = BufferPools.For(Gpu);

		using BufferEntity<int> entity = pool.Int.Rent(4);
		Vessel<VectorInt> vessel = VectorInt.CreateVessel(Gpu);

		using (entity.Possess(vessel)) { }

		using (entity.Possess(vessel))
		{
			vessel.Target.ID.Should().BeGreaterThan(0u);
		}

		vessel.IsVessel.Should().BeTrue();
	}

	[Fact]
	public void Possess_WhileAlreadyPossessed_Throws()
	{
		BufferPool pool = BufferPools.For(Gpu);

		using BufferEntity<int> entity = pool.Int.Rent(4);
		Vessel<VectorInt> vessel = VectorInt.CreateVessel(Gpu);

		using BufferEntity<int>.PossessionScope<VectorInt> outer = entity.Possess(vessel);

		Action act = () => entity.Possess(vessel);

		act.Should().Throw<EntityAlreadyPossessedException>();
	}

	[Fact]
	public void Possess_VesselAlreadyInhabited_Throws()
	{
		BufferPool pool = BufferPools.For(Gpu);
		Mask inhabited = CreateMask(new bool[32]);

		using BufferEntity<int> entity = pool.Int.Rent(1);
		Vessel<Mask> vessel = inhabited.AsVessel(Gpu);

		Action act = () => entity.Possess(vessel);

		act.Should().Throw<VesselAlreadyInhabitedException>();
	}

	[Fact]
	public void Dispose_EntityWhilePossessed_Throws()
	{
		BufferPool pool = BufferPools.For(Gpu);

		BufferEntity<int> entity = pool.Int.Rent(4);
		Vessel<VectorInt> vessel = VectorInt.CreateVessel(Gpu);
		BufferEntity<int>.PossessionScope<VectorInt> possession = entity.Possess(vessel);

		Action act = () => entity.Dispose();

		act.Should().Throw<EntityDisposeWhilePossessedException>();

		possession.Dispose();
		entity.Dispose();
	}

	[Fact]
	public void Possess_VesselGpuMismatch_Throws()
	{
		using global::BAVCL.GPU otherGpu = GPUManager.GetGPU(forceCPU: true);
		if (ReferenceEquals(otherGpu, Gpu))
			return;

		BufferPool pool = BufferPools.For(Gpu);
		using BufferEntity<int> entity = pool.Int.Rent(4);
		Vessel<Mask> vessel = Mask.CreateVessel(otherGpu, 32);

		Action act = () => entity.Possess(vessel);

		act.Should().Throw<VesselGpuMismatchException>();
	}

	[Fact]
	public void Banish_IsIdempotent()
	{
		BufferPool pool = BufferPools.For(Gpu);

		using BufferEntity<int> entity = pool.Int.Rent(4);
		Vessel<VectorInt> vessel = VectorInt.CreateVessel(Gpu);

		BufferEntity<int>.PossessionScope<VectorInt> possession = entity.Possess(vessel);
		possession.Dispose();
		possession.Dispose();

		vessel.IsVessel.Should().BeTrue();
	}
}
