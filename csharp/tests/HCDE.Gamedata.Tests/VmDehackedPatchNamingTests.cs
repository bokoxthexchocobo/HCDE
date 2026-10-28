namespace HCDE.Gamedata.Tests;

public class VmDehackedPatchNamingTests
{
    [Fact]
    public void SanitizePatchName_ReplacesUnsafeCharacters()
    {
        Assert.Equal("patch_name", VmDehackedPatchNaming.SanitizePatchName("patch:name"));
    }

    [Fact]
    public void IsCodePointerIndexInRange_RejectsOutOfBounds()
    {
        Assert.False(VmDehackedPatchNaming.IsCodePointerIndexInRange(5, 5, 3));
        Assert.True(VmDehackedPatchNaming.IsCodePointerIndexInRange(2, 5, 5));
    }
}
