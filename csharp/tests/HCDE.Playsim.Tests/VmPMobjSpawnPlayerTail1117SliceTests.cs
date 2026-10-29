namespace HCDE.Playsim.Tests;

public class VmPMobjSpawnPlayerTail1117SliceTests
{
    [Fact]
    public void RunSpawnPlayerTail_RespawnClearsSoundTargetsAndDropsCeiling()
    {
        var ctx = new VmPMobjSpawnPlayerTail1117Slice.SpawnPlayerTailContext();
        VmPMobjSpawnPlayerTail1117Slice.RunSpawnPlayerTail(ctx, new VmPMobjSpawnPlayerTail1117Slice.SpawnPlayerInputs
        {
            State = VmPMobjSpawnPlayerTail1117Slice.PstReborn,
            HasOldActor = true,
            Multiplayer = true,
            Top = 200,
            CeilingZ = 128,
            Height = 56
        });
        Assert.Equal(0, ctx.Refire);
        Assert.Equal(-1, ctx.FixedLightLevel);
        Assert.True(ctx.FilterCoopRespawn);
        Assert.True(ctx.ClearSoundTargets);
        Assert.True(ctx.DropBelowCeiling);
        Assert.Equal(72, ctx.SpawnZ);
        Assert.True(ctx.SpawnTeleportFog);
    }

    [Fact]
    public void RunSpawnPlayerTail_SkipsScriptsForTemporaryPlayer()
    {
        var ctx = new VmPMobjSpawnPlayerTail1117Slice.SpawnPlayerTailContext();
        VmPMobjSpawnPlayerTail1117Slice.RunSpawnPlayerTail(ctx, new VmPMobjSpawnPlayerTail1117Slice.SpawnPlayerInputs
        {
            Flags = VmPMobjSpawnPlayerTail1117Slice.SpfTempPlayer,
            HasOldActor = true,
            State = VmPMobjSpawnPlayerTail1117Slice.PstEnter
        });
        Assert.False(ctx.SetupPsprites);
        Assert.False(ctx.StartEnterScripts);
    }

    [Fact]
    public void RejectMapThingEditorNumber_RejectsZeroAndMinusOne()
    {
        Assert.True(VmPMobjSpawnPlayerTail1117Slice.RejectMapThingEditorNumber(0));
        Assert.True(VmPMobjSpawnPlayerTail1117Slice.RejectMapThingEditorNumber(-1));
        Assert.False(VmPMobjSpawnPlayerTail1117Slice.RejectMapThingEditorNumber(3001));
    }

    [Fact]
    public void ApplyDemoChaseCam_SetsCheatWhenWatching()
    {
        var ctx = new VmPMobjSpawnPlayerTail1117Slice.SpawnPlayerTailContext();
        VmPMobjSpawnPlayerTail1117Slice.ApplyDemoChaseCam(ctx, demoPlayback: true, demoNew: false, chaseDemo: true);
        Assert.True((ctx.DemoCheats & VmPMobjSpawnPlayerTail1117Slice.CfChaseCam) != 0);
    }
}
