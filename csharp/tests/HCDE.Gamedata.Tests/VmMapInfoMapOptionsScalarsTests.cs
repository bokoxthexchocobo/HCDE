namespace HCDE.Gamedata.Tests;

public class VmMapInfoMapOptionsScalarsTests
{
    [Fact]
    public void ClampSkyMistYScale_Bounds()
    {
        Assert.Equal(544, VmMapInfoMapOptionsScalars.ClampSkyMistYScale(900));
    }

    [Fact]
    public void ClampWaveLimit_CapsAt3000()
    {
        Assert.Equal(3000, VmMapInfoMapOptionsScalars.ClampWaveLimit(5000));
    }

    [Fact]
    public void ParseMusicPair_PreservesOrder()
    {
        var (music, order) = VmMapInfoMapOptionsScalars.ParseMusicPair("D_RUNNIN", 2);
        Assert.Equal("D_RUNNIN", music);
        Assert.Equal(2, order);
    }
}
