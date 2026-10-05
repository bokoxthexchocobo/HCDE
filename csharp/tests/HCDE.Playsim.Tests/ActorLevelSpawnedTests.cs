namespace HCDE.Playsim.Tests;

public class ActorLevelSpawnedTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MapSpawnClearsDroppedUnlessClassDefaultHasIt(bool defaultDropped)
    {
        var actor = new RecordingActor { SpawnDropped = defaultDropped };
        ThingActivation.InitializeSpawn(actor, false);
        Assert.Equal(defaultDropped, actor.Dropped);
        Assert.Equal(defaultDropped, actor.DroppedAtFlags);
        Assert.True(actor.FlagsCalled);
    }

    [Fact]
    public void DynamicSpawnRetainsBeginPlayDroppedAndSkipsMapFlags()
    {
        var actor = new RecordingActor();
        ThingActivation.InitializeSpawn(actor, false, mapSpawn: false);
        Assert.True(actor.Dropped); Assert.False(actor.FlagsCalled);
    }

    private sealed class RecordingActor : Actor
    {
        public bool FlagsCalled { get; private set; }
        public bool DroppedAtFlags { get; private set; }
        public override void BeginPlay() { base.BeginPlay(); Dropped = true; }
        public override void HandleSpawnFlags() { FlagsCalled = true; DroppedAtFlags = Dropped; base.HandleSpawnFlags(); }
    }
}
