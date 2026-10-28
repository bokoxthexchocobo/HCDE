namespace HCDE.Gamedata.Tests;

public class VmMapInfoMapOptionsPicsTests
{
    [Fact]
    public void ApplyInterPic_SetsLabel()
    {
        var draft = new VmMapInfoLevelOptionsDraft();
        VmMapInfoMapOptionsPics.ApplyInterPic(draft, "INTERPIC");
        Assert.Equal("INTERPIC", draft.MapLabel);
    }

    [Fact]
    public void ApplyExitPic_SetsNextMap()
    {
        var draft = new VmMapInfoLevelOptionsDraft();
        VmMapInfoMapOptionsPics.ApplyExitPic(draft, "EXITPIC");
        Assert.Equal("EXITPIC", draft.NextMap);
    }
}
