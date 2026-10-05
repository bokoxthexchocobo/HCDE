namespace HCDE.Playsim.Tests;

public class ActorBeginPlayTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SpawnInvokesBeginPlayBeforeMapDormancy(bool mapDormant)
    {
        var actor = new RecordingActor { IsMonster = true };
        ThingActivation.InitializeSpawn(actor, mapDormant);
        Assert.Equal(mapDormant ? "begin,deactivate" : "begin", string.Join(",", actor.Events));
        Assert.Equal(mapDormant, actor.Dormant);
    }

    [Fact]
    public void BeginPlayOverrideCanReplaceClassDormancyHandling()
    {
        var actor = new RecordingActor { Dormant = true, IsMonster = true, SkipBase = true };
        ThingActivation.InitializeSpawn(actor, false);
        Assert.Equal("begin", string.Join(",", actor.Events));
        Assert.True(actor.Dormant);
    }

    [Fact]
    public void BaseBeginPlayUsesVirtualDeactivationAfterClearingDormancy()
    {
        var actor = new RecordingActor { Dormant = true, IsMonster = true };
        actor.BeginPlay();
        Assert.Equal("begin,deactivate", string.Join(",", actor.Events));
        Assert.True(actor.Dormant); Assert.False(actor.DormantAtDeactivate);
    }

    private sealed class RecordingActor : Actor
    {
        public List<string> Events { get; } = [];
        public bool SkipBase { get; init; }
        public bool DormantAtDeactivate { get; private set; }
        public override void BeginPlay()
        {
            Events.Add("begin"); if (!SkipBase) base.BeginPlay();
        }
        public override void Deactivate(Actor? activator)
        {
            Events.Add("deactivate"); DormantAtDeactivate = Dormant; base.Deactivate(activator);
        }
    }
}
