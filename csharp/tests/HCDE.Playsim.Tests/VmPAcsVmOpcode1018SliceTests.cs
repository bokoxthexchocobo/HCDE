namespace HCDE.Playsim.Tests;

public class VmPAcsVmOpcode1018SliceTests
{
    [Fact]
    public void FormatPrintFixed_UsesInvariantDouble()
    {
        Assert.Equal("1.5", VmPAcsVmOpcode1018Slice.FormatPrintFixed(98304));
    }

    [Fact]
    public void TryResolvePrintNameSpecial_MapsNegativeKinds()
    {
        var ctx = new VmPAcsVmOpcode1018Slice.PrintNameContext
        {
            LevelName = "MAP01",
            MapName = "map01",
            NextMap = "map02",
            NextSecretMap = "sec01",
            SkillName = "Hurt Me Plenty",
        };
        Assert.True(VmPAcsVmOpcode1018Slice.TryResolvePrintNameSpecial(
            VmPAcsCallfunc925Slice.PrintNameLevelName, ctx, out var level));
        Assert.Equal("MAP01", level);
        Assert.True(VmPAcsVmOpcode1018Slice.TryResolvePrintNameSpecial(
            VmPAcsCallfunc925Slice.PrintNameSkill, ctx, out var skill));
        Assert.Equal("Hurt Me Plenty", skill);
    }

    [Fact]
    public void ResolvePrintNamePlayer_UsesActivatorWhenIndexZero()
    {
        var ctx = new VmPAcsVmOpcode1018Slice.PrintNameContext
        {
            ActivatorPlayer = new object(),
            PlayerDisplayName = _ => "DoomGuy",
        };
        Assert.Equal("DoomGuy", VmPAcsVmOpcode1018Slice.ResolvePrintNamePlayer(0, ctx));
    }

    [Fact]
    public void ResolvePrintNamePlayer_FormatsUnknownPlayerSlot()
    {
        var ctx = new VmPAcsVmOpcode1018Slice.PrintNameContext
        {
            ResolvePlayerIndex = _ => false,
        };
        Assert.Equal("Player 3", VmPAcsVmOpcode1018Slice.ResolvePrintNamePlayer(3, ctx));
    }

    [Fact]
    public void TryBuildPrintCharArray_AppendsUntilNul()
    {
        var stack = new List<int> { 0, 0 };
        var sp = 2;
        Assert.True(VmPAcsVmOpcode1018Slice.TryBuildPrintCharArray(
            stack,
            ref sp,
            ranged: false,
            readChar: (_, off) => off switch { 0 => (int)'A', 1 => (int)'B', _ => 0 },
            out var text));
        Assert.Equal("AB", text);
        Assert.Equal(0, sp);
    }

    [Fact]
    public void AppendCharArrayRun_RespectsCapacity()
    {
        var text = VmPAcsVmOpcode1018Slice.AppendCharArrayRun((_, off) => (int)('a' + off), 0, 0, 2);
        Assert.Equal("ab", text);
    }
}
