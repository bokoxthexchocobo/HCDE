namespace HCDE.Gamedata.Tests;

public class VmEMapInfoLumpResolveTests
{
    [Fact]
    public void TryBuildSiblingRelativePath_JoinsDirectory()
    {
        var rel = VmEMapInfoLumpResolve.TryBuildSiblingRelativePath("maps/emapinfo.txt", "extras/foo.edf");
        Assert.Equal("maps/extras/foo.edf", rel);
    }

    [Fact]
    public void ScaleFixedGravity_ZeroIsMax()
    {
        Assert.Equal(double.MaxValue, VmEMapInfoLumpResolve.ScaleFixedGravity(0));
    }
}
