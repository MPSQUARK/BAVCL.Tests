using static BAVCL.Core.Enums;

namespace BAVCL.Tests;

[Binding]
public class AccessSteps
{
	private readonly ScenarioContext _scenarioContext;

	public AccessSteps(ScenarioContext scenarioContext)
	{
		_scenarioContext = scenarioContext;
	}

	[When(@"I access (.*) (\d+) from the vector with alias (.*)")]
	public void WhenIAccessIndexFromTheVectorWithAlias(string accessType, int position, string alias)
	{
		Vector vector = _scenarioContext.Get<Vector>(alias);
		switch (accessType)
		{
			case "index":
				_scenarioContext.Add("accessedValue", vector[position]);
				break;
			case "column":
				_scenarioContext.Add("accessedValue", vector.GetSliceAsArray(position, Axis.Column));
				break;
			case "row":
				_scenarioContext.Add("accessedValue", vector.GetSliceAsArray(position, Axis.Row));
				break;
			default:
				throw new ArgumentException($"Unknown access type: {accessType}");
		}

	}

	[Then(@"I should get the value (\d+)")]
	public void ThenIShouldGetTheValue(int expectedValue)
	{
		float accessedValue = _scenarioContext.Get<float>("accessedValue");
		accessedValue.Should().Be(expectedValue);
	}

	[Then(@"I should get the following values")]
	public void ThenIShouldGetTheFollowingValues(float[] expectedValues)
	{
		float[] accessedValues = _scenarioContext.Get<float[]>("accessedValue");
		accessedValues.Should().BeEquivalentTo(expectedValues);
	}


}
