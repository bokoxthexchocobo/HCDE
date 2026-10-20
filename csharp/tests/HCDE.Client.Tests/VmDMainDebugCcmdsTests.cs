namespace HCDE.Client.Tests;

public class VmDMainDebugCcmdsTests
{
    [Fact]
    public void ShouldIgnoreDebugRestart_DuringNetgame()
    {
        Assert.True(VmDMainDebugCcmds.ShouldIgnoreDebugRestart(true, false));
    }

    [Fact]
    public void StripTitleColorEscapes_RemovesBracketSequence()
    {
        var stripped = VmDMainDebugCcmds.StripTitleColorEscapes("Plain\x1c[cf00]Title");
        Assert.Equal("PlainTitle", stripped);
    }

    [Fact]
    public void FormatBrandedWindowTitle_PrefixesHcde()
    {
        Assert.Equal("HCDE - Doom", VmDMainDebugCcmds.FormatBrandedWindowTitle("Doom"));
    }

    [Fact]
    public void ShouldMarkFsDirEntryHidden_WhenShadowed()
    {
        Assert.True(VmDMainDebugCcmds.ShouldMarkFsDirEntryHidden(1, 0));
    }

    [Fact]
    public void BuildWindowTitle_CaseOne_UsesLevelAndGame()
    {
        Assert.Equal("HCDE - MAP01 - Doom", VmDMainDebugCcmds.BuildWindowTitle(1, "MAP01", "Doom"));
    }

    [Fact]
    public void MultiExecFileList_YieldsAllArgs()
    {
        Assert.Equal(new[] { "a.cfg", "b.cfg" }, VmDMainDebugCcmds.MultiExecFileList(new[] { "a.cfg", "b.cfg" }));
    }

    [Fact]
    public void GetCmdLineFilesStartIndex_OptionalStartsAtZero()
    {
        Assert.Equal(0, VmDMainDebugCcmds.GetCmdLineFilesStartIndex(true, 3));
    }
}
