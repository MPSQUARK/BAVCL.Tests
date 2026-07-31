using BAVCL.Modules.Generators;

namespace BAVCL.Tests.Core.GeneratorsTests;

public class GeneratorsTests
{
	[Fact]
	public void Arange_Int_ReturnsExpectedSequence()
	{
		var values = GeneratorsModule.ArangeArray(0, 4, 1);

		values.Should().Equal([0, 1, 2, 3]);
	}

	[Fact]
	public void Arange_Double_NegativeRangeWithPositiveStep_AdjustsStep()
	{
		var values = GeneratorsModule.ArangeArray(0d, -4d, 1d);

		values.Should().Equal([0d, -1d, -2d, -3d]);
	}

	[Fact]
	public void Linspace_Int_IncludesBothEndpoints()
	{
		var values = GeneratorsModule.LinspaceArray(0, 8, 5);

		values.Should().HaveCount(5);
		values[0].Should().Be(0);
		values[^1].Should().Be(8);
		values.Should().Equal([0, 2, 4, 6, 8]);
	}

	[Fact]
	public void Linspace_Double_ReturnsEvenlySpacedValues()
	{
		var values = GeneratorsModule.LinspaceArray(0d, 4d, 3);

		values.Should().HaveCount(3);
		values[0].Should().Be(0d);
		values[^1].Should().Be(4d);
		values[1].Should().Be(2d);
	}

	[Fact]
	public void Arange_LazyEnumerable_MatchesMaterializedArray()
	{
		var lazy = GeneratorsModule.Arange(2, 8, 2).ToArray();

		lazy.Should().Equal([2, 4, 6]);
	}

	[Fact]
	public void Linspace_SinglePoint_ReturnsStartOnly()
	{
		var values = GeneratorsModule.LinspaceArray(7, 99, 1);

		values.Should().Equal([7]);
	}
}
