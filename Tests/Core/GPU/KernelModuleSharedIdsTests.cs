using System;
using System.Linq;
using BAVCL.Source.Core.Enums;

namespace BAVCL.Tests.Core.GPU;

public sealed class KernelModuleSharedIdsTests
{
	[Fact]
	public void KernelDomain_AndSharedModule_EncodedIds_DoNotOverlap()
	{
		int maxDomain = Enum.GetValues<KernelDomain>().Cast<int>().Max();
		int sharedBase = Enum.GetValues<KernelDomainSharedModule>().Cast<int>().Min();
		maxDomain.Should().BeLessThan(sharedBase);

		int minShared = Enum.GetValues<KernelDomainSharedModule>()
			.Cast<int>()
			.Min();

		minShared.Should().BeGreaterThanOrEqualTo(sharedBase);
	}
}
