using BAVCL.Core;

namespace BAVCL.Tests.Helpers;

static class VesselTestExtensions
{
	internal static Vessel<T> AsVessel<T>(this T target, GPU gpu) where T : class =>
		new(target, gpu);
}
