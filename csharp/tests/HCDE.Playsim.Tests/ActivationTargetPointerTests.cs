using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActivationTargetPointerTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void ThingTargetsSupportsEveryRepresentedPointerStorage(int kind)
    {
        var sim = Room(); var trigger = sim.AddBot(300, 0, 3001); var thing = Subject(sim, kind);
        thing.ActivationType = 2; thing.MasterId = trigger.Id; thing.TracerId = trigger.Id;
        Assert.False(ActorSpecialActions.ActivateSpecial(sim, thing, trigger));
        Assert.Equal(trigger.Id, thing.TargetId); Assert.Equal(trigger.Id, thing.MasterId); Assert.Equal(trigger.Id, thing.TracerId);
        if (thing is ProjectileActor missile) Assert.Same(trigger, missile.Owner);
        var saved = SimSavegame.Write(sim); thing.SetTargetPointer(null); SimSavegame.Apply(sim, saved);
        Assert.Equal(trigger.Id, thing.TargetId); Assert.Equal(saved, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void TriggerTargetsSupportsEveryRepresentedPointerStorage(int kind)
    {
        var sim = Room(); var trigger = Subject(sim, kind); var thing = sim.AddBot(300, 0, 3001);
        thing.ActivationType = 4;
        Assert.False(ActorSpecialActions.ActivateSpecial(sim, thing, trigger)); Assert.Equal(thing.Id, trigger.TargetId);
        if (trigger is ProjectileActor missile) Assert.Same(thing, missile.Owner);
        var saved = SimSavegame.Write(sim); trigger.SetTargetPointer(null); SimSavegame.Apply(sim, saved);
        Assert.Equal(thing.Id, trigger.TargetId); Assert.Equal(saved, SimSavegame.Write(sim));
    }

    [Fact]
    public void CombinedFlagsAssignBothOriginalParticipants()
    {
        var sim = Room(); var thing = sim.AddBot(0, 0); var trigger = sim.Players.Single(); thing.ActivationType = 6;
        ActorSpecialActions.ActivateSpecial(sim, thing, trigger);
        Assert.Equal(trigger.Id, thing.TargetId); Assert.Equal(thing.Id, trigger.TargetId);
    }

    [Fact]
    public void NullTriggerClearsThingTargetWithoutRevealingFallback()
    {
        var sim = Room(); var thing = sim.AddBot(0, 0); thing.StoredTargetId = sim.Players.Single().Id;
        thing.SetTargetPointer(sim.Players.Single()); thing.ActivationType = 6;
        ActorSpecialActions.ActivateSpecial(sim, thing, null);
        Assert.Null(thing.TargetId); Assert.Null(thing.StoredTargetId);
    }

    [Fact]
    public void UnsetFlagsLeaveTargetsAlone()
    {
        var sim = Room(); var thing = sim.AddBot(0, 0); var trigger = sim.Players.Single();
        var old = sim.AddBot(300, 0, 3001); thing.SetTargetPointer(old); trigger.SetTargetPointer(old);
        ActorSpecialActions.ActivateSpecial(sim, thing, trigger);
        Assert.Equal(old.Id, thing.TargetId); Assert.Equal(old.Id, trigger.TargetId);
    }

    private static Actor Subject(AuthoritySimulation sim, int kind)
    {
        if (kind == 1) return sim.Players.Single();
        if (kind == 2) return sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        var actor = sim.AddBot(0, 0); if (kind == 0) actor.Brain = null;
        return actor;
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 500 }]
    });
}
