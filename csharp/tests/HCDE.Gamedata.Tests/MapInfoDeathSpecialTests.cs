namespace HCDE.Gamedata.Tests;

public class MapInfoDeathSpecialTests
{
    [Theory]
    [InlineData("map 1 \"Test\" { }", true)]
    [InlineData("map MAP01 \"Test\" { }", false)]
    [InlineData("map MAP01 \"Test\" { activateowndeathspecials }", true)]
    [InlineData("map 1 \"Test\" { killeractivatesdeathspecials }", false)]
    [InlineData("map MAP01 \"Test\" { activateowndeathspecials killeractivatesdeathspecials }", false)]
    [InlineData("map MAP01 \"Test\" { killeractivatesdeathspecials activateowndeathspecials }", true)]
    public void DeathSpecialPolicyUsesNativeDefaultsAndLastFlag(string text, bool own)
    {
        Assert.True(MapInfoParser.TryParse(text, out var info, out var error), error);
        Assert.Equal(own, info.FindMap("MAP01")!.ActivateOwnDeathSpecials);
    }
}
