namespace BAVCL.Tests.Core.Enums;

using BAVCL.Geometric;

public class OperationsEnumTests
{
    [Fact]
    public void Operations_ContainsExpectedValues()
    {
        Enum.GetNames<Operations>().Should().Contain(
        [
            nameof(Operations.add),
            nameof(Operations.subtract),
            nameof(Operations.multiply),
            nameof(Operations.divide),
            nameof(Operations.pow),
            nameof(Operations.magnitude),
            nameof(Operations.distance)
        ]);
    }

    [Fact]
    public void Axis_ContainsRowAndColumn()
    {
        Enum.GetNames<Axis>().Should().BeEquivalentTo(nameof(Axis.Row), nameof(Axis.Column));
    }

    [Fact]
    public void IoFormatters_ExposeSingletonDefault()
    {
        typeof(IFormatter<Vector>).IsAssignableFrom(typeof(JsonFormatter)).Should().BeTrue();
        typeof(IFormatter<Vector>).IsAssignableFrom(typeof(XmlFormatter)).Should().BeTrue();
        typeof(IFormatter<Vector3>).IsAssignableFrom(typeof(CsvFormatter)).Should().BeTrue();
        typeof(IFormatter<Mask>).IsAssignableFrom(typeof(CsvFormatter)).Should().BeTrue();
        JsonFormatter.Default.Should().BeSameAs(JsonFormatter.Default);
        JsonFormatter.Default.Extension.Should().Be(".json");
        CsvFormatter.Default.Extension.Should().Be(".csv");
        TxtFormatter.Default.Extension.Should().Be(".txt");
        XmlFormatter.Default.Extension.Should().Be(".xml");
    }
}
