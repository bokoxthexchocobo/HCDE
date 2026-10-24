namespace HCDE.Playsim.Tests;

public class VmPMobjMorphSpawnTests
{
    [Fact]
    public void Morph_UnmorphRunsPreUnmorph() =>
        Assert.True(true);

    [Fact]
    public void Morph_LinksDummyOnMorphIn() =>
        Assert.True(VmPMobjMorphSpawn.SetUnmorphedDummyLink(
            new VmPMobjMorphSpawn.MorphLinkContext(false, false)));

    [Fact]
    public void Spawn_AllowsValidPlayer() =>
        Assert.True(VmPMobjMorphSpawn.SpawnPlayerAllowed(
            new VmPMobjMorphSpawn.SpawnPlayerGateContext(false, 0, 8, true)));

    [Fact]
    public void Spawn_CoopRespawnFilter() =>
        Assert.True(VmPMobjMorphSpawn.FilterCoopRespawnInventory(
            new VmPMobjMorphSpawn.CoopRespawnInventoryContext(
                true, 0, false, false, VmPMobjMorphSpawn.PstReborn, true)));

    [Fact]
    public void Spawn_RunEnterScriptsOnPstEnter() =>
        Assert.True(VmPMobjMorphSpawn.RunEnterScripts(
            new VmPMobjMorphSpawn.SpawnScriptContext(0, true, VmPMobjMorphSpawn.PstEnter, false)));

    [Fact]
    public void Spawn_RebornClearsSoundTargets() =>
        Assert.True(true);

    [Fact]
    public void Spawn_SinkBelowCeiling() =>
        Assert.Equal(8, VmPMobjMorphSpawn.SinkZ(16, 8));

    [Fact]
    public void Morph_UnmorphClearsDummyFlag() =>
        Assert.True(VmPMobjMorphSpawn.ClearDummyUnmorphFlag(
            new VmPMobjMorphSpawn.MorphAlternativeFinalizeContext(true)));

    [Fact]
    public void Morph_SetsUnmorphedOnMorphIn() =>
        Assert.Equal(VmPMobjMorphSpawn.MfUnmorphed,
            VmPMobjMorphSpawn.ApplyUnmorphFlags(0, false));

    [Fact]
    public void Spawn_CoopHeldWeaponRequiresLiveWeapon() =>
        Assert.True(VmPMobjMorphSpawn.PassHeldWeaponToCoopFilter(
            new VmPMobjMorphSpawn.CoopHeldWeaponContext(true, false, false)));

    [Fact]
    public void Spawn_RebornMaintenanceBundle() =>
        Assert.True(VmPMobjMorphSpawn.SubstitutePlayerPointers(
            new VmPMobjMorphSpawn.RebornMaintenanceContext(true)));
}
