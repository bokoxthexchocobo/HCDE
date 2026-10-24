namespace HCDE.Client.Tests;

public class VmCmdLibCompleteTests
{
    [Fact]
    public void StrBin_UnescapesNativeSequences()
    {
        Assert.Equal("a\n", VmCmdLibStrBin.Unescape("a\\n"));
    }

    [Fact]
    public void CheckWildcards_MatchesQuestionMark()
    {
        Assert.True(VmCmdLibUtilities.CheckWildcards("d?g", "dog"));
    }

    [Fact]
    public void IsAbsPath_DetectsRootAndDrive()
    {
        Assert.True(VmCmdLibUtilities.IsAbsPath("/tmp/x"));
        if (OperatingSystem.IsWindows()) Assert.True(VmCmdLibUtilities.IsAbsPath("C:\\game"));
    }

    [Fact]
    public void NormalizeFileName_CollapsesDotDot()
    {
        Assert.Equal("textures/brick.png", VmCmdLibUtilities.NormalizeFileName("textures/../textures/./brick.png"));
    }
}
