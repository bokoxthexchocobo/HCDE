namespace HCDE.Client.Tests;

public class VmTimeUnitConversionsTests
{
    [Fact]
    public void MsAndNs_RoundTrip()
    {
        Assert.Equal(1_000_000UL, VmTimeUnitConversions.MsToNs(1));
        Assert.Equal(35UL, VmTimeUnitConversions.NsToMs(35_000_000));
    }

    [Fact]
    public void TicConversions_Match35Hz()
    {
        const double rate = 35;
        Assert.Equal(1_000_000_000UL, VmTimeUnitConversions.TicToNs(rate, rate));
        Assert.Equal(35, VmTimeUnitConversions.NsToTic(1_000_000_000, rate));
    }
}
