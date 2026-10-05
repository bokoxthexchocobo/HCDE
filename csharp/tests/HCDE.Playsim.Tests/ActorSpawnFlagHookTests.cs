namespace HCDE.Playsim.Tests;

public class ActorSpawnFlagHookTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SpawnCallsFlagHookAfterBeginPlay(bool dormant)
    {
        var actor = new RecordingActor { IsMonster = true };
        ThingActivation.InitializeSpawn(actor, dormant);
        Assert.Equal(dormant ? "begin,flags,deactivate" : "begin,flags", string.Join(",", actor.Events));
        Assert.Equal(dormant, actor.Dormant);
    }

    [Fact]
    public void OverrideCanReplaceMapDormancyHandling()
    {
        var actor = new RecordingActor { IsMonster = true, SkipBase = true };
        ThingActivation.InitializeSpawn(actor, true);
        Assert.False(actor.Dormant); Assert.Equal("begin,flags", string.Join(",", actor.Events));
    }

    private sealed class RecordingActor : Actor
    {
        public List<string> Events { get; } = [];
        public bool SkipBase { get; init; }
        public override void BeginPlay() { Events.Add("begin"); base.BeginPlay(); }
        public override void HandleSpawnFlags() { Events.Add("flags"); if (!SkipBase) base.HandleSpawnFlags(); }
        public override void Deactivate(Actor? activator) { Events.Add("deactivate"); base.Deactivate(activator); }
    }
}
