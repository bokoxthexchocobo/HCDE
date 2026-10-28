namespace HCDE.Playsim.Tests;

public class VmPMobjActorSerializeTests
{
    [Fact]
    public void Serialize_SkipsUserLightsOnRollback()
    {
        var s = new VmPMobjActorSerialize.SerializeActorState { IsRollback = true };
        Assert.False(VmPMobjActorSerialize.ShouldSerializeUserLights(s));
        Assert.True(VmPMobjActorSerialize.RollbackSkipsUserLights(s));
    }

    [Fact]
    public void Serialize_PersistsGhostFlags()
    {
        var g = new VmPMobjActorSerialize.FlagPersistGate(0, VmPMobjActorSerialize.Mf2Thrughost, VmPMobjActorSerialize.Mf3Ghost, 0, 0);
        Assert.True(VmPMobjActorSerialize.ShouldPersistGhostBits(g));
    }

    [Fact]
    public void Serialize_ArgsOnlyWhenSpecial()
    {
        Assert.False(VmPMobjActorSerialize.SerializeArgsWhenSpecial(0));
        Assert.True(VmPMobjActorSerialize.SerializeArgsWhenSpecial(12));
        Assert.Equal(3, VmPMobjActorSerialize.ResolveArgsCount(12, 3, 5));
    }

    [Fact]
    public void Serialize_TailInvokesUserLightsWhenNotRollback()
    {
        var s = new VmPMobjActorSerialize.SerializeActorState { IsRollback = false, Special = 1, Writing = true };
        var terrain = false;
        var args = false;
        var lights = false;
        VmPMobjActorSerialize.ApplySerializeTail(
            s,
            () => terrain = true,
            () => args = true,
            () => lights = true);
        Assert.True(terrain);
        Assert.True(args);
        Assert.True(lights);
    }

    [Fact]
    public void Serialize_ThruSpeciesGate()
    {
        var g = new VmPMobjActorSerialize.FlagPersistGate(0, VmPMobjActorSerialize.Mf2Thruspecies, 0, VmPMobjActorSerialize.Mf6Thruspecies, 0);
        Assert.True(VmPMobjActorSerialize.ShouldPersistThruSpecies(g));
    }

    [Fact]
    public void Serialize_RunPipelineRecordsSuperAndTail()
    {
        var s = new VmPMobjActorSerialize.SerializeActorState { IsRollback = false, Special = 5, Writing = true };
        var core = false;
        var terrain = false;
        VmPMobjActorSerialize.RunSerializePipeline(
            s,
            () => core = true,
            () => terrain = true,
            () => { },
            null);
        Assert.True(core);
        Assert.True(terrain);
        Assert.Contains("Super", s.SerializedKeys);
    }

    [Fact]
    public void Serialize_WaterAndSpeciesPersist()
    {
        Assert.True(VmPMobjActorSerialize.WaterLevelPersist(1));
        Assert.True(VmPMobjActorSerialize.SpeciesPersist(2));
        Assert.False(VmPMobjActorSerialize.ThruBitsPersist(0));
    }
}
