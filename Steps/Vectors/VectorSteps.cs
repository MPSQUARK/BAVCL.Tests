using System;
using BAVCL.Tests.Models;
using TechTalk.SpecFlow;
	
	namespace MyNamespace
	{
		[Binding]
		public class VectorSteps
		{
			private readonly ScenarioContext _scenarioContext;
	
			public VectorSteps(ScenarioContext scenarioContext)
			{
				_scenarioContext = scenarioContext;
			}
			
			[When(@"I create the following cached vector")]
			public void GivenICreateTheFollowingCachedVector(float[] array)
			{
				GPU gpu = _scenarioContext.Get<GPU>("gpu");
				Vector vector = new(gpu, array);
				_scenarioContext.Add("vector", vector);
			}
			
			[Then(@"the vector should have the following properties:")]
			public void ThenTheVectorShouldHaveTheFollowingProperties(VectorProperties vectorProperties)
			{
				Vector vector = _scenarioContext.Get<Vector>("vector");
				vectorProperties.Matches(vector);
			}
			
			
		}
	}