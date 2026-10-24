namespace HCDE.Gamedata.Tests;

public class VmDehackedRenderStyleLookupTests
{
    [Fact]
    public void FindStyle_ReturnsRegisteredStyle()
    {
        VmDehackedRenderStyleLookup.RegisterStyle(9, "Add");
        Assert.Equal(9, VmDehackedRenderStyleLookup.FindStyle("add"));
        Assert.Equal(-1, VmDehackedRenderStyleLookup.FindStyle("missing"));
    }
}
