using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaisePermissionDimensionTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public void ExplicitRaisePermissionSeesLivingDimensionsEvenWithPositionBypass(int flags)
    {
        var sim = Room(); var corpse = Ready(sim); var raiser = new Observer(true);
        Assert.True(ActorRaiseActions.RaiseActor(raiser, corpse, flags));
        Assert.Equal(Fixed.FromInt(56), raiser.HeightSeen); Assert.Equal(Fixed.FromInt(20), raiser.RadiusSeen);
        Assert.True(raiser.SolidSeen);
    }

    [Fact]
    public void PermissionVetoRetainsNativePreparedDimensionsButDoesNotRevive()
    {
        var sim = Room(); var corpse = Ready(sim); sim.DrainSoundRequests();
        Assert.False(ActorRaiseActions.RaiseActor(new Observer(false), corpse));
        Assert.Equal(Fixed.FromInt(56), corpse.Height); Assert.Equal(Fixed.FromInt(20), corpse.Radius);
        Assert.True(corpse.Solid); Assert.True(corpse.IsDead); Assert.True(corpse.Corpse);
        Assert.Empty(sim.DrainSoundRequests());
    }

    [Fact]
    public void FailedPositionCheckRestoresCorpseDimensionsAndSkipsPermission()
    {
        var sim = Room(); var corpse = Ready(sim); sim.Ceilings[0] = 10; var raiser = new Observer(true);
        Assert.False(ActorRaiseActions.RaiseActor(raiser, corpse)); Assert.False(raiser.Called);
        Assert.Equal(Fixed.FromInt(14), corpse.Height); Assert.Equal(Fixed.FromInt(5), corpse.Radius); Assert.False(corpse.Solid);
    }

    [Fact]
    public void PermissionDimensionChangesSurviveRevive()
    {
        var sim = Room(); var corpse = Ready(sim); var raiser = new Observer(true, changeSize: true);
        Assert.True(ActorRaiseActions.RaiseActor(raiser, corpse));
        Assert.Equal(Fixed.FromInt(30), corpse.Height); Assert.Equal(Fixed.FromInt(7), corpse.Radius);
    }

    [Fact]
    public void ArchvilePermissionStillSeesRestoredCorpseDimensions()
    {
        var sim = Room(); var corpse = Ready(sim); var raiser = new Observer(false);
        Assert.False(ArchvileActions.TryRaise(sim, raiser, sim.Players.Single()));
        Assert.True(raiser.Called); Assert.Equal(Fixed.FromInt(14), raiser.HeightSeen);
        Assert.Equal(Fixed.FromInt(5), raiser.RadiusSeen); Assert.False(raiser.SolidSeen);
    }

    private sealed class Observer(bool allow, bool changeSize = false) : Actor
    {
        public bool Called { get; private set; }
        public Fixed HeightSeen { get; private set; }
        public Fixed RadiusSeen { get; private set; }
        public bool SolidSeen { get; private set; }
        public override bool CanResurrect(Actor? other, bool passive)
        {
            Called = true; HeightSeen = other!.Height; RadiusSeen = other.Radius; SolidSeen = other.Solid;
            if (changeSize) { other.Height = Fixed.FromInt(30); other.Radius = Fixed.FromInt(7); }
            return allow;
        }
    }

    private static Actor Ready(AuthoritySimulation sim)
    {
        var actor = sim.Actors.Single(a => a.IsMonster); actor.Health = 0;
        actor.States.Enter(actor, ActorStateMachine.Corpse);
        actor.Height = Fixed.FromInt(14); actor.Radius = Fixed.FromInt(5); actor.Solid = false; return actor;
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }]
    });
}
