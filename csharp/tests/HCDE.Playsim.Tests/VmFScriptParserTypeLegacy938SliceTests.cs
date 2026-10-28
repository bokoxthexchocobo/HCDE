namespace HCDE.Playsim.Tests;

public class VmFScriptParserTypeLegacy938SliceTests
{
    [Fact]
    public void StringCoerceValue_UsesDirectString()
    {
        var s = VmFScriptParserTypeLegacy938Slice.StringCoerceValue(
            VmFScriptParser.SvtString, "hi", "ignored");
        Assert.Equal("hi", s);
    }

    [Fact]
    public void SpawnExplosionZ_UsesFloorWhenArgMissing()
    {
        Assert.Equal(12, VmFScriptParserTypeLegacy938Slice.SpawnExplosionZ(3, 99, 12));
    }

    [Fact]
    public void BuildSpawnExplosion_ReturnsZeroWhenSpawnFails()
    {
        var invoke = VmFScriptParserTypeLegacy938Slice.BuildSpawnExplosion(
            3, 1, 2, 0, 5, null, 7);
        Assert.NotNull(invoke);
        Assert.Equal(0, invoke!.Value.ReturnState);
    }

    [Fact]
    public void RadiusAttackCanRun_RequiresBothActors()
    {
        Assert.False(VmFScriptParserTypeLegacy938Slice.RadiusAttackCanRun(new object(), null));
    }

    [Fact]
    public void BuildSetObjPosition_DefaultsYAndZ()
    {
        var invoke = VmFScriptParserTypeLegacy938Slice.BuildSetObjPosition(
            2, new object(), 10, 0, 0, 20, 30);
        Assert.NotNull(invoke);
        Assert.Equal(20, invoke!.Value.Y);
        Assert.Equal(30, invoke.Value.Z);
    }

    [Fact]
    public void TestLocationActor_FallsBackToTrigger()
    {
        var trigger = new object();
        Assert.Same(trigger, VmFScriptParserTypeLegacy938Slice.TestLocationActor(0, null, trigger));
    }

    [Fact]
    public void TestLocationReturn_ConvertsBool()
    {
        Assert.Equal(1, VmFScriptParserTypeLegacy938Slice.TestLocationReturn(true));
    }
}
