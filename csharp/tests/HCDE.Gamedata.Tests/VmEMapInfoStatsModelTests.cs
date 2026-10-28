namespace HCDE.Gamedata.Tests;

public class VmEMapInfoStatsModelTests
{
    [Fact]
    public void ShouldRecordSample_CapsAtEight()
    {
        Assert.True(VmEMapInfoStatsModel.ShouldRecordSample(7));
        Assert.False(VmEMapInfoStatsModel.ShouldRecordSample(8));
    }
}
