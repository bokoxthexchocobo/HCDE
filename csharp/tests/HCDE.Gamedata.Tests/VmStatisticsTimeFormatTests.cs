namespace HCDE.Gamedata.Tests;

public class VmStatisticsTimeFormatTests
{
    [Fact]
    public void Seconds_ExtractsFromTics()
    {
        Assert.Equal(1, VmStatisticsTimeFormat.Seconds(35));
    }
}
