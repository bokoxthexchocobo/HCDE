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
    public void TryResolveMbfAlias_MatchesAlias()
    {
        var table = new[] { ("A_FireShotgun", "A_FireShotgun2") };
        Assert.True(VmDehackedPatchParsCodePtr.TryResolveMbfAlias("A_FireShotgun", table, out var resolved, out var wasMbf));
        Assert.True(wasMbf);
        Assert.Equal("A_FireShotgun2", resolved);
    }

    [Fact]
    public void BubblePickupSpriteSort_MovesLeft()
    {
        var sprites = new[] { "ZZZZ", "ABCD", "WXYZ" };
        VmDehackedPatchParsCodePtr.BubblePickupSpriteSort(sprites, 1, "ABCD");
        Assert.Equal("ABCD", sprites[0]);
    }

    [Fact]
    public void RunPatchParsNative_SetsPar()
    {
        var lines = new Queue<(int, string, string)>(new[]
        {
            (2, "par", "02 90"),
            (0, "", ""),
        });
        var applied = new List<(string, int)>();
        var result = VmDehackedPatchParsCodePtr.RunPatchParsNative(
            () => lines.Dequeue(),
            map => map == "MAP02",
            (map, par) => applied.Add((map, par)),
            _ => { },
            () => { },
            _ => { });
        Assert.Equal(0, result);
        Assert.Single(applied);
        Assert.Equal(("MAP02", 90), applied[0]);
    }

    [Fact]
    public void RunPatchCodePtrsNative_NullPointer()
    {
        var lines = new Queue<(int, string, string)>(new[]
        {
            (1, "Frame 3", "NULL"),
            (0, "", ""),
        });
        object? cleared = new object();
        VmDehackedPatchParsCodePtr.RunPatchCodePtrsNative(
            () => lines.Dequeue(),
            _ => cleared,
            _ => { },
            s => cleared = null,
            (_, _, _) => { },
            _ => null,
            (_, _) => { },
            (_, _) => { },
            Array.Empty<(string, string)>(),
            _ => { });
        Assert.Null(cleared);
    }
}
