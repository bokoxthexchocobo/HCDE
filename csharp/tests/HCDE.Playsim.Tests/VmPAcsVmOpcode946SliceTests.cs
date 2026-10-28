using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class VmPAcsVmOpcode946SliceTests
{
    [Fact]
    public void LevelScalarFromFixed_ConvertsGravity() =>
        Assert.Equal(1.5, VmPAcsVmOpcode946Slice.LevelScalarFromFixed(98304));

    [Fact]
    public void SetsLevelGravity_ForGravityOpcodes()
    {
        Assert.True(VmPAcsVmOpcode946Slice.SetsLevelGravity((int)AcsPcode.SetGravity));
        Assert.False(VmPAcsVmOpcode946Slice.SetsLevelGravity((int)AcsPcode.Spawn));
    }

    [Fact]
    public void GiveActorInventoryIndices_ReadTopThreeSlots()
    {
        Assert.Equal(5, VmPAcsVmOpcode946Slice.GiveActorInventoryTidIndex(8));
        Assert.Equal(7, VmPAcsVmOpcode946Slice.GiveActorInventoryAmountIndex(8));
    }

    [Fact]
    public void SpawnDirectPcAfterRead_AdvancesSixWords() =>
        Assert.Equal(16, VmPAcsVmOpcode946Slice.SpawnDirectPcAfterRead(10));

    [Fact]
    public void ApplySpawnStackPop_ReducesByFive()
    {
        var sp = 8;
        VmPAcsVmOpcode946Slice.ApplySpawnStackPop(ref sp);
        Assert.Equal(3, sp);
    }

    [Fact]
    public void SpawnResultSlot_PointsAtFirstArg()
    {
        Assert.Equal(2, VmPAcsVmOpcode946Slice.SpawnResultSlot(8));
    }

    [Fact]
    public void ClearActorInventoryUsesWorldTarget_WhenTidZero() =>
        Assert.True(VmPAcsVmOpcode946Slice.ClearActorInventoryUsesWorldTarget(0));

    [Fact]
    public void GiveActorInventoryUsesActivator_WhenTidZero() =>
        Assert.True(VmPAcsVmOpcode946Slice.ActorInventoryUsesActivatorTid(0));

    [Fact]
    public void CheckActorInventoryStackIndices_UseTopTwoSlots()
    {
        Assert.Equal(6, VmPAcsVmOpcode946Slice.CheckActorInventoryTidIndex(8));
        Assert.Equal(7, VmPAcsVmOpcode946Slice.CheckActorInventoryTypeIndex(8));
    }

    [Fact]
    public void TriggersAirControlChanged_ForSetAirControlOpcodes()
    {
        Assert.True(VmPAcsVmOpcode946Slice.TriggersAirControlChanged((int)AcsPcode.SetAirControl));
        Assert.False(VmPAcsVmOpcode946Slice.TriggersAirControlChanged((int)AcsPcode.SetGravity));
    }

    [Fact]
    public void SpawnUsesNonForcedPlacement_MatchesNativeDoSpawn() =>
        Assert.True(VmPAcsVmOpcode946Slice.SpawnUsesNonForcedPlacement());

    [Fact]
    public void ApplyCheckActorInventoryStackPop_DropsOneSlot()
    {
        var sp = 4;
        VmPAcsVmOpcode946Slice.ApplyCheckActorInventoryStackPop(ref sp);
        Assert.Equal(3, sp);
    }
}
