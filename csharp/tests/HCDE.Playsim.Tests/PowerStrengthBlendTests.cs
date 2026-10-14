namespace HCDE.Playsim.Tests;

public class PowerStrengthBlendTests
{
    [Theory]
    [InlineData(0, 0u)]
    [InlineData(1, 0x3fff0000u)]
    [InlineData(8, 0x3fff0000u)]
    [InlineData(16, 0x3eff0000u)]
    [InlineData(1008, 0x00ff0000u)]
    [InlineData(1024, 0u)]
    public void DefaultStrengthBlendMatchesNativeTruncation(int tics, uint expected)
    {
        var player = new PlayerPawn { PowerStrengthTics = tics };
        Assert.Equal(expected, player.GetStrengthBlend());
        Assert.Equal(tics != 0, player.HasPowerStrength);
    }
}
