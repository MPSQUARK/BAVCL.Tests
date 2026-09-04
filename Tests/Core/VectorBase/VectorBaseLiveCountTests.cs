using BAVCL.Core;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorBase;

public class VectorBaseLiveCountTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
	[Fact]
	public void GpuScope_TracksLiveCountOnModifiedPin()
	{
		var vector = CreateVector([1f, 2f, 3f]);

		using (GpuScope.Begin(vector))
			vector.LiveCount.Should().Be(1u);

		vector.LiveCount.Should().Be(0u);
	}

	[Fact]
	public void GpuScopeReadOnly_TracksLiveCountWithoutActiveGpu()
	{
		var vector = CreateVector([1f, 2f, 3f]);

		using (GpuScope.BeginReadOnly(vector))
			vector.LiveCount.Should().Be(1u);

		vector.LiveCount.Should().Be(0u);
	}
}
