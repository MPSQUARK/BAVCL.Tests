namespace BAVCL.Tests;

[Binding]
public class GpuSteps
{
	private readonly ScenarioContext _scenarioContext;

	public GpuSteps(ScenarioContext scenarioContext)
	{
		_scenarioContext = scenarioContext;
	}

	[Given(@"I have a gpu")]
	public void GivenIhaveagpu()
	{
		GPU gpu = new();
		_scenarioContext.Add("gpu", gpu);
	}

	[Then(@"it should be cached on the GPU")]
	public void ThenItShouldBeCachedOnTheGPU()
	{
		GPU gpu = _scenarioContext.Get<GPU>("gpu");
		Vector vector = _scenarioContext.Get<Vector>("vector");

		vector.ID.Should().BeGreaterThan(0);
		gpu.IsStored(vector.ID).Should().BeTrue();
	}

	[Then(@"there should be (\d+) items? stored on the GPU")]
	public void ThenThereShouldBeItemsStoredOnTheGPU(int count)
	{
		GPU gpu = _scenarioContext.Get<GPU>("gpu");
		gpu.StoredIDs().Count.Should().Be(count);
	}

	[Then(@"it should not be cached on the GPU")]
	public void ThenItShouldNotBeCachedOnTheGPU()
	{
		GPU gpu = _scenarioContext.Get<GPU>("gpu");
		Vector vector = _scenarioContext.Get<Vector>("vector");

		vector.ID.Should().Be(0);
		gpu.IsStored(vector.ID).Should().BeFalse();
	}


}