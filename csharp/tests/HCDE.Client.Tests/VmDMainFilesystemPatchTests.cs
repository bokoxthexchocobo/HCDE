namespace HCDE.Client.Tests;

public class VmDMainFilesystemPatchTests
{
    [Fact]
    public void ShouldRenameBlodToBlud_WhenNotDoomChex()
    {
        Assert.True(VmDMainRenameSpritesIo.ShouldRenameBlodToBlud(0, VmDMainRenameSpritesIo.BlodId));
        Assert.Equal(VmDMainRenameSpritesIo.BludId, VmDMainRenameSpritesIo.RenameBlodSprite(VmDMainRenameSpritesIo.BlodId));
    }

    [Fact]
    public void ShouldClearLump_WhenListedForDeletion()
    {
        var list = new[] { "FOOBAR" };
        Assert.True(VmDMainRenameSpritesIo.ShouldClearDeletableLump(
            1, 1, 2, 1, 3, "FOOBAR", list));
    }

    [Fact]
    public void ShouldRenameBigFont_ToCanonicalName()
    {
        Assert.True(VmDMainRenameSpritesIo.ShouldRenameBigFontLump("DBIGFONT", "DBIGFONT"));
        Assert.Equal("BIGFONT", VmDMainRenameSpritesIo.BigFontLumpName());
    }

    [Fact]
    public void MatchNerveSizeIndex_FindsUnityBuild()
    {
        Assert.Equal(1, VmDMainRenameNerve.MatchNerveSizeIndex(3821966));
    }

    [Fact]
    public void ApplyMap00Rename_ShiftsDigitsLikeNative()
    {
        var chars = "MAP01\0\0\0".ToCharArray();
        VmDMainRenameNerve.ApplyMap00Rename(chars);
        Assert.Equal('L', chars[4]);
        Assert.Equal('0', chars[5]);
        Assert.Equal('1', chars[6]);
    }

    [Fact]
    public void ExtraLumpDeletionRange_Covers299Entries()
    {
        var (start, end) = VmDMainFixMacHexen.ExtraLumpDeletionRange(1000);
        Assert.Equal(702, start);
        Assert.Equal(1000, end);
        Assert.Equal(299, end - start + 1);
    }

    [Fact]
    public void IsKnownMacHexenIwadSize_IncludesRetail()
    {
        Assert.True(VmDMainFixMacHexen.IsKnownMacHexenIwadSize(VmDMainFixMacHexen.FullSize));
    }
}
