namespace HCDE.Playsim.Tests;

public class SpawnHookLifetimeTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BeginPlaySeesDecodedMapDormancy(bool dormant)
    {
        var actor = new RecordingActor();
        ThingActivation.InitializeSpawn(actor, dormant);
        Assert.Equal(dormant, actor.ObservedDormancy); Assert.True(actor.FlagsCalled);
    }

    [Fact]
    public void DestroyedDuringBeginPlaySkipsFlagHandling()
    {
        var actor = new RecordingActor { RemoveAtBegin = true, SpawnFriendly = true, SpawnAmbush = true };
        ThingActivation.InitializeSpawn(actor, true);
        Assert.True(actor.Destroyed); Assert.False(actor.FlagsCalled);
        Assert.False(actor.Friendly); Assert.False(actor.Ambush);
    }

    private sealed class RecordingActor : Actor
    {
        public bool RemoveAtBegin { get; init; }
        public bool ObservedDormancy { get; private set; }
        public bool FlagsCalled { get; private set; }
        public override void BeginPlay()
        {
            ObservedDormancy = SpawnDormant;
            if (RemoveAtBegin) Destroy(); else base.BeginPlay();
        }
        public override void HandleSpawnFlags() { FlagsCalled = true; base.HandleSpawnFlags(); }
    }
}
