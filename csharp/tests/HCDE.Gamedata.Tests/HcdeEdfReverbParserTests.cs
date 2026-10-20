namespace HCDE.Gamedata.Tests;

public class HcdeEdfReverbParserTests
{
    [Fact]
    public void TryApplyPropertyLine_ParsesRoomsize()
    {
        var rev = new HcdeEdfReverbDefinition();
        Assert.True(HcdeEdfReverbParser.TryApplyPropertyLine(rev, "roomsize 0.8"));
        Assert.Equal(0.8f, rev.Room);
    }
}
