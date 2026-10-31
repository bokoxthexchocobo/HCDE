namespace HCDE.Playsim.Tests;

public class VmPMobjConstructActor1115SliceTests
{
    [Fact]
    public void RunUpdateWaterLevel_PlaysDiveOnlyForPlayersAndReturnsFalse()
    {
        var dived = false;
        var result = VmPMobjConstructActor1115Slice.RunUpdateWaterLevel(
            doSplash: false,
            hasPlayer: true,
            splash: () => { },
            refreshDepth: () => { },
            playDive: () => dived = true);
        Assert.True(dived);
        Assert.False(result);
    }

    [Fact]
    public void RunConstructActor_ZerosReactionForAlwaysFastMonsters()
    {
        var ctx = new VmPMobjConstructActor1115Slice.ConstructContext
        {
            Flags3 = VmPMobjConstructActor1115Slice.Mf3IsMonster,
            Flags5 = VmPMobjConstructActor1115Slice.Mf5AlwaysFast,
            MaxPlayers = 4,
            Rng = 5,
            ServerTics = 3
        };
        VmPMobjConstructActor1115Slice.RunConstructActor(ctx);
        Assert.Equal(0, ctx.ReactionTime);
        Assert.Equal(1, ctx.LastLookPlayer);
        Assert.Equal(0, ctx.TidToHate);
    }

    [Fact]
    public void RunConstructActor_PlacesOnCeilingAndCopiesMapFloor()
    {
        var ctx = new VmPMobjConstructActor1115Slice.ConstructContext
        {
            ZMode = VmPMobjConstructActor1115Slice.SpawnZMode.OnCeiling,
            FloorZ = 0,
            CeilingZ = 128,
            Height = 56,
            SpawningMapThing = true
        };
        VmPMobjConstructActor1115Slice.RunConstructActor(ctx);
        Assert.Equal(72, ctx.SpawnZ);
        Assert.True(ctx.CopySectorFloorCeiling);
        Assert.False(ctx.CopySpawnZ);
    }

    [Fact]
    public void RunConstructActor_FloatRandomOnlyWhenSpaceExceeds48()
    {
        var tight = new VmPMobjConstructActor1115Slice.ConstructContext
        {
            ZMode = VmPMobjConstructActor1115Slice.SpawnZMode.FloatRandom,
            FloorZ = 0,
            CeilingZ = 80,
            Height = 56
        };
        VmPMobjConstructActor1115Slice.RunConstructActor(tight);
        Assert.False(tight.ApplyFloatRandom);

        var roomy = new VmPMobjConstructActor1115Slice.ConstructContext
        {
            ZMode = VmPMobjConstructActor1115Slice.SpawnZMode.FloatRandom,
            FloorZ = 0,
            CeilingZ = 200,
            Height = 56
        };
        VmPMobjConstructActor1115Slice.RunConstructActor(roomy);
        Assert.True(roomy.ApplyFloatRandom);
    }
}
