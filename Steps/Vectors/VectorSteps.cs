using BAVCL.Tests.Models;

namespace BAVCL.Tests;

[Binding]
public class VectorSteps
{
	private readonly ScenarioContext _scenarioContext;

	public VectorSteps(ScenarioContext scenarioContext)
	{
		_scenarioContext = scenarioContext;
	}
	
	[When(@"I create the following (cached|non-cached) vector")]
	public void GivenICreateTheFollowingCachedVector(bool isCached, float[] array)
	{
		GPU gpu = _scenarioContext.Get<GPU>("gpu");
		Vector vector = new(gpu, array, cache: isCached);
		_scenarioContext.Add("vector", vector);
	}
	
	[When(@"I create a zeros vector of length (\d+)")]
	public void WhenICreateAZerosVectorOfLength(int length)
	{
		GPU gpu = _scenarioContext.Get<GPU>("gpu");
		Vector vector = new(gpu, length);
		_scenarioContext.Add("vector", vector);
	}
	
	[Then(@"the vector should have the following properties:")]
	public void ThenTheVectorShouldHaveTheFollowingProperties(VectorProperties vectorProperties)
	{
		Vector vector = _scenarioContext.Get<Vector>("vector");
		vectorProperties.Matches(vector);
	}

	[Then(@"the vector should be (1|2) dimensional")]
	public void ThenTheVectorShouldBeDimensional(int dimension)
	{
		Vector vector = _scenarioContext.Get<Vector>("vector");
		if (dimension == 1)
		{
			//vector.Columns.Should().Be(0);
			//vector.RowCount().Should().BeGreaterThan(0);
		}
			
		
	}
	
	[Then(@"the vector should have the following values on the CPU")]
	public void ThenTheVectorShouldHaveTheFollowingValuesOnTheCPUAndGPU(float[] vectorValues)
	{
		Vector vector = _scenarioContext.Get<Vector>("vector");
		vector.Value.Should().NotBeNullOrEmpty();
		vector.Value.Should().BeEquivalentTo(vectorValues);
	}
	
	[Then(@"the vector should have the following values on the GPU")]
	public void ThenTheVectorShouldHaveTheFollowingValuesOnTheGPU(float[] vectorValues)
	{
		Vector vector = _scenarioContext.Get<Vector>("vector");
		vector.SyncCPU();
		vector.Value.Should().NotBeNullOrEmpty();
		vector.Value.Should().BeEquivalentTo(vectorValues);
	}
	
	[StepArgumentTransformation]
	public bool IsCached(string isCached) => isCached == "cached" || string.IsNullOrEmpty(isCached);
	
}
