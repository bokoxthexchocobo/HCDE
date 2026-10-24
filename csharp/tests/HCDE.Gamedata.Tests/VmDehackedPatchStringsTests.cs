namespace HCDE.Gamedata.Tests;

public class VmDehackedPatchStringsTests
{
    [Fact]
    public void FoldContinuationLines_JoinsBackslash()
    {
        var folded = VmDehackedPatchStrings.FoldContinuationLines(["line\\", "two"]);
        Assert.Equal("linetwo", folded);
    }

    [Fact]
    public void NormalizeStringTableKey_AliasesRedSkull()
    {
        Assert.Equal("GOTREDSKUL", VmDehackedPatchStrings.NormalizeStringTableKey("GOTREDSKULL"));
    }
}
