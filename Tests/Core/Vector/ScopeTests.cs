using System.Threading;
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
	public void CpuScope_Begin_syncToGpu_uploadsOnDispose()
	{
		var vector = CreateVector([1f, 2f, 3f]);
		vector.Residence = Residence.Gpu;

		using (var scope = vector.CpuScopeAndSync())
		{
			scope.HasView.Should().BeTrue();
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
	public void CpuScope_Begin_worksOnCoherenceOnlyCacheable()
	{
		var host = new CoherenceOnlyCacheable();

		using (var scope = host.CpuScopeAndSync())
		{
			scope.HasView.Should().BeFalse();
			host.Residence.Should().Be(Residence.ActiveCpu);
			host.SyncCpuEntered.Should().BeTrue();
		}

		host.Residence.Should().Be(Residence.InSync);
		host.UpdateCacheCalled.Should().BeTrue();
	}

	[Fact]
	public void CpuScope_View_throwsWhenNoEditableSpan()
	{
		ICacheable<float> cacheable = new CoherenceOnlyCacheable();

		using (var scope = cacheable.CpuScope())
			scope.HasView.Should().BeFalse();

		Assert.Throws<InvalidOperationException>(() =>
		{
			using var scope = cacheable.CpuScope();
			_ = scope.View;
		});
	}

	[Fact]
	public void EditCpu_outsideCpuScope_throws()
	{
		var vector = CreateVector([1f, 2f, 3f]);
		ICacheable<float> cacheable = vector;

		Action act = () => cacheable.EditCpu(_ => { });

		act.Should().Throw<InvalidOperationException>();
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

	sealed class CoherenceOnlyCacheable : ICacheable<float>
	{
		int _depth;
		ResidenceField _residence;

		public bool SyncCpuEntered { get; private set; }
		public bool UpdateCacheCalled { get; private set; }

		public Residence Residence
		{
			get => _residence.Value;
			set => _residence.Value = value;
		}

		public uint LiveCount { get; private set; }
		public uint ID { get; set; }
		public long MemorySize => 0;

		public void DeCache() { }
		public void IncrementLiveCount() => LiveCount++;
		public void DecrementLiveCount() => LiveCount--;
		public void SyncCPU() => SyncCpuEntered = true;
		public void SyncCPU(ILGPU.Runtime.MemoryBuffer buffer) { }
		public ILGPU.Runtime.MemoryBuffer UpdateCache()
		{
			UpdateCacheCalled = true;
			Residence = Residence.InSync;
			return null!;
		}

		public ReadOnlySpan<float> RetrieveReadOnlySpan() => [];
		void ICacheable<float>.EditCpu(Action<Memory<float>> edit)
		{
			if (_depth == 0 || !ResidenceHelper.IsActiveCpu(Residence))
				throw new InvalidOperationException($"{nameof(ICacheable<float>.EditCpu)} requires an open CpuScope.");

			edit(Memory<float>.Empty);
		}
		public ILGPU.Runtime.MemoryBuffer UpdateCache(float[] array) => UpdateCache();

		public void EnterCpuScope()
		{
			ResidenceHelper.GuardCrossContext(Residence, enteringCpu: true);
			if (Interlocked.Increment(ref _depth) != 1)
				return;

			Residence current = Residence;
			if (!ResidenceHelper.IsActiveCpu(current))
				ResidenceScopeHelper.TransitionOrReconcile(this, current, Residence.ActiveCpu);
			SyncCPU();
		}

		public void ExitCpuScope(bool syncToGpu)
		{
			if (Interlocked.Decrement(ref _depth) != 0)
				return;

			ResidenceScopeHelper.TransitionOrReconcile(this, Residence.ActiveCpu, Residence.Cpu);
			if (syncToGpu)
				UpdateCache();
		}

		public void RollbackCpuScopeEnter() => ExitCpuScope(syncToGpu: false);

		public void SetResidence(Residence value) => _residence.Value = value;

		public bool TrySetResidence(Residence expected, Residence value) =>
			_residence.TryTransition(expected, value);
	}
}
