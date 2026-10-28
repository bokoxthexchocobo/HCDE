namespace HCDE.Gamedata.Tests;

public class VmDehackedMbf21FlagNamesTests
{
    [Fact]
    public void KnownFlags_IncludesBoss()
    {
        Assert.Contains("BOSS", VmDehackedMbf21FlagNames.KnownFlags);
    }

    [Fact]
    public void ClearMbf21Defaults_ResetsGravity()
    {
        var d = new VmDehackedMbf21ActorDefaults { Gravity = 0.25, IsBoss = true };
        VmDehackedMbf21FlagNames.ClearMbf21Defaults(d);
        Assert.Equal(1, d.Gravity);
        Assert.False(d.IsBoss);
    }
}
