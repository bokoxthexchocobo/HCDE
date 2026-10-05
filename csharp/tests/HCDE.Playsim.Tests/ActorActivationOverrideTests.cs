using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorActivationOverrideTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DirectThingActionCallsOverrideAfterUpdatingFlags(bool activate)
    {
        var sim = Room(); var actor = new RecordingActor { ActivationType = activate ? 1280 : 1536 };
        Assert.True(ThingActivation.Execute(sim, actor, 0, activate));
        Assert.Equal(activate, actor.Activated); Assert.Same(actor, actor.Trigger);
        Assert.Equal(activate ? 1536 : 1280, actor.FlagsAtCall);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SpecialActivationOverrideReceivesOriginalTriggerBeforeThingActs(bool activate)
    {
        var sim = Room(); var trigger = sim.Actors[0];
        var actor = new RecordingActor { ActivationType = (activate ? 1280 : 1536) | 1 };
        Assert.True(ActorSpecialActions.ActivateSpecial(sim, actor, trigger));
        Assert.Equal(activate, actor.Activated); Assert.Same(trigger, actor.Trigger);
        Assert.Equal((activate ? 1536 : 1280) | 1, actor.FlagsAtCall);
    }

    [Fact]
    public void CallingBaseActorMethodDoesNotConsumeSpecialFlags()
    {
        var sim = Room(); var actor = sim.Actors[0]; actor.ActivationType = 1280; actor.Dormant = true;
        actor.Activate(null); Assert.False(actor.Dormant); Assert.Equal(1280, actor.ActivationType);
        actor.Deactivate(null); Assert.True(actor.Dormant); Assert.Equal(1280, actor.ActivationType);
    }

    private sealed class RecordingActor : Actor
    {
        public bool? Activated { get; private set; }
        public Actor? Trigger { get; private set; }
        public int FlagsAtCall { get; private set; }
        public override void Activate(Actor? activator) => Record(activator, true);
        public override void Deactivate(Actor? activator) => Record(activator, false);
        private void Record(Actor? activator, bool activate)
        {
            Trigger = activator; Activated = activate; FlagsAtCall = ActivationType;
        }
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 }],
    });
}
