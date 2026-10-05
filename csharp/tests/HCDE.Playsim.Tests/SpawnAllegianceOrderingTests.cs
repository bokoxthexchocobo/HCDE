namespace HCDE.Playsim.Tests;

public class SpawnAllegianceOrderingTests
{
    [Fact]
    public void DormantCallbackSeesAmbushBeforeMapFriendliness()
    {
        var actor = new RecordingActor { IsMonster = true, SpawnAmbush = true, SpawnFriendly = true };
        ThingActivation.InitializeSpawn(actor, true);
        Assert.True(actor.SawAmbush); Assert.False(actor.SawFriendly);
        Assert.True(actor.Ambush); Assert.True(actor.Friendly); Assert.True(actor.Dormant);
    }

    [Fact]
    public void MissingMapFlagsPreserveClassDefaults()
    {
        var actor = new Actor { Ambush = true, Friendly = true };
        ThingActivation.InitializeSpawn(actor, false);
        Assert.True(actor.Ambush); Assert.True(actor.Friendly);
    }

    [Fact]
    public void OverrideCanReplaceMapAmbushAndFriendliness()
    {
        var actor = new SkipFlagsActor { SpawnAmbush = true, SpawnFriendly = true };
        ThingActivation.InitializeSpawn(actor, false);
        Assert.False(actor.Ambush); Assert.False(actor.Friendly);
    }

    private sealed class RecordingActor : Actor
    {
        public bool SawAmbush { get; private set; }
        public bool SawFriendly { get; private set; }
        public override void Deactivate(Actor? activator)
        {
            SawAmbush = Ambush; SawFriendly = Friendly; base.Deactivate(activator);
        }
    }
    private sealed class SkipFlagsActor : Actor
    {
        public override void HandleSpawnFlags() { }
    }
}
