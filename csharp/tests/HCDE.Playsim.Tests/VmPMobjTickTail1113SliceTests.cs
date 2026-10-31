namespace HCDE.Playsim.Tests;

public class VmPMobjTickTail1113SliceTests
{
    [Fact]
    public void ShouldApplyTerrainFlat_WhenTouchingOrInWater()
    {
        Assert.True(VmPMobjTickTail1113Slice.ShouldApplyTerrainFlat(aboveFloor: false, waterLevel: 0));
        Assert.True(VmPMobjTickTail1113Slice.ShouldApplyTerrainFlat(aboveFloor: true, waterLevel: 1));
        Assert.False(VmPMobjTickTail1113Slice.ShouldApplyTerrainFlat(aboveFloor: true, waterLevel: 0));
    }

    [Fact]
    public void ResolveRespawnMonsterTics_ScalesNegativeMinimum()
    {
        Assert.Equal(70, VmPMobjTickTail1113Slice.ResolveRespawnMonsterTics(12, -2));
        Assert.Equal(8, VmPMobjTickTail1113Slice.ResolveRespawnMonsterTics(12, 8));
    }

    [Fact]
    public void TryNightmareRespawn_SpawnsWhenDiceAndCadenceAllow()
    {
        var ctx = new VmPMobjTickTail1113Slice.NightmareRespawnContext
        {
            Tics = -1,
            Flags5 = VmPMobjTickTail1113Slice.Mf5AlwaysRespawn,
            RespawnMonsters = 1,
            MoveCount = 0,
            LevelTime = 32,
            DiceRoll = 10,
            RespawnDice = 200
        };
        Assert.True(VmPMobjTickTail1113Slice.TryNightmareRespawn(ctx));
        Assert.True(ctx.Spawn);
    }

    [Fact]
    public void RunCheckNoDelay_ReturnsFalseWhenActionFreesActor()
    {
        var ctx = new VmPMobjTickTail1113Slice.CheckNoDelayContext
        {
            Flags7 = VmPMobjTickTail1113Slice.Mf7HandleNoDelay,
            StateNoDelay = true,
            ActionCalled = true,
            Euthanized = true
        };
        Assert.False(VmPMobjTickTail1113Slice.RunCheckNoDelay(ctx));
        Assert.True(ctx.ClearedHandleFlag);
    }

    [Fact]
    public void BuildSectorEnterActions_OrsFloorAndCeiling()
    {
        var act = VmPMobjTickTail1113Slice.BuildSectorEnterActions(true, true, false);
        Assert.Equal(
            VmPMobjTickTail1113Slice.SecEnter | VmPMobjTickTail1113Slice.SecHitFloor | VmPMobjTickTail1113Slice.SecHitCeiling,
            act);
    }
}
