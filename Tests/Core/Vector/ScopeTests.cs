using BAVCL.Core;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorTests;

public class ScopeTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
	[Fact]
	public void CpuScope_Begin_nestedScopes_keepActiveCpuUntilOutermostDispose()
	{
		var vector = CreateVector([1f, 2f, 3f]);
		vector.Residence = Residence.Gpu;

		using (vector.CpuScope())
		{
			vector.Residence.Should().Be(Residence.ActiveCpu);

			using (vector.CpuScope())
			{
				vector.Residence.Should().Be(Residence.ActiveCpu);
			}

			vector.Residence.Should().Be(Residence.ActiveCpu);
		}

		vector.Residence.Should().Be(Residence.Cpu);
	}

	[Fact]
	public void CpuScope_BeginAndSync_uploadsOnDispose()
	{
		var vector = CreateVector([1f, 2f, 3f]);
		vector.Residence = Residence.Gpu;

		using (var scope = vector.CpuScopeAndSync())
		{
			EditableView<float> view = scope.View;
			view[0] = 99f;
		}

		vector.Residence.Should().Be(Residence.InSync);
		vector.RetrieveReadOnlySpan()[0].Should().Be(99f);
	}

	[Fact]
	public void CpuScope_Begin_crossContext_throwsWhenGpuScopeActive()
	{
		var vector = CreateVector([1f, 2f, 3f]);

		using (GpuScope.Begin(vector))
		{
			Action act = () =>
			{
				using CpuScope<float> _ = vector.CpuScope();
			};

			act.Should().Throw<InvalidOperationException>();
		}
	}

	[Fact]
	public void CpuScope_Begin_requiresVectorBase()
	{
		ICacheable<float> cacheable = new NonVectorCacheable();

		Action act = () =>
		{
			using CpuScope<float> _ = CpuScope.Begin(cacheable);
		};

		act.Should().Throw<NotSupportedException>();
	}

	[Fact]
	public void GpuScope_Begin_setsActiveGpuOnModified()
	{
		var vector = CreateVector([1f, 2f, 3f]);
		vector.Residence = Residence.InSync;

		using (GpuScope.Begin(vector))
		{
			vector.Residence.Should().Be(Residence.ActiveGpu);
			vector.LiveCount.Should().Be(1u);
		}

		vector.Residence.Should().Be(Residence.Gpu);
		vector.LiveCount.Should().Be(0u);
	}

	sealed class NonVectorCacheable : ICacheable<float>
	{
		public Residence Residence { get; set; }
		public uint LiveCount { get; private set; }
		public uint ID { get; set; }
		public long MemorySize => 0;

		public void DeCache() { }
		public void IncrementLiveCount() => LiveCount++;
		public void DecrementLiveCount() => LiveCount--;
		public void SyncCPU() { }
		public void SyncCPU(ILGPU.Runtime.MemoryBuffer buffer) { }
		public ILGPU.Runtime.MemoryBuffer UpdateCache() => throw new NotImplementedException();
		public ReadOnlySpan<float> RetrieveReadOnlySpan() => [];
		public ILGPU.Runtime.MemoryBuffer UpdateCache(float[] array) => throw new NotImplementedException();
	}
}
