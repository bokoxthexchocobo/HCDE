namespace HCDE.Gamedata.Tests;

public class VmDehackedPatchParsCodePtrTests
{
    [Fact]
    public void TryParseParLine_MapFormat()
    {
        Assert.True(VmDehackedPatchParsCodePtr.TryParseParLine("02 180", out var map, out var par));
        Assert.Equal("MAP02", map);
        Assert.Equal(180, par);
    }

    [Fact]
    public void TryParseParLine_EpisodeFormat()
    {
        Assert.True(VmDehackedPatchParsCodePtr.TryParseParLine("1 1 120", out var map, out var par));
        Assert.Equal("E1M1", map);
        Assert.Equal(120, par);
    }

    [Fact]
    public void NormalizeActionSymbol_AddsPrefix()
    {
        Assert.Equal("A_FirePistol", VmDehackedPatchParsCodePtr.NormalizeActionSymbol("FirePistol"));
    }

    [Fact]
    public void RemapCPosAttack_DehackedName()
    {
        Assert.Equal("A_CPosAttackDehacked", VmDehackedPatchParsCodePtr.RemapCPosAttack("A_CPosAttack"));
    }

    [Fact]
    public void BubblePickupSpriteSort_MovesLeft()
    {
        var sprites = new[] { "ZZZZ", "ABCD", "WXYZ" };
        VmDehackedPatchParsCodePtr.BubblePickupSpriteSort(sprites, 1, "ABCD");
        Assert.Equal("ABCD", sprites[0]);
    }
}
