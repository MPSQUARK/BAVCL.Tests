using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.MemoryTests;

public class IsStoredTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
	[Fact]
	public void IsStored_ReturnsTrueForCachedVector()
	{
		var vector = CreateVector([1f, 2f, 3f]);

		Gpu.IsStored(vector.ID).Should().BeTrue();
	}

	[Fact]
	public void IsStored_ReturnsFalseAfterGcItem()
	{
		var vector = CreateVector([1f, 2f, 3f]);
		uint id = vector.ID;

		Gpu.GCItem(id);

		Gpu.IsStored(id).Should().BeFalse();
	}

	[Fact]
	public void IsStored_ReturnsFalseForUnknownId()
	{
		Gpu.IsStored(999_999u).Should().BeFalse();
	}
}
