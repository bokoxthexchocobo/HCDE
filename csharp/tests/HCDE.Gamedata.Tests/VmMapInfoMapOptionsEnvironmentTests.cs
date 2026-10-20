namespace HCDE.Gamedata.Tests;

public class VmMapInfoMapOptionsEnvironmentTests
{
    [Fact]
    public void ParseCdIdHex_ReadsHex()
    {
        Assert.Equal(0x12, VmMapInfoMapOptionsEnvironment.ParseCdIdHex("12"));
    }

    [Fact]
    public void ClampWallShade_DividesByTwo()
    {
        Assert.Equal(64, VmMapInfoMapOptionsEnvironment.ClampWallShade(128));
    }

    [Fact]
    public void ApplyEvenLighting_ClearsSkySpeeds()
    {
        var draft = new VmMapInfoLevelOptionsDraft { SkySpeed1 = 1, SkySpeed2 = 2 };
        VmMapInfoMapOptionsEnvironment.ApplyEvenLighting(draft);
        Assert.Equal(0, draft.SkySpeed1);
        Assert.Equal(0, draft.SkySpeed2);
    }

    [Fact]
    public void ApplyNoGravity_ReturnsMax()
    {
        Assert.Equal(double.MaxValue, VmMapInfoMapOptionsEnvironment.ApplyNoGravity());
    }
}
