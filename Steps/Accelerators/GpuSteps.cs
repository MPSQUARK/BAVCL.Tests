using System;
using BAVCL;
using TechTalk.SpecFlow;
	
	namespace MyNamespace
	{
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
				GPU gpu = new GPU();
				_scenarioContext.Add("gpu", gpu);
			}

		}
	}