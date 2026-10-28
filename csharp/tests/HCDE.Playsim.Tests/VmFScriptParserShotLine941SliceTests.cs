namespace HCDE.Playsim.Tests;

public class VmFScriptParserShotLine941SliceTests
{
    [Fact]
    public void ResolveSpawnShot2Source_UsesTriggerWhenSecondArgNegativeInt()
    {
        var trigger = new object();
        var resolved = VmFScriptParserShotLine941Slice.ResolveSpawnShot2Source(true, -1, new object(), trigger);
        Assert.Same(trigger, resolved);
    }

    [Fact]
    public void SpawnShot2ClearsMissileOnFailedSpawn_WhenCheckFails() =>
        Assert.True(VmFScriptParserShotLine941Slice.SpawnShot2ClearsMissileOnFailedSpawn(false));

    [Fact]
    public void KillInSectorShouldMassacre_RequiresMonsterAndTag() =>
        Assert.False(VmFScriptParserShotLine941Slice.KillInSectorShouldMassacre(false, true));

    [Fact]
    public void SetLineTriggerTagFromArgc_DefaultsZero() =>
        Assert.Equal(0, VmFScriptParserShotLine941Slice.SetLineTriggerTagFromArgc(2, 9));

    [Fact]
    public void MergeSetLineTriggerFlags_PreservesActivationBits()
    {
        var mask = VmFScriptParserShotLine941Slice.LineFlagPreserveMask;
        var merged = VmFScriptParserShotLine941Slice.MergeSetLineTriggerFlags(mask, 0xFF00);
        Assert.Equal(mask, merged & mask);
    }

    [Fact]
    public void FillRunLineSpecialArgs_PadsMissingWithZero()
    {
        Span<int> args = stackalloc int[5];
        VmFScriptParserShotLine941Slice.FillRunLineSpecialArgs(2, [11, 22], args);
        Assert.Equal(11, args[0]);
        Assert.Equal(22, args[1]);
        Assert.Equal(0, args[4]);
    }

    [Fact]
    public void BuildSaveCurrentScript_ReturnsNullWithoutThinker() =>
        Assert.Null(VmFScriptParserShotLine941Slice.BuildSaveCurrentScript(null, new object(), new object(), 0));
}
