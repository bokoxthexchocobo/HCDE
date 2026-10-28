namespace HCDE.Gamedata.Tests;

public class VmDehackedStructKeyApplyTests
{
    [Fact]
    public void TryApply_MatchesKey()
    {
        var value = 0;
        var keys = new[] { new VmDehackedStructKey { Name = "Speed", Setter = v => value = v } };
        Assert.True(VmDehackedStructKeyApply.TryApply(keys, "speed", 5));
        Assert.Equal(5, value);
    }
}
