namespace BAVCL.Tests.Core.Enums;

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
    public void FileTypes_ContainsCsvAndTxt()
    {
        Enum.GetNames<FileTypes>().Should().Contain(nameof(FileTypes.CSV), nameof(FileTypes.TXT));
    }
}
