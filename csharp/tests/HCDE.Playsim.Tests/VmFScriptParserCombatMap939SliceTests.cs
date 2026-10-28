namespace HCDE.Playsim.Tests;

public class VmFScriptParserCombatMap939SliceTests
{
    [Fact]
    public void HealModeFromArgc_RestoresDefaultWhenSingleArg() =>
        Assert.Equal(
            VmFScriptParserCombatMap939Slice.HealMode.RestoreDefault,
            VmFScriptParserCombatMap939Slice.HealModeFromArgc(1));

    [Fact]
    public void ObjDeadReturn_DetectsCorpseFlag() =>
        Assert.Equal(1, VmFScriptParserCombatMap939Slice.ObjDeadReturn(10, VmFScriptParserCombatMap939Slice.MfCorpse));

    [Fact]
    public void BuildMapThingExist_RejectsOutOfRange() =>
        Assert.Equal(0, VmFScriptParserCombatMap939Slice.BuildMapThingExist(9, 5, new object(), true).ReturnValue);

    [Fact]
    public void MapThingsCountReturn_UsesSize() =>
        Assert.Equal(12, VmFScriptParserCombatMap939Slice.MapThingsCountReturn(12));

    [Fact]
    public void BuildObjState_UsesTriggerForUnaryForm()
    {
        var trigger = new object();
        var invoke = VmFScriptParserCombatMap939Slice.BuildObjState(1, null, trigger, 3, 0);
        Assert.NotNull(invoke);
        Assert.Same(trigger, invoke!.Value.Actor);
        Assert.Equal("Missile", invoke.Value.StateName);
    }

    [Fact]
    public void ObjStateValid_RejectsZero() =>
        Assert.False(VmFScriptParserCombatMap939Slice.ObjStateValid(0));

    [Fact]
    public void SpawnMissileCanRun_RequiresBoth() =>
        Assert.False(VmFScriptParserCombatMap939Slice.SpawnMissileCanRun(new object(), null));
}
