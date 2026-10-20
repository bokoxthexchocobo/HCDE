namespace HCDE.Gamedata.Tests;

public class VmMapInfoOptionDispatchTests
{
    [Fact]
    public void TryDispatchOption_MatchesName()
    {
        var opts = new[] { new VmMapInfoOptionRegistration { Name = "sky1" } };
        Assert.True(VmMapInfoOptionDispatch.TryDispatchOption("SKY1", true, opts, out var m));
        Assert.Equal("sky1", m!.Name);
    }
}
